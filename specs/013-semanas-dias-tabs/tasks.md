# Tasks: Configuracion Semanas y Dias

**Input**: Design documents from `/specs/013-semanas-dias-tabs/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend y frontend incluyen cobertura de unit/integration/component para historias impactadas.

**Organization**: Tareas agrupadas por historia de usuario para implementacion y validacion independiente.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estructura tecnica y puntos de integracion del nuevo flujo Semana > Dia.

- [X] T001 Crear carpeta de modulo de plan mensual en `src/backend/src/GymTracker.Application/MonthlyPlan/` para contratos y servicios
- [X] T002 Crear carpeta de controlador API de plan mensual en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/`
- [X] T003 [P] Crear carpeta frontend de plan mensual en `src/frontend/src/pages/routines/monthlyPlan/`
- [X] T004 [P] Crear carpeta de tests del modulo mensual en `src/frontend/tests/component/routines/monthlyPlan/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base backend/frontend obligatoria antes de implementar historias.

**⚠️ CRITICAL**: Ninguna historia puede comenzar hasta cerrar esta fase.

- [X] T005 Crear entidades de dominio `MonthlyPlan`, `PlanWeek` y `PlanDay` en `src/backend/src/GymTracker.Domain/Entities/MonthlyPlan/MonthlyPlan.cs`
- [X] T006 [P] Crear entidad de historico `HistoricalExerciseRecord` en `src/backend/src/GymTracker.Domain/Entities/MonthlyPlan/HistoricalExerciseRecord.cs`
- [X] T007 [P] Crear entidad de auditoria de migracion en `src/backend/src/GymTracker.Domain/Entities/MonthlyPlan/MigrationAudit.cs`
- [X] T008 Crear contratos DTO del modulo mensual en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanContracts.cs`
- [X] T009 [P] Crear interfaz repositorio `IMonthlyPlanRepository` en `src/backend/src/GymTracker.Application/MonthlyPlan/IMonthlyPlanRepository.cs`
- [X] T010 Implementar servicio base `MonthlyPlanService` con validacion ownership en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanService.cs`
- [X] T011 Implementar repositorio infraestructura del modulo en `src/backend/src/GymTracker.Infrastructure/MonthlyPlan/MonthlyPlanRepository.cs`
- [X] T059 Definir mapeos EF/SQL del modulo mensual en `src/backend/src/GymTracker.Infrastructure/Admin/AdminDbContext.cs`
- [X] T060 Crear migracion SQL del modulo mensual en `src/backend/src/GymTracker.Infrastructure/Migrations/*_AddMonthlyPlansModule.cs`
- [X] T061 [P] Verificar aplicacion automatica de migraciones SQL del modulo en `src/backend/src/GymTracker.Infrastructure/DatabaseMigrationExtensions.cs`
- [X] T012 Crear endpoint de migracion dura en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/MonthlyPlanMigrationController.cs`
- [X] T066 Implementar servicio de migracion dura Rutina/Sesion -> Semana/Dia en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanMigrationService.cs`
- [X] T013 Implementar endpoint CRUD base del plan mensual en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/MonthlyPlansController.cs`
- [X] T014 Actualizar registro DI del modulo mensual en `src/backend/src/GymTracker.Application/DependencyInjection.cs`
- [X] T015 [P] Actualizar registro de infraestructura del modulo en `src/backend/src/GymTracker.Infrastructure/DependencyInjection.cs`
- [X] T016 Actualizar contrato OpenAPI versionado del backend en `src/backend/src/GymTracker.Api/Swagger/OpenApiConfiguration.cs`
- [X] T017 [P] Alinear contrato funcional de la feature en `specs/013-semanas-dias-tabs/contracts/openapi.yaml`
- [X] T018 Implementar logging estructurado y correlation para operaciones del modulo en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/MonthlyPlansController.cs`
- [X] T019 [P] Agregar health checks del modulo mensual en `src/backend/src/GymTracker.Api/Extensions/HealthCheckExtensions.cs`
- [ ] T020 [P] Crear pruebas unitarias de reglas base (dias 1..7 y ownership) en `src/backend/tests/GymTracker.Application.UnitTests/Routines/MonthlyPlanRulesTests.cs`
- [ ] T021 [P] Crear pruebas de integracion de contrato/seguridad para endpoints mensuales en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlansAuthorizationIntegrationTests.cs`
- [ ] T067 [P] Crear prueba E2E de migracion con reconciliacion de conteos/auditoria en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlanMigrationIntegrationTests.cs`

**Checkpoint**: Fundacion completa, historias habilitadas.

---

## Phase 3: User Story 1 - Plan mensual por semanas (Priority: P1) 🎯 MVP

**Goal**: Mostrar y persistir la planificacion con 4 semanas fijas reemplazando la navegacion visible de Rutina/Sesion.

**Independent Test**: Desde `/routines`, el usuario propietario ve Semana 1..4, guarda plan y recupera los datos al recargar.

### Tests for User Story 1

- [ ] T022 [P] [US1] Crear prueba de integracion CRUD de plan mensual en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlansCrudIntegrationTests.cs`
- [ ] T023 [P] [US1] Crear prueba de componente de tabs semanales fijas en `src/frontend/tests/component/routines/monthlyPlan/WeeklyTabsPlanner.test.tsx`
- [ ] T024 [P] [US1] Crear prueba Redux de carga/guardado de plan mensual en `src/frontend/tests/redux/monthlyPlanReducer.test.ts`

### Implementation for User Story 1

- [X] T025 [P] [US1] Crear interfaz `IMonthlyPlan` en `src/frontend/src/interfaces/monthlyPlan/IMonthlyPlan.ts`
- [X] T026 [P] [US1] Crear interfaz `IWeekPlan` en `src/frontend/src/interfaces/monthlyPlan/IWeekPlan.ts`
- [X] T027 [P] [US1] Crear servicio API del plan mensual en `src/frontend/src/services/api/monthlyPlans/monthlyPlansApi.ts`
- [X] T028 [P] [US1] Crear estado Redux del modulo en `src/frontend/src/redux/states/monthlyPlan/monthlyPlanState.ts`
- [X] T029 [P] [US1] Crear acciones Redux del modulo en `src/frontend/src/redux/actions/monthlyPlan/monthlyPlanActions.ts`
- [X] T030 [P] [US1] Crear reducer Redux del modulo en `src/frontend/src/redux/reducers/monthlyPlan/monthlyPlanReducer.ts`
- [X] T031 [US1] Registrar reducer `monthlyPlan` en `src/frontend/src/redux/store.ts`
- [X] T032 [US1] Implementar componente tabs de semanas en `src/frontend/src/pages/routines/monthlyPlan/components/MonthlyWeekTabs.tsx`
- [X] T033 [US1] Integrar vista mensual en la pagina principal de rutinas en `src/frontend/src/pages/routines/RoutinesPage.tsx`
- [X] T034 [US1] Retirar secciones visibles legacy de rutina/sesion de la vista principal en `src/frontend/src/pages/routines/sessions/form/RoutineSessionsSection.tsx`
- [X] T068 [US1] Restringir uso de endpoints legacy de Rutina en el flujo mensual (sin romper otros flujos) mediante desacople de rutas/consumidores en `src/backend/src/GymTracker.Api/Controllers/Routines/RoutinesController.cs` y `src/frontend/src/pages/routines/RoutinesPage.tsx`
- [X] T069 [US1] Restringir uso de endpoints legacy de Sesion en el flujo mensual (sin romper otros flujos) mediante desacople de rutas/consumidores en `src/backend/src/GymTracker.Api/Controllers/Sessions/SessionsController.cs` y `src/frontend/src/pages/routines/RoutinesPage.tsx`

**Checkpoint**: US1 funcional y demostrable de forma independiente.

---

## Phase 4: User Story 2 - Configurar dias del mes y recorte (Priority: P1)

**Goal**: Configurar N global (1..7) y aplicar recorte confirmado fuera de rango preservando historico estadistico.

**Independent Test**: Con N=5 y ejercicios en Dia 5, reducir a N=3 recorta plan activo de dias 4/5 y mantiene historico.

### Tests for User Story 2

- [ ] T035 [P] [US2] Crear prueba unitaria de recorte y preservacion historica en `src/backend/tests/GymTracker.Application.UnitTests/Routines/MonthlyPlanTruncationRulesTests.cs`
- [ ] T036 [P] [US2] Crear prueba de integracion del endpoint `truncate-days` en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlanTruncationIntegrationTests.cs`
- [ ] T037 [P] [US2] Crear prueba de componente para selector de dias global y confirmacion en `src/frontend/tests/component/routines/monthlyPlan/MonthlyDayTabsConfig.test.tsx`

### Implementation for User Story 2

- [X] T038 [US2] Implementar operacion de recorte por dias en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanService.cs`
- [X] T039 [P] [US2] Persistir snapshots historicos de recorte en `src/backend/src/GymTracker.Infrastructure/MonthlyPlan/MonthlyPlanRepository.cs`
- [X] T040 [P] [US2] Exponer endpoint de recorte en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/MonthlyPlansController.cs`
- [X] T041 [US2] Implementar estado y acciones de `activeDays`/confirmacion en `src/frontend/src/redux/actions/monthlyPlan/monthlyPlanActions.ts`
- [X] T042 [US2] Implementar tabs de dias globales (Dia 1..N) en `src/frontend/src/pages/routines/monthlyPlan/components/MonthlyDayTabs.tsx`
- [X] T043 [US2] Integrar confirmacion de recorte y feedback vacio/error en `src/frontend/src/pages/routines/monthlyPlan/MonthlyPlanPage.tsx`
- [X] T044 [US2] Ajustar copia i18n para semanas/dias/recorte en `src/frontend/src/i18n/lenguage/es.ts` y `src/frontend/src/i18n/lenguage/en.ts`
- [X] T062 [US2] Implementar fallback de lectura/escritura preservando ultimo estado confirmado en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanService.cs`
- [X] T063 [US2] Exponer respuesta degradada y metadata operativa de fallback en `src/backend/src/GymTracker.Api/Controllers/MonthlyPlans/MonthlyPlansController.cs`
- [ ] T064 [P] [US2] Crear prueba de integracion de modo degradado en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlanDegradationIntegrationTests.cs`
- [X] T065 [US2] Implementar manejo UI de degradacion y estado confirmado en `src/frontend/src/pages/routines/monthlyPlan/MonthlyPlanPage.tsx`

**Checkpoint**: US2 funcional y validable sin depender de US3.

---

## Phase 5: User Story 3 - Configurar ejercicios dentro de cada dia (Priority: P2)

**Goal**: Gestionar ejercicios por contexto Semana > Dia, guardando y recuperando por combinacion exacta.

**Independent Test**: Agregar ejercicios en Semana 2 > Dia 3, navegar a otros tabs y volver confirmando persistencia contextual.

### Tests for User Story 3

- [ ] T045 [P] [US3] Crear prueba de integracion de asociacion ejercicio->SemanaDia en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlanDayExercisesIntegrationTests.cs`
- [ ] T046 [P] [US3] Crear prueba de componente de persistencia contextual de ejercicios en `src/frontend/tests/component/routines/monthlyPlan/DayExercisePlanner.test.tsx`
- [ ] T047 [P] [US3] Crear prueba Redux para mutaciones de ejercicios por semana/dia en `src/frontend/tests/redux/monthlyPlanDayExercisesReducer.test.ts`

### Implementation for User Story 3

- [X] T048 [US3] Extender contratos backend de ejercicios por dia en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanContracts.cs`
- [X] T049 [P] [US3] Implementar reglas de asociacion y orden de ejercicios por dia en `src/backend/src/GymTracker.Application/MonthlyPlan/MonthlyPlanService.cs`
- [X] T050 [P] [US3] Persistir coleccion de ejercicios por dia en `src/backend/src/GymTracker.Infrastructure/MonthlyPlan/MonthlyPlanRepository.cs`
- [X] T051 [US3] Implementar panel de ejercicios del dia en `src/frontend/src/pages/routines/monthlyPlan/components/DayExercisesPanel.tsx`
- [X] T052 [US3] Conectar seleccion de ejercicios existente al contexto Semana > Dia en `src/frontend/src/pages/routines/exercices/form/LinkExerciseDialog.tsx`
- [X] T053 [US3] Integrar panel de ejercicios en la vista mensual en `src/frontend/src/pages/routines/monthlyPlan/MonthlyPlanPage.tsx`
- [X] T054 [US3] Persistir y recargar ejercicios por contexto en `src/frontend/src/redux/actions/monthlyPlan/monthlyPlanActions.ts`

**Checkpoint**: Todas las historias operativas e independientes.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cierre de calidad, observabilidad y consistencia transversal.

- [X] T055 [P] Actualizar guia operativa de la feature para validar SC-005/SC-006/SC-009 en `specs/013-semanas-dias-tabs/quickstart.md`
- [X] T056 Endurecer validaciones y mapeo de errores API en `src/backend/src/GymTracker.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [ ] T057 [P] Verificar latencia p95 de endpoints mensuales en `src/backend/tests/GymTracker.Api.IntegrationTests/Routines/MonthlyPlansPerformanceIntegrationTests.cs`
- [ ] T058 [P] Ejecutar checklist final de regression UI ligado a estados vacio/error/autorizacion en `src/frontend/tests/component/routines/RoutinesPageStates.test.tsx`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 (Setup): puede iniciar de inmediato.
- Phase 2 (Foundational): depende de Phase 1 y bloquea todas las historias.
- Phase 3 (US1): depende de Phase 2.
- Phase 4 (US2): depende de Phase 2 y reutiliza modulo mensual introducido en US1.
- Phase 5 (US3): depende de Phase 2 y de los contratos de plan mensual de US1/US2.
- Phase 6 (Polish): depende de cierre de historias objetivo.

### User Story Dependencies

- US1 (P1): inicia al terminar Foundational.
- US2 (P1): inicia al terminar Foundational; comparte base con US1.
- US3 (P2): inicia al terminar Foundational, con integracion sobre estructura mensual ya expuesta.

### Within Each User Story

- Pruebas primero (fallando antes de implementar).
- Contratos/modelos antes de servicios.
- Servicios antes de endpoint/UI final.
- Integracion y estados UI al final de la historia.

## Parallel Opportunities

- Setup: T003 y T004 pueden ejecutarse en paralelo.
- Foundational: T006, T007, T009, T015, T017, T019, T020, T021, T061 y T067 pueden ejecutarse en paralelo.
- US1: T022-T024 en paralelo; T025-T030 en paralelo por archivo.
- US2: T035-T037 en paralelo; T039 y T040 en paralelo tras T038; T064 en paralelo tras T063.
- US3: T045-T047 en paralelo; T049 y T050 en paralelo tras T048.

---

## Parallel Example: User Story 1

```bash
# Pruebas US1 en paralelo
Task: "T022 [US1] MonthlyPlansCrudIntegrationTests.cs"
Task: "T023 [US1] WeeklyTabsPlanner.test.tsx"
Task: "T024 [US1] monthlyPlanReducer.test.ts"

# Base frontend US1 en paralelo
Task: "T025 [US1] IMonthlyPlan.ts"
Task: "T026 [US1] IWeekPlan.ts"
Task: "T027 [US1] monthlyPlansApi.ts"
Task: "T028 [US1] monthlyPlanState.ts"
Task: "T029 [US1] monthlyPlanActions.ts"
Task: "T030 [US1] monthlyPlanReducer.ts"
```

---

## Implementation Strategy

### MVP First (US1)

1. Completar Phase 1 y Phase 2.
2. Entregar US1 con tabs de 4 semanas + persistencia mensual.
3. Validar criterios independientes de US1.

### Incremental Delivery

1. US1: estructura mensual y reemplazo visual de Rutina/Sesion.
2. US2: configuracion global de dias y recorte con historico.
3. US3: gestion de ejercicios por Semana > Dia.
4. Polish: rendimiento, observabilidad y regresion.

### Parallel Team Strategy

1. Equipo A: backend foundational + migracion.
2. Equipo B: frontend monthly plan + Redux.
3. Equipo C: pruebas integracion/componentes por historia.

---

## Notes

- Todas las tareas incluyen rutas de archivo concretas.
- Marcador [P] indica archivos distintos y baja colision.
- IDs secuenciales T001..T069 (incluye extensiones T059..T069 para cierre de gaps) en orden de ejecucion.
- Cada historia mantiene criterio de prueba independiente.

