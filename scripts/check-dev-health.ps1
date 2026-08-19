param(
    [string]$BackendUrl = "http://localhost:5092/health",
    [string[]]$FrontendUrls = @("http://localhost:5173", "http://localhost:5174")
)

$ErrorActionPreference = "Stop"

function Test-HttpStatus {
    param(
        [Parameter(Mandatory = $true)][string]$Url,
        [int]$TimeoutSeconds = 5
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec $TimeoutSeconds -UseBasicParsing
        return [PSCustomObject]@{
            Url = $Url
            Ok = ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300)
            StatusCode = $response.StatusCode
            Message = "OK"
        }
    }
    catch {
        $statusCode = $null
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [PSCustomObject]@{
            Url = $Url
            Ok = $false
            StatusCode = $statusCode
            Message = $_.Exception.Message
        }
    }
}

Write-Host "Checking backend health..." -ForegroundColor Cyan
$backend = Test-HttpStatus -Url $BackendUrl

if ($backend.Ok) {
    Write-Host "[OK] Backend" -ForegroundColor Green
    Write-Host "  URL: $($backend.Url)"
    Write-Host "  Status: $($backend.StatusCode)"
}
else {
    Write-Host "[FAIL] Backend" -ForegroundColor Red
    Write-Host "  URL: $($backend.Url)"
    Write-Host "  Error: $($backend.Message)"
}

Write-Host "`nChecking frontend health..." -ForegroundColor Cyan
$frontendResults = @()
foreach ($url in $FrontendUrls) {
    $result = Test-HttpStatus -Url $url
    $frontendResults += $result

    if ($result.Ok) {
        Write-Host "[OK] Frontend" -ForegroundColor Green
        Write-Host "  URL: $($result.Url)"
        Write-Host "  Status: $($result.StatusCode)"
        break
    }
}

if (-not ($frontendResults | Where-Object { $_.Ok })) {
    $firstError = $frontendResults | Select-Object -First 1
    Write-Host "[FAIL] Frontend" -ForegroundColor Red
    Write-Host "  Tried: $($FrontendUrls -join ', ')"
    Write-Host "  Error: $($firstError.Message)"
}

$allOk = $backend.Ok -and ($frontendResults | Where-Object { $_.Ok } | Measure-Object).Count -gt 0
if ($allOk) {
    Write-Host "`nDevelopment environment is healthy." -ForegroundColor Green
    exit 0
}

Write-Host "`nDevelopment environment is not healthy." -ForegroundColor Yellow
exit 1
