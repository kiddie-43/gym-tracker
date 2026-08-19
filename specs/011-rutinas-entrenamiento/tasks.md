# Tasks: Pagina de Rutinas de Entrenamiento

**Input**: Design documents from `/specs/011-rutinas-entrenamiento/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend y frontend requieren cobertura obligatoria por constitucion del proyecto.

**Organization**: Tareas agrupadas por historia de usuario para implementacion y validacion independiente.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estructura del modulo y esqueleto de endpoints/cliente API.

- [X] T001 Crear estructura base de backend para rutinas en src/backend/src/GymTracker.Domain/Entities/Routines/ y src/backend/src/GymTracker.Application/Features/Routines/
- [X] T002 Crear estructura base de frontend de rutinas en src/frontend/src/features/routines/ y src/frontend/src/pages/routines/
- [X] T003 [P] Crear contratos TypeScript iniciales para rutinas en src/frontend/src/interfaces/routines/routines.ts
- [X] T004 [P] Crear clientes API iniciales de rutinas y entrenamiento en src/frontend/src/services/api/routines/routinesApi.ts y src/frontend/src/services/api/routines/trainingFlowApi.ts

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura comun bloqueante para todas las historias.

**CRITICAL**: Ninguna historia puede comenzar antes de completar esta fase.

- [X] T005 Implementar entidades base Routine y RoutineSession en src/backend/src/GymTracker.Domain/Entities/Routines/Routine.cs y src/backend/src/GymTracker.Domain/Entities/Routines/RoutineSession.cs
- [X] T006 [P] Implementar entidades SessionExerciseLink y PlannedSet en src/backend/src/GymTracker.Domain/Entities/Routines/SessionExerciseLink.cs y src/backend/src/GymTracker.Domain/Entities/Routines/PlannedSet.cs
- [X] T007 [P] Implementar entidades ExerciseTrainingLog, TrainingAttachment y TrainingFlowState en src/backend/src/GymTracker.Domain/Entities/Training/ExerciseTrainingLog.cs y src/backend/src/GymTracker.Domain/Entities/Training/TrainingFlowState.cs
- [X] T008 Definir interfaces de repositorio de rutinas y entrenamiento en src/backend/src/GymTracker.Application/Interfaces/Persistence/IRoutineRepository.cs y src/backend/src/GymTracker.Application/Interfaces/Persistence/ITrainingFlowRepository.cs
- [X] T009 Implementar repositorios Firestore base para rutinas y training flow en src/backend/src/GymTracker.Infrastructure/Firebase/Repositories/RoutineRepository.cs y src/backend/src/GymTracker.Infrastructure/Firebase/Repositories/TrainingFlowRepository.cs
- [X] T010 Configurar validaciones transversales (borrado logico, ownership por usuario, max adjuntos) en src/backend/src/GymTracker.Application/Validation/RoutinesValidationRules.cs
- [X] T011 [P] Registrar servicios/repositorios en DI en src/backend/src/GymTracker.Api/Extensions/ServiceCollectionExtensions.cs
- [X] T012 [P] Definir mapeos DTO base de rutinas y entrenamiento en src/backend/src/GymTracker.Application/Contracts/Routines/RoutineDtos.cs y src/backend/src/GymTracker.Application/Contracts/Training/TrainingFlowDtos.cs
- [X] T013 Sincronizar contrato OpenAPI base de feature en specs/011-rutinas-entrenamiento/contracts/openapi.yaml

**Checkpoint**: Foundation completa, historias habilitadas.

---

## Phase 3: User Story 1 - Crear rutina personal (Priority: P1) 🎯 MVP

**Goal**: Crear, listar, editar, archivar y reactivar rutinas del usuario con aislamiento por propietario.

**Independent Test**: Crear una rutina, editarla, archivarla y reactivarla desde la vista de usuario verificando persistencia.

### Tests for User Story 1 (MANDATORY)

- [X] T014 [P] [US1] Crear unit tests de reglas de Routine (titulo requerido, soft delete/reactivate) en src/backend/tests/GymTracker.Application.UnitTests/Routines/RoutineRulesTests.cs
- [X] T015 [P] [US1] Crear integration tests de endpoints /api/routines y /api/routines/{id}/reactivate en src/backend/tests/GymTracker.Api.IntegrationTests/Routines/RoutinesCrudIntegrationTests.cs
- [X] T016 [P] [US1] Crear test de componente de tarjetas de rutinas (titulo, fecha, cantidad) en src/frontend/tests/component/routines/RoutinesCards.test.tsx

### Implementation for User Story 1

- [X] T017 [US1] Implementar casos de uso List/Create/Update/Delete/Reactivate de rutina en src/backend/src/GymTracker.Application/Features/Routines/RoutinesCommandHandlers.cs
- [X] T018 [US1] Implementar endpoints de rutinas en src/backend/src/GymTracker.Api/Endpoints/RoutinesEndpoints.cs
- [X] T019 [US1] Implementar estado Redux de rutinas (lista y formulario) en src/frontend/src/redux/states/routines/routinesState.ts
- [X] T020 [US1] Implementar acciones Redux de CRUD de rutinas en src/frontend/src/redux/actions/routines/routinesActions.ts
- [X] T021 [US1] Implementar pagina de rutinas en tarjetas en src/frontend/src/pages/routines/RoutinesPage.tsx
- [X] T022 [US1] Implementar dialogo crear/editar rutina con validacion de nombre en src/frontend/src/features/routines/components/RoutineFormDialog.tsx

**Checkpoint**: US1 funcional y validable de forma independiente.

---

## Phase 4: User Story 2 - Planificar sesiones por dias (Priority: P1)

**Goal**: Crear y editar sesiones por rutina con validacion de dias sin conflicto.

**Independent Test**: Crear sesiones en una rutina, asignar dias y validar bloqueo de dia duplicado en sesiones activas.

### Tests for User Story 2 (MANDATORY)

- [X] T023 [P] [US2] Crear unit tests de validacion de dias de sesion por rutina en src/backend/tests/GymTracker.Application.UnitTests/Routines/RoutineSessionDayConflictTests.cs
- [X] T024 [P] [US2] Crear integration tests de endpoint POST /api/routines/{routineId}/sessions en src/backend/tests/GymTracker.Api.IntegrationTests/Routines/RoutineSessionsIntegrationTests.cs
- [X] T025 [P] [US2] Crear test de componente de editor de sesiones por dia en src/frontend/tests/component/routines/RoutineSessionsPlanner.test.tsx

### Implementation for User Story 2

- [X] T026 [US2] Implementar casos de uso de sesiones (crear/actualizar/soft delete) en src/backend/src/GymTracker.Application/Features/Routines/RoutineSessionsCommandHandlers.cs
- [X] T027 [US2] Extender endpoints de sesiones en src/backend/src/GymTracker.Api/Endpoints/RoutinesEndpoints.cs
- [X] T028 [US2] Implementar seccion de sesiones en detalle de rutina en src/frontend/src/features/routines/components/RoutineSessionsSection.tsx
- [X] T029 [US2] Implementar selector de dias y validaciones de conflicto en src/frontend/src/features/routines/components/SessionDaysPicker.tsx
- [X] T030 [US2] Integrar gestion de sesiones en acciones/estado redux en src/frontend/src/redux/actions/routines/routinesActions.ts y src/frontend/src/redux/states/routines/routinesState.ts

**Checkpoint**: US2 funcional y validable de forma independiente.

---

## Phase 5: User Story 4 - Registrar entrenamiento por ejercicio (Priority: P1)

**Goal**: Registrar kg/repeticiones, notas y adjuntos por ejercicio y usuario desde vista full-screen.

**Independent Test**: En ejercicio seleccionado, guardar log con sets, nota y adjuntos; recuperar solo para usuario propietario.

### Tests for User Story 4 (MANDATORY)

- [X] T031 [P] [US4] Crear unit tests de ExerciseTrainingLog (valores positivos y max 5 adjuntos) en src/backend/tests/GymTracker.Application.UnitTests/Training/ExerciseTrainingLogRulesTests.cs
- [X] T032 [P] [US4] Crear integration tests de POST/GET /api/exercise-training-logs con ownership en src/backend/tests/GymTracker.Api.IntegrationTests/Training/ExerciseTrainingLogsIntegrationTests.cs
- [X] T033 [P] [US4] Crear test de formulario de datos de ejercicio con adjuntos y notas en src/frontend/tests/component/routines/ExerciseTrainingDataForm.test.tsx

### Implementation for User Story 4

- [X] T034 [US4] Implementar casos de uso de guardado/lectura de logs por ejercicio en src/backend/src/GymTracker.Application/Features/Training/ExerciseTrainingLogHandlers.cs
- [X] T035 [US4] Implementar endpoints de exercise training logs en src/backend/src/GymTracker.Api/Endpoints/ExerciseTrainingLogsEndpoints.cs
- [X] T036 [US4] Implementar vista full-screen de ejercicio para datos de entrenamiento en src/frontend/src/features/routines/components/ExerciseTrainingFullscreen.tsx
- [X] T037 [US4] Implementar formulario de sets/notas/adjuntos con limite 5 en src/frontend/src/features/routines/components/ExerciseTrainingDataForm.tsx
- [X] T038 [US4] Integrar APIs de training logs en redux/actions en src/frontend/src/redux/actions/routines/routinesActions.ts

**Checkpoint**: US4 funcional y validable de forma independiente.

---

## Phase 6: User Story 5 - Ejecutar entrenamiento guiado con bloqueo de navegacion (Priority: P1)

**Goal**: Stepper full-screen en misma ruta con bloqueo de navegacion y recuperacion de estado tras F5/reingreso.

**Independent Test**: Iniciar entrenamiento, navegar dentro del arbol, recargar/reingresar y restaurar ultimo estado; cancelar y recuperar navegacion normal.

### Tests for User Story 5 (MANDATORY)

- [X] T039 [P] [US5] Crear unit tests de TrainingFlowState (nodos validos y lock) en src/backend/tests/GymTracker.Application.UnitTests/Training/TrainingFlowStateRulesTests.cs
- [X] T040 [P] [US5] Crear integration tests de /api/training-flow/start|active|cancel en src/backend/tests/GymTracker.Api.IntegrationTests/Training/TrainingFlowIntegrationTests.cs
- [X] T041 [P] [US5] Crear test de flujo stepper con bloqueo de navegacion y restore en src/frontend/tests/component/routines/TrainingStepperFlow.test.tsx

### Implementation for User Story 5

- [X] T042 [US5] Implementar casos de uso start/cancel/update/get de training flow en src/backend/src/GymTracker.Application/Features/Training/TrainingFlowHandlers.cs
- [X] T043 [US5] Implementar endpoints training flow en src/backend/src/GymTracker.Api/Endpoints/TrainingFlowEndpoints.cs
- [X] T044 [US5] Implementar stepper full-screen sin cambio de ruta en src/frontend/src/features/routines/components/TrainingStepper.tsx
- [X] T045 [US5] Implementar guard de bloqueo de navegacion interna mientras trainingLocked=true en src/frontend/src/features/routines/guards/trainingNavigationGuard.ts
- [X] T046 [US5] Integrar persistencia/restauracion de estado activo en src/frontend/src/redux/actions/routines/routinesActions.ts y src/frontend/src/services/api/routines/trainingFlowApi.ts
- [X] T047 [US5] Implementar botones Iniciar/Cancelar entrenamiento en src/frontend/src/features/routines/components/TrainingFlowControls.tsx

**Checkpoint**: US5 funcional y validable de forma independiente.

---

## Phase 7: User Story 3 - Definir ejercicios y series (Priority: P2)

**Goal**: Configurar ejercicios de catalogo en sesiones y gestionar series planificadas.

**Independent Test**: Agregar ejercicio desde catalogo a una sesion, crear/editar series planificadas y desvincular ejercicio logicamente.

### Tests for User Story 3 (MANDATORY)

- [X] T048 [P] [US3] Crear integration tests de agregar/quitar ejercicio en sesion en src/backend/tests/GymTracker.Api.IntegrationTests/Routines/SessionExercisesIntegrationTests.cs
- [X] T049 [P] [US3] Crear unit tests de PlannedSet (repeticiones/carga positivas) en src/backend/tests/GymTracker.Application.UnitTests/Routines/PlannedSetRulesTests.cs
- [X] T050 [P] [US3] Crear test de selector de catalogo y panel de eliminacion de ejercicio en src/frontend/tests/component/routines/SessionExercisesCatalog.test.tsx

### Implementation for User Story 3

- [X] T051 [US3] Implementar casos de uso add/unlink de ejercicios de sesion en src/backend/src/GymTracker.Application/Features/Routines/SessionExercisesHandlers.cs
- [X] T052 [US3] Implementar validacion y persistencia de series planificadas en src/backend/src/GymTracker.Application/Features/Routines/PlannedSetsHandlers.cs
- [X] T053 [US3] Extender endpoint de sesion-ejercicios en src/backend/src/GymTracker.Api/Endpoints/RoutinesEndpoints.cs
- [X] T054 [US3] Implementar lista de ejercicios configurados y accion anadir en src/frontend/src/features/routines/components/RoutineSessionExercisesSection.tsx
- [X] T055 [US3] Implementar panel de eliminacion de ejercicio en tarjeta en src/frontend/src/features/routines/components/SessionExerciseCard.tsx
- [X] T056 [US3] Implementar editor de series planificadas en src/frontend/src/features/routines/components/PlannedSetsEditor.tsx

**Checkpoint**: US3 funcional y validable de forma independiente.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Cierre transversal, consistencia y validacion final de quickstart.

- [X] T057 [P] Actualizar documentacion tecnica de feature en specs/011-rutinas-entrenamiento/quickstart.md
- [X] T058 [P] Alinear contrato OpenAPI final con implementacion en specs/011-rutinas-entrenamiento/contracts/openapi.yaml
- [X] T059 Implementar logging estructurado y codigos de error consistentes en src/backend/src/GymTracker.Api/Middleware/ExceptionHandlingMiddleware.cs
- [X] T060 Ejecutar validacion end-to-end del quickstart y registrar resultados en specs/011-rutinas-entrenamiento/research.md

---

## Phase 9: Consistency Remediation (Post-Analysis)

**Purpose**: Cerrar hallazgos de consistencia entre spec/plan/tasks/contrato antes de implementacion completa.

- [X] T061 [P] [US1] Crear test de estados loading/error/empty de la vista principal de rutinas en src/frontend/tests/component/routines/RoutinesPageStates.test.tsx
- [X] T062 [P] Crear contract tests de endpoints criticos de rutinas/training flow/logs en src/backend/tests/GymTracker.Api.IntegrationTests/Contracts/RoutinesTrainingContractsTests.cs
- [X] T063 [US2] Implementar y probar politica last-write-wins para actualizacion concurrente de rutina en src/backend/src/GymTracker.Application/Features/Routines/RoutinesCommandHandlers.cs y src/backend/tests/GymTracker.Api.IntegrationTests/Routines/RoutinesConcurrencyIntegrationTests.cs
- [X] T064 [P] [US3] Implementar endpoint para editar serie planificada en src/backend/src/GymTracker.Api/Endpoints/RoutinesEndpoints.cs y prueba de integracion en src/backend/tests/GymTracker.Api.IntegrationTests/Routines/PlannedSetsCrudIntegrationTests.cs
- [X] T065 [P] [US3] Implementar endpoint para eliminar serie planificada en src/backend/src/GymTracker.Api/Endpoints/RoutinesEndpoints.cs y prueba de integracion en src/backend/tests/GymTracker.Api.IntegrationTests/Routines/PlannedSetsCrudIntegrationTests.cs
- [X] T066 [US4] Implementar calculo de comparativas de progreso (ultima sesion/promedio ultimas 4/mejor marca) en src/backend/src/GymTracker.Application/Features/Training/ProgressComparisonService.cs
- [X] T067 [P] [US4] Crear unit tests de metricas obligatorias (volumen, carga, repeticiones, variacion, tendencia) en src/backend/tests/GymTracker.Application.UnitTests/Training/ProgressComparisonServiceTests.cs
- [X] T068 [US4] Implementar endpoint GET /api/exercise-training-logs/progress-comparison en src/backend/src/GymTracker.Api/Endpoints/ExerciseTrainingLogsEndpoints.cs y prueba de integracion en src/backend/tests/GymTracker.Api.IntegrationTests/Training/ProgressComparisonIntegrationTests.cs
- [X] T069 [US4] Implementar visualizacion de comparativas en detalle de ejercicio en src/frontend/src/features/routines/components/ExerciseProgressComparisonPanel.tsx y test en src/frontend/tests/component/routines/ExerciseProgressComparisonPanel.test.tsx
- [X] T070 [US5] Implementar modo degradado de catalogo externo y mensaje visible al usuario en src/backend/src/GymTracker.Application/Features/Routines/ExerciseCatalogFallbackService.cs y src/frontend/src/features/routines/components/CatalogDegradedModeAlert.tsx

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 (Setup): sin dependencias.
- Phase 2 (Foundational): depende de Phase 1 y bloquea todas las historias.
- Phases 3-7 (User Stories): dependen de Phase 2.
- Phase 8 (Polish): depende de historias objetivo completadas.

### User Story Dependencies

- US1: inicia tras Phase 2.
- US2: inicia tras Phase 2 (se apoya en modelo de rutina de US1).
- US4: inicia tras Phase 2 (puede validarse con datos sembrados de rutina/sesion).
- US5: inicia tras Phase 2 (se integra con US1/US2/US4 para flujo completo).
- US3 (P2): inicia tras Phase 2; recomendado despues de US2 por dependencia funcional de sesiones.

### Within Each User Story

- Tests primero (fallando antes de implementar).
- Casos de uso/servicios antes que endpoints.
- Endpoints antes de integracion frontend.
- UI antes de ajustes de estado/acciones finales.

### Parallel Opportunities

- Setup con [P] en paralelo: T003, T004.
- Foundational con [P] en paralelo: T006, T007, T011, T012.
- Tests [P] de cada historia en paralelo dentro de la misma fase.
- Trabajo backend/frontend en paralelo cuando no comparten archivos.

---

## Parallel Example: User Story 5

```bash
# Tests en paralelo
T039 [US5] TrainingFlowStateRulesTests
T040 [US5] TrainingFlowIntegrationTests
T041 [US5] TrainingStepperFlow.test.tsx

# Implementacion paralela (sin conflictos de archivo)
T043 [US5] Endpoints backend training flow
T044 [US5] Componente frontend TrainingStepper
T047 [US5] Controles Iniciar/Cancelar
```

---

## Implementation Strategy

### MVP First

1. Completar Phase 1 y Phase 2.
2. Implementar US1.
3. Validar US1 de forma independiente.
4. Demo interna del MVP (tarjetas + CRUD logico de rutinas).

### Incremental Delivery

1. US1 (base de rutinas)
2. US2 (sesiones por dia)
3. US4 (registro por ejercicio)
4. US5 (flujo guiado y bloqueo/restore)
5. US3 (series planificadas y gestion completa de ejercicios de sesion)

### Team Parallel Strategy

1. Backend team: T017/T026/T034/T042/T051
2. Frontend team: T021/T028/T036/T044/T054
3. QA team: tareas de pruebas [P] por historia + T060

---

## Notes

- Todas las tareas cumplen formato checklist estricto con ID secuencial.
- Las tareas [USx] permiten trazabilidad directa a historias del spec.
- Se mantiene borrado logico obligatorio en todas las operaciones de eliminacion.
- No se incluye navegacion a otra ruta para el flujo de entrenamiento (stepper en misma vista).
