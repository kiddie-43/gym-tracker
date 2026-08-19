# Tasks: Logs Metricos Dinamicos de Entrenamiento

**Input**: Design documents from `/specs/012-logs-metricas-dinamicas/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/openapi.yaml`

**Tests**: Backend changes include unit/integration coverage; frontend changes include reducer/component flow coverage.

**Terminologia Canonica**: `idGrupo` es termino de dominio; `groupId` se usa solo como alias contractual/DTO cuando aplique.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estructura de trabajo para backend y frontend del modulo.

- [X] T001 Crear estructura base del modulo backend en `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogs/`, `src/backend/src/GymTracker.Application/Contracts/Training/`, `src/backend/src/GymTracker.Domain/Entities/Training/`, y `src/backend/src/GymTracker.Infrastructure/Sql/`.
- [X] T002 Crear estructura base del modulo frontend en `src/frontend/src/interfaces/routines/trainingMetricLogs/`, `src/frontend/src/redux/trainingMetricLogs/`, `src/frontend/src/services/trainingMetricLogs/`, y `src/frontend/src/pages/routines/exercices/detail/`.
- [X] T003 [P] Preparar y alinear borrador inicial del contrato OpenAPI de feature con endpoints finales en `specs/012-logs-metricas-dinamicas/contracts/openapi.yaml`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base tecnica obligatoria antes de implementar historias.

**CRITICAL**: Ninguna historia debe empezar antes de completar esta fase.

- [X] T004 Implementar entidades de dominio `TrainingMetricLog` y `MetricLogChangeEvent` en `src/backend/src/GymTracker.Domain/Entities/Training/TrainingMetricLog.cs`.
- [X] T005 [P] Definir contrato de persistencia `ITrainingMetricLogRepository` en `src/backend/src/GymTracker.Application/Interfaces/Persistence/ITrainingMetricLogRepository.cs`.
- [X] T006 Implementar esquema SQL (tablas, indices, soft delete, unicidad de idGrupo) en `src/backend/src/GymTracker.Infrastructure/Sql/SqlRoutinesSchemaInitializer.cs`.
- [X] T007 Implementar repositorio SQL base para CRUD y consulta diaria en `src/backend/src/GymTracker.Infrastructure/Sql/SqlTrainingMetricLogRepository.cs`.
- [X] T008 Registrar dependencias de repositorio/handlers del modulo en `src/backend/src/GymTracker.Api/Program.cs`.
- [X] T009 Implementar reglas de validacion transversal (solo valor editable, borrado por grupo, ownership) en `src/backend/src/GymTracker.Application/Validation/TrainingMetricLogValidationRules.cs`.
- [X] T010 [P] Añadir DTOs base de modulo (request/response comunes) en `src/backend/src/GymTracker.Application/Contracts/Training/TrainingMetricLogDtos.cs`.

**Checkpoint**: Base lista; historias pueden implementarse.

---

## Phase 3: User Story 1 - Gestionar logs metricos dinamicos (Priority: P1) MVP

**Goal**: Crear, consultar, editar valor y borrar logicamente por grupo en backend con trazabilidad.

**Independent Test**: Se valida creando grupo de logs, actualizando un valor, consultando el grupo y eliminandolo logicamente, verificando que no aparece en listados activos.

### Tests for User Story 1

- [X] T011 [P] [US1] Crear pruebas unitarias de reglas de negocio (unicidad grupo, solo valor editable, borrado logico grupo) en `src/backend/tests/GymTracker.Application.UnitTests/Training/TrainingMetricLogRulesTests.cs`.
- [X] T012 [P] [US1] Crear pruebas de integracion CRUD + ownership del modulo en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs`.

### Implementation for User Story 1

- [X] T013 [US1] Implementar handler de casos de uso CRUD del grupo en `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`.
- [X] T014 [US1] Implementar controlador REST para create/getGroup/patchValue/deleteGroup en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs`.
- [X] T015 [US1] Persistir eventos de trazabilidad (create/update_value/soft_delete_group) en `src/backend/src/GymTracker.Infrastructure/Sql/SqlTrainingMetricLogRepository.cs`.
- [X] T016 [US1] Rechazar explicitamente borrado parcial por metrica y mutacion de campos inmutables en `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`.

