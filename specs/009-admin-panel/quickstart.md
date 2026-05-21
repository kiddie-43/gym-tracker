# Quickstart: Panel Admin Catalogos (009)

## Goal

Implementar y validar los paneles de administracion de Musculos, Mediciones y Ejercicios con soft delete, restauracion y CSV de musculos.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Firebase configurado para backend y frontend
- Dependencias del monorepo restauradas

## Run Backend

1. `cd src/backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`
5. Verificar Swagger en `/swagger`.

## Run Frontend

1. `cd src/frontend`
2. `npm install`
3. `npm run dev`
4. Abrir aplicacion en URL de Vite.

## Manual Validation

1. Ir a `/administracion`.
2. Panel Musculos:
   - Crear un musculo manualmente.
   - Eliminar (soft delete) y verificar que desaparece de la vista por defecto.
   - Activar check de borrados y reactivarlo.
3. Import CSV Musculos:
   - Cargar archivo con mezcla de filas validas/invalidas.
   - Verificar importacion parcial y reporte por fila.
4. Panel Mediciones:
   - Crear tipo con fields dinamicos.
   - Soft delete y restauracion con filtro.
5. Panel Ejercicios:
   - Crear ejercicio referenciando musculo y medicion no borrados.
   - Verificar que selectores no muestran borrados.
6. Conflicto de reactivacion:
   - Intentar reactivar registro cuyo `code` ya existe activo.
   - Verificar error de conflicto.

## Minimum Test Matrix

- Backend unit:
  - validacion unicidad activa para create/reactivar.
  - importacion parcial de CSV.
- Backend integration:
  - CRUD + soft delete + reactivar para 3 catalogos.
  - filtros con `includeDeleted`.
- Frontend component/integration:
  - check "ver borrados" y accion reactivar en tabla.
  - flujo de import CSV y visualizacion de resumen.
  - selectores de ejercicio excluyen borrados.

## API Smoke Checklist

- `GET /api/admin/muscles?includeDeleted=false`
- `POST /api/admin/muscles/import-csv`
- `POST /api/admin/muscles/{id}/reactivate`
- `GET /api/admin/measurement-types?includeDeleted=true`
- `POST /api/admin/measurement-types/{id}/reactivate`
- `GET /api/admin/exercises?includeDeleted=false`
- `POST /api/admin/exercises/{id}/reactivate`

## Backend Smoke Commands (PowerShell)

1. `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "ExercisesCanonicalEndpointsTests|ExercisesRelationsValidationTests|LegacyAdminRoutesCompatibilityTests"`
2. `dotnet test src/backend/tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj --filter "ExerciseRelationsValidatorTests|UniqueCodeValidatorTests"`
3. Legacy routes reemplazadas deben devolver `404`:
   - `GET /api/admin/exercise-types`
   - `GET /api/admin/exercise-form-types`
