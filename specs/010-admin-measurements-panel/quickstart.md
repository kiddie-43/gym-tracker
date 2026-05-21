# Quickstart: Admin Measurements Panel (010)

## Goal

Validar end-to-end el panel de administracion de mediciones con popup CRUD, importacion CSV parcial, y disponibilidad de tipos activos para asignacion a ejercicios.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Variables de entorno Firebase (Auth + Firestore) configuradas para backend/frontend
- Dependencias restauradas en `backend/` y `frontend/`

## Run Backend

1. `cd backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`
5. Abrir Swagger en `/swagger`.

## Run Frontend

1. `cd frontend`
2. `npm install`
3. `npm run dev`
4. Abrir la URL de Vite y navegar al panel de administracion.

## Manual Validation (aligned to stories)

1. CRUD popup (P1):
   - Abrir panel de mediciones.
   - Crear medicion con `key`, `name`, `unit`, `dataType`, `category`.
   - Editar desde el mismo popup.
   - Desactivar y validar que no aparece en la vista activa.
2. UX parity con musculos (P1):
   - Verificar botones visibles: `Anadir`, `Importar CSV`, `Refrescar`.
   - Verificar filtro de busqueda y toggle para incluir desactivados.
   - Reactivar una medicion desactivada desde la tabla.
3. CSV import (P1):
   - Importar archivo mixto (filas validas e invalidas).
   - Confirmar procesamiento parcial y resultado por fila.
   - Validar rechazo por `key` duplicada.
4. Assignment to exercises (P2):
   - Ir al formulario de ejercicio y abrir selector de mediciones.
   - Confirmar que aparecen solo tipos activos.
   - Desactivar una medicion y confirmar que deja de aparecer para nuevas asignaciones.

## QA Checklist (2026-05-18)

Terminologia oficial de estado:
- UI: "desactivado"
- Semantica tecnica: soft delete
- API query: `includeInactive=true` para incluir desactivados

Validaciones ejecutadas:
1. Seguridad admin de mediciones (401/403/200): `MeasurementTypesAuthorizationTests`
2. CRUD/reactivate de mediciones: `MeasurementTypesCrudTests`
3. Import CSV parcial por fila: `MeasurementTypesImportCsvTests`
4. Volumen CSV 200 filas con >=95% validas: `MeasurementTypesImportCsvVolumeTests`
5. Endpoint assignable solo activos: `MeasurementTypesAssignableTests`
6. Trazabilidad historica tras desactivar: `MeasurementTypesHistoricalTraceabilityTests`
7. Popup create/edit: `MeasurementTypesPanel.popup.test.tsx`
8. Estados loading/error/vacio: `MeasurementTypesPanel.states.test.tsx`
9. Dialogo CSV mediciones: `MeasurementTypesCsvImportDialog.test.tsx`
10. Selector de ejercicios usa assignable activo: `ExercisesPanel.measurementAssignable.test.tsx`

Resultado global de validacion:
- Backend unit/integration de mediciones: en verde
- Frontend tests de panel/importador/asignacion: en verde
- Contratos OpenAPI (feature + backend): sincronizados con `includeInactive`

## CSV Sample

```csv
key,name,unit,dataType,category,description
weight,Peso,kg,decimal,strength,Carga principal
duration,Duracion,min,time,cardio,Tiempo total
rpe,RPE,score,integer,general,Esfuerzo percibido
bad_type,Tipo Invalido,kg,float,strength,Debe fallar por dataType
```

## Minimum Test Matrix

- Backend unit:
  - validacion de enums (`dataType`, `category`)
  - unicidad de `key` en create/reactivate
  - parser/validador CSV por fila
- Backend integration:
  - CRUD + delete + reactivate de `measurementTypes`
  - import CSV parcial y respuesta detallada
  - endpoint `assignable` devuelve solo activos
- Frontend tests:
  - popup unificado create/edit
  - tabla con estados loading/error/empty
  - flujo de importacion y render de errores por fila
  - selector de ejercicios excluye desactivados

## API Smoke Checklist

- `GET /api/admin/measurement-types?includeInactive=false`
- `POST /api/admin/measurement-types`
- `PUT /api/admin/measurement-types/{id}`
- `DELETE /api/admin/measurement-types/{id}`
- `POST /api/admin/measurement-types/{id}/reactivate`
- `POST /api/admin/measurement-types/import-csv`
- `GET /api/admin/measurement-types/assignable`

## Suggested Test Commands

1. `dotnet test backend/tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj`
2. `dotnet test backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "Measurement|Admin"`
3. `cd frontend; npm run test -- measurements`