**Checkpoint**: US1 funcional y testeable de forma independiente.

---

## Phase 4: User Story 2 - Detalle de logs con Redux y popups (Priority: P2)

**Goal**: Gestionar detalle de logs desde frontend con estado global y componentes compartidos.

**Independent Test**: Se valida que desde la pagina de detalle se puede abrir popup crear/editar/eliminar desde ActionMenu y que el listado refresca por Redux.

### Tests for User Story 2

- [X] T017 [P] [US2] Crear pruebas de reducer/actions/thunks del modulo en `src/frontend/tests/redux/trainingMetricLogsReducer.test.ts`.
- [X] T018 [P] [US2] Crear prueba de flujo UI ActionMenu -> PopupDialog -> refresh lista y validacion de estados mediante `FeedbackMessage` en `src/frontend/tests/component/routines/exercices/detail/TrainingMetricLogsDetailPage.test.tsx`.

### Implementation for User Story 2

- [X] T019 [US2] Crear interfaz unica del modulo en `src/frontend/src/interfaces/routines/trainingMetricLogs/TrainingMetricLog.ts`.
- [X] T020 [US2] Implementar estado Redux (state/actions/reducer/thunks) en `src/frontend/src/redux/trainingMetricLogs/`.
- [X] T021 [US2] Implementar servicio API del modulo en `src/frontend/src/services/trainingMetricLogs/trainingMetricLogsService.ts`.
- [X] T022 [US2] Implementar pagina detalle en `src/frontend/src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage.tsx`, usando `FeedbackMessage` para estados de carga/error/vacio.
- [X] T023 [US2] Integrar uso de componentes compartidos PopupDialog y ActionMenu en `src/frontend/src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage.tsx`.

**Checkpoint**: US2 funcional y testeable de forma independiente.

---

## Phase 5: User Story 4 - Regla diaria por fecha actual (Priority: P2)

**Goal**: Resolver "actual" por fecha actual y mayor CreatedAt, devolviendo [] sin datos.

**Independent Test**: Se valida que `current-day` devuelve el grupo vigente correcto y responde `[]` cuando no hay registros del dia.

### Tests for User Story 4

- [X] T024 [P] [US4] Crear pruebas unitarias de seleccion diaria (fecha actual + mayor CreatedAt) en `src/backend/tests/GymTracker.Application.UnitTests/Training/TrainingMetricLogsCurrentDayRulesTests.cs`.
- [X] T025 [P] [US4] Crear pruebas de integracion para endpoint current-day con caso vacio `[]` en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCurrentDayIntegrationTests.cs`.

### Implementation for User Story 4

- [X] T026 [US4] Implementar consulta de current-day con desempate por CreatedAt en `src/backend/src/GymTracker.Infrastructure/Sql/SqlTrainingMetricLogRepository.cs`.
- [X] T027 [US4] Implementar caso de uso current-day devolviendo lista vacia sin error en `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`.
- [X] T028 [US4] Exponer endpoint `GET /api/training-metric-logs/current-day` en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs`.
- [X] T029 [US4] Consumir current-day y manejar `[]` como estado vacio en `src/frontend/src/redux/trainingMetricLogs/thunks.ts` y `src/frontend/src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage.tsx`.

**Checkpoint**: US4 funcional y testeable de forma independiente.

---

## Phase 6: User Story 3 - Render dinamico de metricas en detalle (Priority: P3)

**Goal**: Mostrar un div por nombre de metrica en detalle de ejercicio.

**Independent Test**: Se valida render de un bloque por metrica y estado vacio cuando no existen metricas.

### Tests for User Story 3

- [X] T030 [P] [US3] Crear prueba de render dinamico de bloques por metrica en `src/frontend/tests/component/routines/exercices/detail/MetricNameBlocks.test.tsx`.

### Implementation for User Story 3

