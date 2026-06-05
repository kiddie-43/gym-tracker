# Quickstart: Logs Metricos Dinamicos de Entrenamiento (012)

## Goal

Validar end-to-end el nuevo modelo de logs metricos dinamicos con `idGrupo`, edicion solo de valor, y borrado logico por grupo.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Configuracion de backend + frontend del workspace

## Run Backend

1. `cd src/backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`

## Run Frontend

1. `cd src/frontend`
2. `npm install`
3. `npm run dev`

## Manual Validation

1. Consultar metricas disponibles por ejercicio:
   - Debe devolver metadatos de metricas habilitadas.
   - Si la fuente de definiciones falla, debe devolver `200 OK`, header `X-Metric-Definitions-Status=degraded` y lista vacia.
2. Crear grupo de logs del dia:
   - Crear un set con multiples metricas.
   - Verificar que comparten `idGrupo`.
3. Editar valor de metrica:
   - Editar una metrica del `idGrupo`.
   - Verificar que solo cambia `valorMetrica`.
4. Aislamiento de grupo:
   - Tener grupos 1, 2 y 3 en el dia.
   - Editar grupo 1 y verificar que grupos 2 y 3 no cambian.
5. Borrado logico por grupo:
   - Eliminar por `idGrupo`.
   - Verificar que todo el grupo desaparece del listado activo.
   - Verificar traza de evento de borrado.
6. Consulta diaria sin datos:
   - Para una fecha/contexto sin registros, verificar `[]`.
   - Si existen varios grupos para el mismo dia/contexto, verificar que se resuelve el ultimo por `createdAt`.
7. Frontend Redux + componentes compartidos:
   - Abrir popup crear/editar/eliminar desde menu de acciones.
   - Confirmar que flujo y refresh dependen de estado Redux.
8. Health checks:
   - Consultar `/health`.
   - Verificar respuesta JSON con `status` global y entrada `training-metric-logs-dependencies`.
   - Verificar `503` solo cuando dependencias SQL criticas no estan disponibles.

## Legacy Migration

- El flujo legacy de `exercise logs` queda sustituido por el contrato `2.0.0` de `/api/training-metric-logs/*`.
- Toda integracion nueva debe consumir exclusivamente `/api/training-metric-logs/*`.
- La migracion debe completarse antes del despliegue de la version contractual `2.0.0`; no existe ventana de compatibilidad dentro del mismo despliegue.
- Para validar retiro efectivo de superficie legacy del modulo dinamico:
   - `GET /api/exercise-training-logs/groups/{groupId}` debe devolver `404`.
   - `PATCH /api/exercise-training-logs/groups/{groupId}/metrics/{metricId}` debe devolver `404`.
   - `DELETE /api/exercise-training-logs/groups/{groupId}` debe devolver `404`.

## Suggested Test Scope

- Backend unit:
  - unicidad de `idGrupo`
  - validacion de edicion solo `valorMetrica`
  - politica de borrado logico por grupo
  - regla de consulta diaria por `CreatedAt`
- Backend integration:
  - create/get/update/delete de logs metricos
  - retorno `[]` sin datos diarios
  - seguridad por usuario autenticado
- Frontend tests:
  - reducer/actions del modulo logs metricos
  - pagina detalle logs + popups + action menu
  - render dinamico de metricas con div por nombre

## API Smoke Checklist

- `GET /api/training-metric-logs/metrics`
- `GET /api/training-metric-logs/current-day`
- `POST /api/training-metric-logs`
- `PATCH /api/training-metric-logs/groups/{groupId}/metrics/{metricId}`
- `DELETE /api/training-metric-logs/groups/{groupId}`
- `GET /api/training-metric-logs/groups/{groupId}`
- `GET /health`

## Validated Results

- `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "FullyQualifiedName~TrainingMetricLogsHealthChecksIntegrationTests|FullyQualifiedName~TrainingMetricLogsCrudIntegrationTests|FullyQualifiedName~TrainingMetricLogsCurrentDayIntegrationTests" --nologo`
- Resultado validado: 7 pruebas, 7 correctas, 0 fallos.
- `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "FullyQualifiedName~TrainingMetricLogsHealthChecksIntegrationTests|FullyQualifiedName~DevelopmentAuthModeSecurityTests" --nologo`
- Resultado validado: 5 pruebas, 5 correctas, 0 fallos.
- `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "FullyQualifiedName~TrainingMetricLogsCrudIntegrationTests|FullyQualifiedName~TrainingMetricLogsPerformanceIntegrationTests" --nologo`
- Resultado validado: 9 pruebas, 9 correctas, 0 fallos.
- Evidencia de rendimiento (SC-002): `TrainingMetricLogsPerformanceIntegrationTests` valida `p95 < 2000 ms` para `GET /api/training-metric-logs` y `GET /api/training-metric-logs/current-day` bajo carga de 20 iteraciones por endpoint.
- `npm --prefix src/frontend run test -- tests/component/routines/exercices/detail/MetricNameBlocks.test.tsx tests/component/routines/exercices/detail/TrainingMetricLogsDetailPage.test.tsx tests/redux/trainingMetricLogsReducer.test.ts`
- Resultado validado: 14 pruebas, 14 correctas, 0 fallos.
