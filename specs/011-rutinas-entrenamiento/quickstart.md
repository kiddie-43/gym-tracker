# Quickstart: Pagina de Rutinas de Entrenamiento (011)

## Goal

Validar end-to-end la planificacion y ejecucion de entrenamiento con stepper full-screen, bloqueo de navegacion, recuperacion de estado y registros por usuario.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Variables de entorno Firebase y configuracion Wger para backend
- Dependencias instaladas (`npm run install:all`)

## Run Backend

1. `cd src/backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`

## Run Frontend

1. `cd src/frontend`
2. `npm install`
3. `npm run dev`

## Manual Validation (aligned to stories)

1. Vista tarjetas de rutinas (P1):
   - Verificar que cada tarjeta muestre titulo, fecha y cantidad de ejercicios.
2. Planificacion de sesiones (P1):
   - Crear sesion con dias validos.
   - Intentar reutilizar un dia en otra sesion activa de la misma rutina y validar bloqueo.
3. Ejercicios en sesion (P2):
   - Agregar ejercicio solo desde catalogo.
   - Quitar ejercicio desde tarjeta y confirmar desvinculacion logica.
4. Registro de entrenamiento (P1):
   - Entrar a ejercicio a pantalla completa.
   - Registrar kg/repeticiones.
   - Guardar nota por usuario.
   - Adjuntar foto/video.
5. Limite multimedia (P1):
   - Intentar adjuntar 6 archivos y validar mensaje de error.
6. Stepper y bloqueo (P1):
   - Pulsar "Iniciar entrenamiento".
   - Verificar bloqueo de navegacion externa.
   - Avanzar/retroceder solo dentro del arbol.
   - Pulsar "Cancelar entrenamiento" y validar navegacion normal.
7. Recuperacion de estado (P1):
   - Con bloqueo activo y en paso intermedio, hacer F5.
   - Cerrar y volver a abrir.
   - Verificar restauracion del ultimo nodo del arbol.

## Minimum Test Matrix

- Backend unit:
  - validaciones de rutina/sesion/dias.
  - validacion maximo 5 adjuntos.
  - reglas de visibilidad por usuario para notas/logs.
- Backend integration:
  - CRUD logico de rutinas/sesiones.
  - vinculo/desvinculo logico de ejercicios.
  - start/cancel/restore de `activeTrainingState`.
  - proteccion por Firebase Auth.
- Frontend tests:
  - tarjetas de rutinas + detalle.
  - stepper full-screen y restriccion de navegacion.
  - restauracion de estado tras recarga.
  - formulario de registro de entrenamiento con adjuntos.

## API Smoke Checklist

- `GET /api/routines`
- `POST /api/routines`
- `PATCH /api/routines/{routineId}`
- `DELETE /api/routines/{routineId}` (soft delete)
- `POST /api/routines/{routineId}/reactivate`
- `POST /api/routines/{routineId}/sessions`
- `POST /api/routines/{routineId}/sessions/{sessionId}/exercises`
- `DELETE /api/routines/{routineId}/sessions/{sessionId}/exercises/{exerciseId}` (soft unlink)
- `POST /api/training-flow/start`
- `POST /api/training-flow/cancel`
- `GET /api/training-flow/active`
- `PUT /api/training-flow/active`
- `POST /api/exercise-training-logs`
- `GET /api/exercise-training-logs/{logId}`

## Validation Results (2026-05-18)

- Backend build: `dotnet build src/GymTracker.Api/GymTracker.Api.csproj` -> OK.
- Backend unit focused: `dotnet test tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj --filter "ProgressComparisonServiceTests|PlannedSetRulesTests|RoutineSessionDayConflictTests"` -> OK.
- Backend integration focused: `dotnet test tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "RoutinesCrudIntegrationTests|RoutineSessionsIntegrationTests|SessionExercisesIntegrationTests|PlannedSetsCrudIntegrationTests|RoutinesConcurrencyIntegrationTests|TrainingFlowIntegrationTests|ExerciseTrainingLogsIntegrationTests|ProgressComparisonIntegrationTests|RoutinesTrainingContractsTests"` -> OK.
- Frontend build: `npm run build` -> OK.
- Frontend routines tests: `npm run test -- --run tests/component/routines` -> OK.

### Notes

- During API integration tests, ASP.NET emitted HTTPS redirection warnings in test host; tests still passed with HTTP client.

## Suggested Commands

1. `dotnet test src/backend/tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj`
2. `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "Routine|TrainingFlow|ExerciseTraining"`
3. `cd src/frontend; npm run test -- routines`
4. `npm run dev` (desde raiz) para levantar backend + frontend en paralelo
5. `npm run dev:health` (desde raiz) para validar backend en `/health` y frontend en puertos `5173/5174`
