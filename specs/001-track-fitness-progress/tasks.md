---
description: "Task list for App de Seguimiento Fitness y Progreso"
---

# Tasks: App de Seguimiento Fitness y Progreso

**Input**: Design documents from `/specs/001-track-fitness-progress/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend changes MUST include unit/integration coverage; frontend changes MUST include component/flow coverage.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicializar la solución backend/frontend y la base de configuración común.

- [x] T001 Crear solución .NET y proyectos base en backend/GymTracker.sln, backend/src/GymTracker.Api/GymTracker.Api.csproj, backend/src/GymTracker.Application/GymTracker.Application.csproj, backend/src/GymTracker.Domain/GymTracker.Domain.csproj, backend/src/GymTracker.Infrastructure/GymTracker.Infrastructure.csproj
- [x] T002 Crear proyectos de pruebas backend en backend/tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj y backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj
- [x] T003 Crear scaffold frontend con React + TypeScript en frontend/package.json, frontend/tsconfig.json, frontend/vite.config.ts y frontend/src/main.tsx
- [x] T004 [P] Configurar análisis y formato backend en backend/Directory.Build.props
- [x] T005 [P] Configurar linting y testing frontend en frontend/eslint.config.js, frontend/vitest.config.ts y frontend/tests/component/setup.ts
- [x] T006 [P] Definir configuración local y variables de entorno en backend/src/GymTracker.Api/appsettings.Development.json, backend/src/GymTracker.Api/Options/FirebaseOptions.cs, backend/src/GymTracker.Api/Options/WgerOptions.cs y frontend/.env.example

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura base obligatoria antes de cualquier historia de usuario.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T007 Implementar bootstrap de API, DI, Swagger y health checks en backend/src/GymTracker.Api/Program.cs
- [x] T008 [P] Implementar middleware de autenticación/autorización con Firebase Auth en backend/src/GymTracker.Api/Middleware/FirebaseAuthMiddleware.cs y backend/src/GymTracker.Api/Middleware/CurrentUserContextMiddleware.cs
- [x] T009 [P] Implementar manejo uniforme de errores ProblemDetails y correlación por request en backend/src/GymTracker.Api/Middleware/ExceptionHandlingMiddleware.cs y backend/src/GymTracker.Infrastructure/Observability/CorrelationIdAccessor.cs
- [x] T010 [P] Implementar cliente Wger base y contratos de catálogo en backend/src/GymTracker.Infrastructure/Wger/WgerApiClient.cs, backend/src/GymTracker.Application/Catalog/ICatalogService.cs y backend/src/GymTracker.Application/Catalog/CatalogDtos.cs
- [x] T011 [P] Implementar caché híbrida de catálogo con memoria + Firestore en backend/src/GymTracker.Infrastructure/Caching/CatalogCacheService.cs y backend/src/GymTracker.Infrastructure/Firebase/CatalogCacheRepository.cs
- [x] T012 [P] Implementar acceso base a Firestore y repositorios comunes en backend/src/GymTracker.Infrastructure/Firebase/FirestoreContext.cs, backend/src/GymTracker.Infrastructure/Firebase/UserScopedRepository.cs y backend/src/GymTracker.Domain/Entities/BaseEntity.cs
- [x] T013 [P] Añadir contrato OpenAPI base y sincronización Swagger en backend/src/GymTracker.Api/Contracts/openapi.yaml y backend/src/GymTracker.Api/Swagger/OpenApiConfiguration.cs
- [x] T014 [P] Crear cliente HTTP tipado del frontend y shell de aplicación en frontend/src/shared/api/httpClient.ts, frontend/src/app/router.tsx, frontend/src/app/App.tsx y frontend/src/theme/theme.ts
- [x] T015 [P] Crear tipos compartidos del dominio para frontend en frontend/src/shared/types/catalog.ts, frontend/src/shared/types/workouts.ts, frontend/src/shared/types/routines.ts, frontend/src/shared/types/diets.ts y frontend/src/shared/types/settings.ts
- [x] T016 Implementar layout base con estados globales loading/error/empty en frontend/src/shared/components/AppLayout.tsx, frontend/src/shared/components/AsyncState.tsx y frontend/src/shared/components/PageHeader.tsx

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Registrar y comparar entrenamientos (Priority: P1) 🎯 MVP

**Goal**: Permitir registrar sesiones de entrenamiento y visualizar comparativas de progreso fiables por ejercicio.

**Independent Test**: Registrar dos sesiones del mismo ejercicio y comprobar comparativas contra última sesión, promedio últimas 4 y mejor marca reciente.

### Tests for User Story 1 (MANDATORY) ⚠️

- [x] T017 [P] [US1] Crear pruebas unitarias de cálculo de comparativas y sesiones incompletas en backend/tests/GymTracker.Application.UnitTests/Progress/ProgressCalculationServiceTests.cs
- [x] T018 [P] [US1] Crear pruebas de integración para POST /api/workouts y GET /api/progress/exercises/{exerciseId} en backend/tests/GymTracker.Api.IntegrationTests/Workouts/WorkoutEndpointsTests.cs y backend/tests/GymTracker.Api.IntegrationTests/Progress/ProgressEndpointsTests.cs
- [x] T019 [P] [US1] Crear pruebas de componentes para formulario de entrenamiento y panel de progreso en frontend/tests/component/workouts/WorkoutEntryForm.test.tsx y frontend/tests/component/progress/ProgressSummaryPanel.test.tsx

### Implementation for User Story 1

- [x] T020 [P] [US1] Implementar entidades de dominio Workout, ExerciseEntry y ProgressSnapshot en backend/src/GymTracker.Domain/Entities/Workout.cs, backend/src/GymTracker.Domain/Entities/ExerciseEntry.cs y backend/src/GymTracker.Domain/Entities/ProgressSnapshot.cs
- [x] T021 [P] [US1] Implementar repositorios de entrenamientos y snapshots en backend/src/GymTracker.Application/Workouts/IWorkoutRepository.cs, backend/src/GymTracker.Application/Progress/IProgressSnapshotRepository.cs, backend/src/GymTracker.Infrastructure/Firebase/WorkoutRepository.cs y backend/src/GymTracker.Infrastructure/Firebase/ProgressSnapshotRepository.cs
- [x] T022 [US1] Implementar servicio de dominio de comparativas en backend/src/GymTracker.Domain/Services/ProgressCalculationService.cs y backend/src/GymTracker.Application/Progress/ProgressService.cs
- [x] T023 [US1] Implementar servicio de aplicación para registrar entrenamientos en backend/src/GymTracker.Application/Workouts/WorkoutService.cs y backend/src/GymTracker.Application/Workouts/WorkoutValidators.cs
- [x] T024 [US1] Implementar endpoints de entrenamientos y progreso en backend/src/GymTracker.Api/Controllers/WorkoutsController.cs y backend/src/GymTracker.Api/Controllers/ProgressController.cs
- [x] T025 [US1] Sincronizar contrato OpenAPI de entrenamientos/progreso en backend/src/GymTracker.Api/Contracts/openapi.yaml y specs/001-track-fitness-progress/contracts/openapi.yaml
- [x] T026 [US1] Implementar páginas y hooks frontend para registrar entrenamientos y ver progreso en frontend/src/features/workouts/pages/WorkoutEntryPage.tsx, frontend/src/features/workouts/components/WorkoutEntryForm.tsx, frontend/src/features/workouts/api/workoutsApi.ts, frontend/src/features/progress/pages/ProgressOverviewPage.tsx y frontend/src/features/progress/components/ProgressSummaryPanel.tsx
- [x] T027 [US1] Implementar alertas de regresión y estados sin referencia en frontend/src/features/progress/components/TrendAlert.tsx y frontend/src/features/progress/utils/progressDisplay.ts

**Checkpoint**: User Story 1 should be fully functional and independently testable.

---

## Phase 4: User Story 2 - Crear y reutilizar rutinas (Priority: P2)

**Goal**: Permitir crear rutinas por día/etiqueta con selección de grupos musculares y ejercicios sugeridos desde catálogo.

**Independent Test**: Crear una rutina con grupos musculares y reutilizarla como base para un entrenamiento real.

### Tests for User Story 2 (MANDATORY when this story changes executable behavior) ⚠️

- [x] T028 [P] [US2] Crear pruebas unitarias de composición de rutinas y filtrado por grupos musculares en backend/tests/GymTracker.Application.UnitTests/Routines/RoutineServiceTests.cs
- [x] T029 [P] [US2] Crear pruebas de integración para GET /api/catalog/muscle-groups, GET /api/catalog/exercises y CRUD de /api/routines en backend/tests/GymTracker.Api.IntegrationTests/Catalog/CatalogEndpointsTests.cs y backend/tests/GymTracker.Api.IntegrationTests/Routines/RoutineEndpointsTests.cs
- [x] T030 [P] [US2] Crear pruebas de componentes para creador de rutinas y multiselector muscular en frontend/tests/component/routines/RoutineBuilderPage.test.tsx y frontend/tests/component/routines/MuscleGroupSelector.test.tsx

### Implementation for User Story 2

- [x] T031 [P] [US2] Implementar entidad Routine y modelos relacionados en backend/src/GymTracker.Domain/Entities/Routine.cs and backend/src/GymTracker.Domain/ValueObjects/PlannedExercise.cs
- [x] T032 [P] [US2] Implementar repositorio y validadores de rutinas en backend/src/GymTracker.Application/Routines/IRoutineRepository.cs, backend/src/GymTracker.Infrastructure/Firebase/RoutineRepository.cs y backend/src/GymTracker.Application/Routines/RoutineValidators.cs
- [x] T033 [US2] Implementar servicios de catálogo para grupos musculares y ejercicios sugeridos en backend/src/GymTracker.Application/Catalog/CatalogService.cs y backend/src/GymTracker.Infrastructure/Wger/WgerCatalogMapper.cs
- [x] T034 [US2] Implementar servicio de aplicación de rutinas y arranque desde rutina en backend/src/GymTracker.Application/Routines/RoutineService.cs
- [x] T035 [US2] Implementar endpoints de catálogo y rutinas en backend/src/GymTracker.Api/Controllers/CatalogController.cs y backend/src/GymTracker.Api/Controllers/RoutinesController.cs
- [x] T036 [US2] Sincronizar contrato OpenAPI de catálogo/rutinas en backend/src/GymTracker.Api/Contracts/openapi.yaml y specs/001-track-fitness-progress/contracts/openapi.yaml
- [x] T037 [US2] Implementar páginas, formularios y APIs frontend para rutinas en frontend/src/features/routines/pages/RoutineBuilderPage.tsx, frontend/src/features/routines/components/RoutineForm.tsx, frontend/src/features/routines/components/MuscleGroupSelector.tsx, frontend/src/features/routines/api/routinesApi.ts y frontend/src/features/workouts/components/StartFromRoutineDialog.tsx

**Checkpoint**: User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Crear dietas diarias y control calórico opcional (Priority: P3)

**Goal**: Permitir planificar dietas por días y registrar comidas con comportamiento calórico opt-in claro.

**Independent Test**: Crear una dieta semanal, registrar comidas y alternar el seguimiento calórico sin romper el flujo.

### Tests for User Story 3 (MANDATORY when this story changes executable behavior) ⚠️

- [x] T038 [P] [US3] Crear pruebas unitarias para reglas de preferencias calóricas y cálculo de estado calórico en backend/tests/GymTracker.Application.UnitTests/Meals/CalorieTrackingRulesTests.cs y backend/tests/GymTracker.Application.UnitTests/Diets/DietServiceTests.cs
- [x] T039 [P] [US3] Crear pruebas de integración para CRUD de /api/diets, POST/GET /api/meals/logs y GET/PUT /api/settings/preferences en backend/tests/GymTracker.Api.IntegrationTests/Diets/DietEndpointsTests.cs, backend/tests/GymTracker.Api.IntegrationTests/Meals/MealLogEndpointsTests.cs y backend/tests/GymTracker.Api.IntegrationTests/Settings/UserPreferencesEndpointsTests.cs
- [x] T040 [P] [US3] Crear pruebas de componentes para planificador de dietas, registro de comidas y preferencias calóricas en frontend/tests/component/diets/DietPlannerPage.test.tsx, frontend/tests/component/meals/MealLogPage.test.tsx y frontend/tests/component/settings/CalorieTrackingToggle.test.tsx

### Implementation for User Story 3

- [x] T041 [P] [US3] Implementar entidades Diet, MealLog y UserPreferences en backend/src/GymTracker.Domain/Entities/Diet.cs, backend/src/GymTracker.Domain/Entities/MealLog.cs y backend/src/GymTracker.Domain/Entities/UserPreferences.cs
- [x] T042 [P] [US3] Implementar repositorios de dietas, comidas y preferencias en backend/src/GymTracker.Application/Diets/IDietRepository.cs, backend/src/GymTracker.Application/Meals/IMealLogRepository.cs, backend/src/GymTracker.Application/Settings/IUserPreferencesRepository.cs, backend/src/GymTracker.Infrastructure/Firebase/DietRepository.cs, backend/src/GymTracker.Infrastructure/Firebase/MealLogRepository.cs y backend/src/GymTracker.Infrastructure/Firebase/UserPreferencesRepository.cs
- [x] T043 [US3] Implementar servicios de aplicación para dietas, comidas y seguimiento calórico en backend/src/GymTracker.Application/Diets/DietService.cs, backend/src/GymTracker.Application/Meals/MealLogService.cs y backend/src/GymTracker.Application/Settings/UserPreferencesService.cs
- [x] T044 [US3] Implementar endpoints de dietas, comidas y preferencias en backend/src/GymTracker.Api/Controllers/DietsController.cs, backend/src/GymTracker.Api/Controllers/MealsController.cs y backend/src/GymTracker.Api/Controllers/SettingsController.cs
- [x] T045 [US3] Sincronizar contrato OpenAPI de dietas/comidas/preferencias en backend/src/GymTracker.Api/Contracts/openapi.yaml y specs/001-track-fitness-progress/contracts/openapi.yaml
- [x] T046 [US3] Implementar páginas y formularios frontend para dietas, comidas y preferencias en frontend/src/features/diets/pages/DietPlannerPage.tsx, frontend/src/features/diets/components/DietDayEditor.tsx, frontend/src/features/meals/pages/MealLogPage.tsx, frontend/src/features/meals/components/MealLogForm.tsx, frontend/src/features/settings/pages/PreferencesPage.tsx y frontend/src/features/settings/components/CalorieTrackingToggle.tsx
- [x] T047 [US3] Implementar visualización de calorías conocidas/no informadas/desactivadas en frontend/src/features/meals/components/CalorieStatusBadge.tsx y frontend/src/features/meals/utils/calorieStatus.ts

**Checkpoint**: User Stories 1, 2 and 3 should be independently functional.

---

## Phase 6: User Story 4 - Consultar historial y estado degradado controlado (Priority: P4)

**Goal**: Garantizar acceso al historial personal y continuidad funcional cuando Wger falle temporalmente.

**Independent Test**: Consultar historial y catálogos cacheados durante una caída simulada de Wger con mensajes de degradación visibles.

### Tests for User Story 4 (MANDATORY when this story changes executable behavior) ⚠️

- [x] T048 [P] [US4] Crear pruebas unitarias para política de caché y degradación en backend/tests/GymTracker.Application.UnitTests/Catalog/CatalogFallbackPolicyTests.cs
- [x] T049 [P] [US4] Crear pruebas de integración para fallback de catálogo e historial paginado en backend/tests/GymTracker.Api.IntegrationTests/Catalog/CatalogFallbackEndpointsTests.cs and backend/tests/GymTracker.Api.IntegrationTests/History/HistoryEndpointsTests.cs
- [x] T050 [P] [US4] Crear pruebas de componentes para vistas de historial y banner de estado degradado en frontend/tests/component/history/HistoryPage.test.tsx y frontend/tests/component/catalog/CatalogStatusBanner.test.tsx

### Implementation for User Story 4

- [x] T051 [P] [US4] Implementar consultas de historial paginado y agregados de dashboard en backend/src/GymTracker.Application/Workouts/WorkoutHistoryService.cs, backend/src/GymTracker.Application/Meals/MealHistoryService.cs y backend/src/GymTracker.Infrastructure/Firebase/DashboardSummaryRepository.cs
- [x] T052 [US4] Implementar política explícita de fallback y metadatos de estado de catálogo en backend/src/GymTracker.Application/Catalog/CatalogAvailabilityService.cs y backend/src/GymTracker.Infrastructure/Caching/CatalogCacheMetadataStore.cs
- [x] T053 [US4] Extender endpoints para historial y estado degradado en backend/src/GymTracker.Api/Controllers/WorkoutsController.cs, backend/src/GymTracker.Api/Controllers/MealsController.cs y backend/src/GymTracker.Api/Controllers/CatalogController.cs
- [x] T054 [US4] Sincronizar contrato OpenAPI de historial y estado de catálogo en backend/src/GymTracker.Api/Contracts/openapi.yaml y specs/001-track-fitness-progress/contracts/openapi.yaml
- [x] T055 [US4] Implementar pantallas de historial y banner de disponibilidad de catálogo en frontend/src/features/workouts/pages/WorkoutHistoryPage.tsx, frontend/src/features/meals/pages/MealHistoryPage.tsx, frontend/src/features/catalog/components/CatalogStatusBanner.tsx y frontend/src/features/progress/components/HistoryFilters.tsx

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras y validaciones que afectan a múltiples historias.

- [x] T056 [P] Actualizar documentación técnica y de entorno en README.md y specs/001-track-fitness-progress/quickstart.md
- [x] T057 Endurecer reglas de seguridad y validación de Firestore en firebase/firestore.rules y backend/src/GymTracker.Application/Common/AuthorizationPolicies.cs
- [x] T058 [P] Afinar rendimiento de consultas y caché en backend/src/GymTracker.Infrastructure/Firebase/FirestoreIndexes.md y backend/src/GymTracker.Infrastructure/Caching/CatalogCacheService.cs
- [x] T059 Ejecutar validación end-to-end del quickstart y registrar resultados en specs/001-track-fitness-progress/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories.
- **User Stories (Phase 3+)**: All depend on Foundational phase completion.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - No dependencies on other stories.
- **User Story 2 (P2)**: Can start after Foundational - Reuses shared catalog/auth foundations and integrates optionally with US1 through start-from-routine.
- **User Story 3 (P3)**: Can start after Foundational - Depends only on shared auth/catalog/settings infrastructure.
- **User Story 4 (P4)**: Can start after Foundational, but is strengthened by the data flows created in US1-US3.

### Within Each User Story

- Tests MUST be written and fail before implementation.
- Domain entities/repositories before services.
- Services before controllers/endpoints.
- Backend contract sync before frontend consumption finalization.
- Frontend pages/components after backend APIs are available or mocked against the contract.

### Parallel Opportunities

- T004-T006 can run in parallel during Setup.
- T008-T015 can run in parallel during Foundational once T007 exists as bootstrap anchor.
- Within each story, test tasks and entity/repository tasks marked `[P]` can run in parallel.
- US2 and US3 can proceed in parallel after Foundational if team capacity allows.

---

## Parallel Example: User Story 1

```text
T017 Contract/domain tests for progress rules
T018 API integration tests for workouts/progress
T019 Frontend component tests for workout entry and progress UI

