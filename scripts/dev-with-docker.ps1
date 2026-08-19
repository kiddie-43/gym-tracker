# Script to start Docker, apply migrations, and run dev environment

Write-Host "🐳 Iniciando Docker..." -ForegroundColor Yellow

function Test-DockerRunning {
    docker info 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

# Ensure the Docker daemon is running
if (-not (Test-DockerRunning)) {
    Write-Host "🐳 Docker no está en ejecución. Arrancando el daemon..." -ForegroundColor Yellow

    if ($IsLinux -or $IsMacOS) {
        # Servidor / CLI: sin Docker Desktop
        if (Get-Command systemctl -ErrorAction SilentlyContinue) {
            sudo systemctl start docker
        } elseif (Get-Command service -ErrorAction SilentlyContinue) {
            sudo service docker start
        } else {
            Write-Host "❌ No se encontró systemctl ni service para arrancar Docker." -ForegroundColor Red
            exit 1
        }
    } else {
        # Windows: arrancar el daemon por CLI (servicio, sin GUI). Pide elevación si hace falta.
        try {
            Start-Service com.docker.service -ErrorAction Stop
        } catch {
            Write-Host "🔐 Solicitando permisos de administrador para arrancar Docker..." -ForegroundColor Yellow
            try {
                Start-Process powershell -Verb RunAs -Wait -ArgumentList '-NoProfile', '-Command', 'Start-Service com.docker.service'
            } catch {
                Write-Host "❌ No se pudo arrancar el servicio 'com.docker.service' (permisos denegados)." -ForegroundColor Red
                exit 1
            }
        }
    }

    $maxWait = 90
    $waited = 0
    while (-not (Test-DockerRunning) -and $waited -lt $maxWait) {
        Start-Sleep -Seconds 3
        $waited += 3
        Write-Host "⏳ Esperando a que Docker esté listo... ($waited/$maxWait s)" -ForegroundColor Yellow
    }

    if (-not (Test-DockerRunning)) {
        Write-Host "❌ Docker no arrancó a tiempo. Verifica el daemon." -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Docker está listo" -ForegroundColor Green
}

# Kill leftover processes listening on dev ports (sin matar el propio npm/pwsh)
Write-Host "🔌 Limpiando puertos..." -ForegroundColor Gray
$devPorts = @(5092, 5173, 5174, 5175)
$currentPid = $PID
foreach ($port in $devPorts) {
    try {
        $owners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($ownerPid in $owners) {
            if ($ownerPid -and $ownerPid -ne 0 -and $ownerPid -ne $currentPid) {
                Stop-Process -Id $ownerPid -Force -ErrorAction SilentlyContinue
            }
        }
    } catch { }
}
Start-Sleep -Seconds 2

# Start docker-compose
docker-compose -f src/backend/docker-compose.sql.yml up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error iniciando Docker" -ForegroundColor Red
    exit 1
}

Write-Host "⏳ Esperando a que SQL Server esté listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

# Check SQL Server health
$maxRetries = 10
$retryCount = 0
$ready = $false

while ($retryCount -lt $maxRetries -and -not $ready) {
    try {
        docker exec gymtracker-sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "GymTracker_SaP4ss!" -Q "SELECT 1" 2>$null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            Write-Host "✅ SQL Server está listo" -ForegroundColor Green
        }
    } catch {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "⏳ Reintentando conexión a SQL Server... ($retryCount/$maxRetries)" -ForegroundColor Yellow
            Start-Sleep -Seconds 5
        }
    }
}

if (-not $ready) {
    Write-Host "❌ SQL Server no está disponible después de esperar" -ForegroundColor Red
    exit 1
}

# Apply migrations
Write-Host "📦 Aplicando migraciones..." -ForegroundColor Yellow
cd $PSScriptRoot\..

dotnet ef database update `
    --project src/backend/src/GymTracker.Infrastructure/GymTracker.Infrastructure.csproj `
    --startup-project src/backend/src/GymTracker.Api/GymTracker.Api.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error aplicando migraciones" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Migraciones aplicadas correctamente" -ForegroundColor Green
Write-Host "🚀 Iniciando ambiente de desarrollo..." -ForegroundColor Cyan

# Resumen de accesos (puertos Docker + Swagger)
Write-Host ""
Write-Host "🐳 Contenedores Docker:" -ForegroundColor Cyan
docker ps --filter "name=gymtracker" --format "   {{.Names}} -> {{.Ports}}"
Write-Host ""
Write-Host "🔗 URLs:" -ForegroundColor Cyan
Write-Host "   Swagger:  http://localhost:5092/swagger" -ForegroundColor Green
Write-Host "   API:      http://localhost:5092" -ForegroundColor Green
Write-Host "   Frontend: http://localhost:5173" -ForegroundColor Green
Write-Host ""

# Wait for socket state to settle before starting backend
Start-Sleep -Seconds 3

# Run dev environment (backend + frontend)
cd $PSScriptRoot\..
npm run dev:concurrent