- [X] T031 [US3] Implementar endpoint de metricas habilitadas por ejercicio en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs` y `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`.
- [X] T032 [US3] Implementar consumo de metricas habilitadas en `src/frontend/src/services/trainingMetricLogs/trainingMetricLogsService.ts`.
- [X] T033 [US3] Implementar componente de bloques de nombres de metricas en `src/frontend/src/pages/routines/exercices/detail/components/MetricNameBlocks.tsx`.
- [X] T034 [US3] Integrar `MetricNameBlocks` en la pagina detalle en `src/frontend/src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage.tsx`.

**Checkpoint**: US3 funcional y testeable de forma independiente.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Endurecimiento final, observabilidad y validacion integral.

- [X] T035 [P] Ajustar logging estructurado y correlacion de operaciones del modulo en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs` y `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`.
- [X] T036 [P] Actualizar guia de validacion manual con resultados finales en `specs/012-logs-metricas-dinamicas/quickstart.md`.
- [X] T037 [P] Sincronizar contrato OpenAPI final con implementacion en `specs/012-logs-metricas-dinamicas/contracts/openapi.yaml`.
- [X] T038 Ejecutar y documentar pase de pruebas objetivo del modulo en `specs/012-logs-metricas-dinamicas/quickstart.md`.
- [X] T039 [P] Implementar health checks de dependencias criticas (SQL y dependencia de definiciones de metricas) en `src/backend/src/GymTracker.Api/Program.cs` y `src/backend/src/GymTracker.Infrastructure/Sql/`.
- [X] T040 [P] Crear pruebas de integracion de health checks y estados degradados en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsHealthChecksIntegrationTests.cs`.
- [X] T041 [P] Ejecutar prueba de rendimiento para validar objetivo p95 < 2s en consultas de listado/current-day y documentar evidencia en `specs/012-logs-metricas-dinamicas/quickstart.md`.
- [X] T042 Implementar degradacion controlada cuando no se puedan obtener definiciones de metricas, devolviendo `200 OK` + header `X-Metric-Definitions-Status=degraded` + lista vacia en backend y fallback consistente en frontend, en `src/backend/src/GymTracker.Application/Features/Training/TrainingMetricLogHandlers.cs`, `src/frontend/src/services/trainingMetricLogs/trainingMetricLogsService.ts` y `src/frontend/src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage.tsx`.
- [X] T043 [P] Crear pruebas frontend/backend para fallback de definiciones de metricas sin perdida de integridad de logs, verificando `200 OK` + header `X-Metric-Definitions-Status=degraded` + lista vacia y comportamiento UI asociado, en `src/frontend/tests/component/routines/exercices/detail/TrainingMetricLogsDetailPage.test.tsx` y `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs`.
- [X] T044 [P] Añadir pruebas de contrato para asegurar que `idGrupo` no sea editable ni expuesto como campo mutable en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs` y `specs/012-logs-metricas-dinamicas/contracts/openapi.yaml`.
- [X] T045 Definir versionado de endpoints y plan de migracion/sunset de legacy en `specs/012-logs-metricas-dinamicas/contracts/openapi.yaml` y `specs/012-logs-metricas-dinamicas/quickstart.md`.
- [X] T046 [P] Implementar validacion de retiro definitivo del flujo legacy y pruebas que aseguren que `/api/training-metric-logs/*` es la unica superficie soportada tras desplegar el contrato `2.0.0`, en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs` y documentacion relacionada.
- [X] T047 [P] Añadir pruebas de integracion para garantizar resolucion por fecha actual + mayor CreatedAt en operaciones mutables (create/update/delete) en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs`.
- [X] T048 [P] Implementar y probar estados loading/error/vacio en vista dinamica de metricas (`MetricNameBlocks`) usando `FeedbackMessage` en `src/frontend/src/pages/routines/exercices/detail/components/MetricNameBlocks.tsx` y `src/frontend/tests/component/routines/exercices/detail/MetricNameBlocks.test.tsx`.
- [X] T049 [P] Implementar paginacion en endpoint/listado de logs metricos (query params page/pageSize y metadatos de respuesta) en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs` y `src/backend/src/GymTracker.Infrastructure/Sql/SqlTrainingMetricLogRepository.cs`.
- [X] T050 [P] Crear pruebas de integracion para paginacion y orden estable en listados de logs metricos en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs`.
- [X] T051 [P] Actualizar consumo frontend del listado paginado y pruebas de estado asociado en `src/frontend/src/redux/trainingMetricLogs/thunks.ts` y `src/frontend/tests/redux/trainingMetricLogsReducer.test.ts`.
- [X] T052 Implementar modo temporal de autenticacion de desarrollo (identidad simulada) solo para entorno `Development`, con logging de activacion al inicio y alineado a FR-032, en `src/backend/src/GymTracker.Api/Program.cs` y `src/backend/src/GymTracker.Api/Middleware/`.
- [X] T053 [P] Crear prueba de seguridad que verifique bloqueo del modo de autenticacion simulada en `Staging` y `Production` en `src/backend/tests/GymTracker.Api.IntegrationTests/Security/DevelopmentAuthModeSecurityTests.cs`.
- [X] T054 [P] Implementar filtros por contexto (idRutina, idSesion, idEntrenamiento, idMetrica) en endpoint/listado de logs metricos en `src/backend/src/GymTracker.Api/Controllers/Training/TrainingMetricLogsController.cs` y `src/backend/src/GymTracker.Infrastructure/Sql/SqlTrainingMetricLogRepository.cs`.
- [X] T055 [P] Crear pruebas de integracion para filtros por contexto en listados de logs metricos en `src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingMetricLogsCrudIntegrationTests.cs`.
- [X] T056 [P] Documentar ADR del rediseño del modelo y la estrategia de sunset en `docs/adr/ADR-012-logs-metricos-dinamicos.md`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: inicia inmediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1; bloquea todas las historias.
- **Phase 3 (US1)**: depende de Phase 2.
- **Phase 4 (US2)**: depende de Phase 3 (API base disponible).
- **Phase 5 (US4)**: depende de Phase 3 (base CRUD disponible).
- **Phase 6 (US3)**: depende de Phase 3 y puede avanzar en paralelo con US2/US4.
- **Phase 7 (Polish)**: depende de historias objetivo completadas.