T020 Domain entities for workouts/progress
T021 Firestore repositories for workouts/progress
```

## Parallel Example: User Story 2

```text
T028 Routine unit tests
T029 API integration tests for catalog/routines
T030 Frontend tests for routine builder

T031 Routine domain models
T032 Routine repositories and validators
```

## Parallel Example: User Story 3

```text
T038 Calorie and diet unit tests
T039 API integration tests for diets/meals/settings
T040 Frontend tests for diet planner and calorie toggle

T041 Diet/MealLog/UserPreferences domain entities
T042 Diet/Meal/Preferences repositories
```

## Parallel Example: User Story 4

```text
T048 Fallback policy unit tests
T049 History/fallback integration tests
T050 Frontend tests for history and degraded mode banner
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate workout registration + progress comparison end-to-end.

### Incremental Delivery

1. Setup + Foundational create the technical baseline.
2. Add US1 for core workout/progress value.
3. Add US2 for routine planning reuse.
4. Add US3 for nutrition planning and calorie preferences.
5. Add US4 for reliability/history hardening.
6. Finish with Polish and quickstart validation.

### Parallel Team Strategy

1. Developer A: backend core and progress (US1, then US4 backend).
2. Developer B: frontend flows for workouts/routines.
3. Developer C: diets/meals/settings across backend and frontend.
4. Shared review point after each story checkpoint.

---

## Notes

- `[P]` tasks = different files, no dependencies on incomplete sibling tasks.
- All tasks include explicit file paths so they are directly executable by an implementation agent.
- Tests are mandatory for impacted backend/frontend slices according to the constitution.
- Git hooks were not executed while generating this task list, per user instruction.