### User Story Dependencies

- **US1 (P1)**: independiente tras foundational; define MVP backend.
- **US2 (P2)**: depende de endpoints de US1.
- **US4 (P2)**: depende de base de US1; independiente de US2.
- **US3 (P3)**: depende de endpoints de metricas y pagina detalle base.

### Within Each User Story

- Pruebas primero (deben fallar antes de implementar).
- Contratos/DTOs antes de handlers.
- Handlers antes de controladores.
- Servicios frontend antes de pagina.

## Parallel Opportunities

- T003 con T001/T002.
- T005 y T010 en paralelo dentro de Phase 2.
- T011 y T012 en paralelo (US1 tests).
- T017 y T018 en paralelo (US2 tests).
- T024 y T025 en paralelo (US4 tests).
- T030 en paralelo con T031 inicial (US3).
- T035, T036 y T037 en paralelo en polish.
- T039, T040 y T041 en paralelo en polish.
- T043 y T044 en paralelo despues de T042.
- T046 en paralelo con cierre documental de T045 una vez definido el versionado.
- T047 y T048 en paralelo en cierre de polish.
- T049, T050 y T051 en paralelo durante cierre de hardening de listados.
- T052 y T053 en paralelo con cierre de hardening de seguridad.
- T054 y T055 en paralelo con hardening de listados.

## Parallel Example: User Story 2

```bash
# Ejecutar en paralelo pruebas de estado y flujo UI
T017: src/frontend/tests/redux/trainingMetricLogsReducer.test.ts
T018: src/frontend/tests/component/routines/exercices/detail/TrainingMetricLogsDetailPage.test.tsx

# Implementacion paralela inicial
T020: src/frontend/src/redux/trainingMetricLogs/
T021: src/frontend/src/services/trainingMetricLogs/trainingMetricLogsService.ts
```

## Implementation Strategy

### MVP First (US1)

1. Completar Phase 1 + Phase 2.
2. Completar US1 (Phase 3).
3. Validar CRUD backend + invariantes (solo valor editable, borrado por grupo).

### Incremental Delivery

1. MVP backend (US1).
2. UX operativa con Redux/popup/menu (US2).
3. Regla diaria (US4).
4. Render dinamico base de metricas (US3).
5. Polish y pruebas finales.

### Team Parallel Strategy

1. Equipo A: backend core (T013-T016, T026-T028).
2. Equipo B: frontend estado y vista (T019-T023, T029, T033-T034).
3. Equipo C: contratos/pruebas/hardening (T011-T012, T024-T025, T030, T035-T055).
