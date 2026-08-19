---
description: "Task list for Modulo de Administracion de Ejercicios con Multimedia"
---

# Tasks: Modulo de Administracion de Ejercicios con Multimedia

**Input**: Design documents from `/specs/002-modulo-administracion-ejercicios-multimedia/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend changes MUST include unit/integration coverage; frontend changes MUST include component tests. Admin authorization, media workflow, snapshot mapping, borrado logico filtering y selector de workouts son superficies criticas.

**Organization**: Tasks are grouped by user story (US1, US2, US3) to enable independent implementation and testing of each story.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Configurar opciones, paquetes y base para el modulo admin.

- [x] T001 [P] Extender `FirebaseOptions` en `backend/src/GymTracker.Api/Options/FirebaseOptions.cs` para agregar `StorageBucket`
- [x] T002 [P] Configurar `appsettings.Development.json` con claves de configuración para Firebase Storage
- [x] T003 [P] Configurar `frontend/.env.example` con variables adicionales de almacenamiento
- [x] T004 Actualizar `.github/copilot-instructions.md` para referenciar los artefactos de la feature 002

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura base obligatoria antes de cualquier historia de usuario.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Authorization & Token Validation

- [x] T005 [P] Validar Firebase ID tokens reales en `backend/src/GymTracker.Api/Middleware/FirebaseAuthMiddleware.cs` (reemplazar la logica actual de bearer token)
- [x] T006 [P] Extraer claim estandarizado `custom.admin=true` en `backend/src/GymTracker.Api/Middleware/CurrentUserContextMiddleware.cs` y agregarlo al contexto de usuario
- [x] T007 Crear política `AdminOnly` en `backend/src/GymTracker.Application/Common/AuthorizationPolicies.cs` junto a `DefaultUserPolicy`
- [x] T008 Validar que el modelo de claim admin (`custom.admin=true`) sea consistente en tests de integración `backend/tests/GymTracker.Api.IntegrationTests/Admin/AuthorizationTests.cs`

### Storage Infrastructure

- [x] T009 [P] Agregar paquete `Google.Cloud.Storage.V1` a `backend/src/GymTracker.Infrastructure/GymTracker.Infrastructure.csproj`
- [x] T010 Crear servicio de Storage `backend/src/GymTracker.Infrastructure/Storage/StorageService.cs` con metodos para signed URL y delete
- [x] T011 Registrar `StorageService` en `backend/src/GymTracker.Api/Program.cs` como scoped service
- [x] T012 [P] Crear DTOs para upload tickets en `backend/src/GymTracker.Application/Admin/Exercises/MediaUploadTicket.cs`

### Firestore Admin Persistence

- [x] T013 [P] Crear abstracciones de repositorio base para admin en `backend/src/GymTracker.Application/Admin/Common/IAdminRepository.cs`
- [x] T014 [P] Crear helpers para claves de indice `admin-index/...` en `backend/src/GymTracker.Infrastructure/Firebase/FirestoreAdminKeyHelper.cs`, manteniendo el patron de claves logicas dentro de `appDocuments`
- [x] T015 Crear metodos de utilidad en `FirestoreContext` para soportar queries por prefijo de índices y validar compatibilidad de prefijos `admin/...` con el key builder existente
- [x] T016 [P] Crear tipos base de entidades admin (`active`, `isDeleted`, `deletedAt`, `createdAt`, `updatedAt`) en `backend/src/GymTracker.Domain/ValueObjects/AdminAuditFields.cs`

### Frontend Authentication & Guard

- [x] T017 [P] Crear contexto/hook de autenticación admin en `frontend/src/features/admin/state/authContext.tsx` que lea el claim estandarizado `custom.admin=true`
- [x] T018 [P] Crear guard de `/admin` en `frontend/src/features/admin/components/AdminGuard.tsx` que bloquee sin claim admin
- [x] T019 [P] Crear layout base de admin en `frontend/src/features/admin/components/AdminLayout.tsx` con sidebar de navegación

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Administrar catalogo maestro fitness (Priority: P1) 🎯 MVP

**Goal**: Permitir CRUD completo de grupos musculares, músculos, tipos de ejercicio y tipos de formulario desde un panel admin.

**Independent Test**: Crear, editar, borrar logicamente cada entidad desde el panel admin, validar persistencia en Firestore y que los listados filtren por borrado logico.

### Tests for User Story 1 (MANDATORY) ⚠️

- [x] T020 [P] [US1] Unit tests para validacion de codigo unico en `backend/tests/GymTracker.Application.UnitTests/Admin/UniqueCodeValidatorTests.cs`
- [x] T021 [P] [US1] Unit tests para filtrado por borrado logico en `backend/tests/GymTracker.Application.UnitTests/Admin/SoftDeleteFilterTests.cs`
- [x] T022 [P] [US1] Integration tests para CRUD de MuscleGroup endpoints en `backend/tests/GymTracker.Api.IntegrationTests/Admin/MuscleGroupsEndpointsTests.cs`
- [x] T023 [P] [US1] Integration tests para CRUD de Muscle endpoints en `backend/tests/GymTracker.Api.IntegrationTests/Admin/MusclesEndpointsTests.cs`
- [x] T024 [P] [US1] Integration tests para CRUD de ExerciseType endpoints en `backend/tests/GymTracker.Api.IntegrationTests/Admin/ExerciseTypesEndpointsTests.cs`
- [x] T025 [P] [US1] Integration tests para CRUD de ExerciseFormType endpoints en `backend/tests/GymTracker.Api.IntegrationTests/Admin/ExerciseFormTypesEndpointsTests.cs`
- [x] T026 [P] [US1] Frontend component tests para tablas y formularios de admin en `frontend/tests/component/admin/AdminCrudTables.test.tsx`

### Implementation for User Story 1

#### Domain & Entities

- [x] T027 [P] [US1] Crear entidad `MuscleGroup` en `backend/src/GymTracker.Domain/Entities/MuscleGroup.cs`
- [x] T028 [P] [US1] Crear entidad `Muscle` en `backend/src/GymTracker.Domain/Entities/Muscle.cs` con validacion de relaciones a MuscleGroup
- [x] T029 [P] [US1] Crear entidad `ExerciseType` en `backend/src/GymTracker.Domain/Entities/ExerciseType.cs`
- [x] T030 [P] [US1] Crear entidad `ExerciseFormField` en `backend/src/GymTracker.Domain/ValueObjects/ExerciseFormField.cs` con validaciones de min/max, opciones
- [x] T031 [P] [US1] Crear entidad `ExerciseFormType` en `backend/src/GymTracker.Domain/Entities/ExerciseFormType.cs` con agregado `fields[]`

#### Repositories & Services

- [x] T032 [P] [US1] Crear repositorio `IMuscleGroupRepository` y `MuscleGroupRepository` en `backend/src/GymTracker.Application/Admin/MuscleGroups/` con soporte de indices de codigo
- [x] T033 [P] [US1] Crear repositorio `IMuscleRepository` y `MuscleRepository` en `backend/src/GymTracker.Application/Admin/Muscles/` con validaciones de grupo muscular
- [x] T034 [P] [US1] Crear repositorio `IExerciseTypeRepository` y `ExerciseTypeRepository` en `backend/src/GymTracker.Application/Admin/ExerciseTypes/`
- [x] T035 [P] [US1] Crear repositorio `IExerciseFormTypeRepository` y `ExerciseFormTypeRepository` en `backend/src/GymTracker.Application/Admin/ExerciseFormTypes/` con validaciones de campos
- [x] T036 [US1] Crear servicio de aplicacion `MuscleGroupService` en `backend/src/GymTracker.Application/Admin/MuscleGroups/MuscleGroupService.cs` (depende T032)
- [x] T037 [US1] Crear servicio de aplicacion `MuscleService` en `backend/src/GymTracker.Application/Admin/Muscles/MuscleService.cs` (depende T033)
- [x] T038 [US1] Crear servicio de aplicacion `ExerciseTypeService` en `backend/src/GymTracker.Application/Admin/ExerciseTypes/ExerciseTypeService.cs` (depende T034)
- [x] T039 [US1] Crear servicio de aplicacion `ExerciseFormTypeService` en `backend/src/GymTracker.Application/Admin/ExerciseFormTypes/ExerciseFormTypeService.cs` (depende T035)

#### Endpoints & DTOs

- [x] T040 [US1] Crear DTOs de entrada/salida para MuscleGroup en `backend/src/GymTracker.Application/Admin/MuscleGroups/MuscleGroupContracts.cs`
- [x] T041 [US1] Crear DTOs de entrada/salida para Muscle en `backend/src/GymTracker.Application/Admin/Muscles/MuscleContracts.cs`
- [x] T042 [US1] Crear DTOs de entrada/salida para ExerciseType en `backend/src/GymTracker.Application/Admin/ExerciseTypes/ExerciseTypeContracts.cs`
- [x] T043 [US1] Crear DTOs de entrada/salida para ExerciseFormType en `backend/src/GymTracker.Application/Admin/ExerciseFormTypes/ExerciseFormTypeContracts.cs`
- [x] T044 [US1] Crear controlador `MuscleGroupsController` en `backend/src/GymTracker.Api/Controllers/Admin/MuscleGroupsController.cs` con endpoints CRUD y paginacion
- [x] T045 [US1] Crear controlador `MusclesController` en `backend/src/GymTracker.Api/Controllers/Admin/MusclesController.cs` con endpoints CRUD
- [x] T046 [US1] Crear controlador `ExerciseTypesController` en `backend/src/GymTracker.Api/Controllers/Admin/ExerciseTypesController.cs` con endpoints CRUD
- [x] T047 [US1] Crear controlador `ExerciseFormTypesController` en `backend/src/GymTracker.Api/Controllers/Admin/ExerciseFormTypesController.cs` con endpoints CRUD y validacion de campos

#### Frontend

- [x] T048 [P] [US1] Crear servicio API de admin en `frontend/src/features/admin/api/adminApi.ts` con llamadas para todas las entidades US1
- [x] T049 [P] [US1] Crear slice Redux para admin en `frontend/src/features/admin/state/adminSlice.ts` con estado compartido de listados y seleccionados
- [x] T050 [US1] Crear pagina de MuscleGroups en `frontend/src/features/admin/pages/MuscleGroupsPage.tsx` con tabla CRUD (depende T049)
- [x] T051 [US1] Crear pagina de Muscles en `frontend/src/features/admin/pages/MusclesPage.tsx` con tabla CRUD y selector de grupo
- [x] T052 [US1] Crear pagina de ExerciseTypes en `frontend/src/features/admin/pages/ExerciseTypesPage.tsx` con tabla CRUD
- [x] T053 [US1] Crear pagina de ExerciseFormTypes en `frontend/src/features/admin/pages/ExerciseFormTypesPage.tsx` con constructor visual de campos
- [x] T054 [P] [US1] Crear componentes reutilizables AdminTable, AdminForm, AdminModal en `frontend/src/features/admin/components/`

#### OpenAPI Contract

- [x] T055 [US1] Sincronizar endpoints de MuscleGroup, Muscle, ExerciseType, ExerciseFormType en `backend/src/GymTracker.Api/Contracts/openapi.yaml` y `specs/002-modulo-administracion-ejercicios-multimedia/contracts/openapi.yaml`

**Checkpoint**: User Story 1 debe ser completamente funcional e independientemente testeable.

---

## Phase 4: User Story 2 - Gestionar ejercicios con multimedia educativa (Priority: P1)

**Goal**: Permitir CRUD de ejercicios con soporte para subida, confirmacion, reorden y set primary de imagenes y videos.

**Independent Test**: Crear ejercicio, emitir upload URL, subir archivo a Storage, confirmar metadato, marcar portada, reordenar, eliminar, validar limites y compensacion ante fallos.

### Tests for User Story 2 (MANDATORY) ⚠️

- [x] T056 [P] [US2] Unit tests para invariantes de media (isPrimary, sortOrder, limites) en `backend/tests/GymTracker.Application.UnitTests/Admin/ExerciseMediaInvariantsTests.cs`
- [x] T057 [P] [US2] Unit tests para snapshot mapping en `backend/tests/GymTracker.Application.UnitTests/Admin/ExerciseSnapshotMappingTests.cs`
- [x] T058 [P] [US2] Integration tests para CRUD de Exercise endpoints en `backend/tests/GymTracker.Api.IntegrationTests/Admin/ExercisesEndpointsTests.cs`
- [x] T059 [P] [US2] Integration tests para flujo de media upload, confirm, reorder, set-primary, delete en `backend/tests/GymTracker.Api.IntegrationTests/Admin/ExerciseMediaEndpointsTests.cs`
- [x] T060 [P] [US2] Integration tests para compensacion ante fallos Firestore/Storage en `backend/tests/GymTracker.Api.IntegrationTests/Admin/MediaCompensationTests.cs`
- [x] T061 [P] [US2] Frontend component tests para uploader multimedia y editor de ejercicio en `frontend/tests/component/admin/ExerciseMediaUploader.test.tsx`

### Implementation for User Story 2

#### Domain & Entities

- [x] T062 [P] [US2] Crear value object `ExerciseMedia` en `backend/src/GymTracker.Domain/ValueObjects/ExerciseMedia.cs` con invariantes de isPrimary, sortOrder, activo
- [x] T063 [P] [US2] Crear entidad `Exercise` en `backend/src/GymTracker.Domain/Entities/Exercise.cs` como agregado que embebe `media[]`, con validaciones de limites y musculos
- [x] T064 [US2] Implementar logica de determinacion de portada principal en `Exercise` (depende T062, T063)

#### Repositories & Services

- [x] T065 [P] [US2] Crear repositorio `IExerciseRepository` y `ExerciseRepository` en `backend/src/GymTracker.Application/Admin/Exercises/` con soporte de indices de codigo e indices de media
- [x] T066 [US2] Crear servicio de aplicacion `ExerciseService` en `backend/src/GymTracker.Application/Admin/Exercises/ExerciseService.cs` (depende T065)
- [x] T067 [US2] Crear servicio de confirmacion y compensacion de media `ExerciseMediaCompensationService` en `backend/src/GymTracker.Application/Admin/Exercises/ExerciseMediaCompensationService.cs` que implemente logica transaccional de Firestore/Storage

#### Endpoints & DTOs

- [x] T068 [US2] Crear DTOs de entrada/salida para Exercise en `backend/src/GymTracker.Application/Admin/Exercises/ExerciseContracts.cs` con snapshot minimo para workouts
- [x] T069 [US2] Crear DTOs para media upload y confirm en `backend/src/GymTracker.Application/Admin/Exercises/ExerciseMediaContracts.cs`
- [x] T070 [US2] Crear controlador `ExercisesController` en `backend/src/GymTracker.Api/Controllers/Admin/ExercisesController.cs` con endpoints CRUD de ejercicio
- [x] T071 [US2] Crear endpoints de media en el controlador Exercise: POST upload-url, POST media (confirm), PATCH media (update metadatos), PATCH media/reorder, PATCH media/set-primary, DELETE media

#### Storage Integration

- [x] T072 [US2] Implementar emision de signed URL en `ExerciseMediaController` llamando a `StorageService.GenerateUploadUrlAsync()`
- [x] T073 [US2] Implementar confirmacion de media con verificacion de existencia de objeto en Storage
- [x] T074 [US2] Implementar compensacion de borrado de objeto en Storage si falla persistencia en Firestore

#### Frontend

- [x] T075 [P] [US2] Crear formulario de ejercicio con selector dinámico de musculos en `frontend/src/features/admin/pages/ExercisesPage.tsx`
- [x] T076 [P] [US2] Crear componente de uploader multimedia en `frontend/src/features/admin/components/ExerciseMediaUploader.tsx` que maneje signed URLs
- [x] T077 [US2] Crear componente de galeria y reordenamiento de media en `frontend/src/features/admin/components/ExerciseMediaGallery.tsx`
- [x] T078 [US2] Extender apiSlice de admin para nuevos endpoints de media en `frontend/src/features/admin/api/adminApi.ts`

#### OpenAPI Contract

- [x] T079 [US2] Sincronizar endpoints de Exercise, media upload-url, media confirm, media reorder, media set-primary, media delete en `backend/src/GymTracker.Api/Contracts/openapi.yaml` y `specs/002-modulo-administracion-ejercicios-multimedia/contracts/openapi.yaml`

**Checkpoint**: User Stories 1 AND 2 deben ser completamente funcionales e independientemente testeables.

---

## Phase 5: User Story 3 - Consumir catalogo admin en workouts sin ruptura (Priority: P2)

**Goal**: Exponer un endpoint dedicado de catalogo admin para workouts, adaptando selectores y preservando historico con snapshots minimos.

**Independent Test**: Crear ejercicio admin, verificar que aparece en selector de workouts, registrar workout y comprobar que contiene snapshot minimo, desactivar ejercicio y verificar historico intacto.

### Tests for User Story 3 (MANDATORY) ⚠️

- [x] T080 [P] [US3] Integration tests para endpoint `/api/workouts/exercise-catalog` en `backend/tests/GymTracker.Api.IntegrationTests/Workouts/WorkoutCatalogEndpointsTests.cs`
- [x] T081 [P] [US3] Integration tests para persistencia de snapshot en workouts y rutinas en `backend/tests/GymTracker.Api.IntegrationTests/Workouts/WorkoutSnapshotTests.cs`
- [x] T082 [P] [US3] Integration tests para lectura de historico de workouts con ejercicios borrados logicamente en `backend/tests/GymTracker.Api.IntegrationTests/Workouts/WorkoutHistoryCompatibilityTests.cs`
- [x] T083 [P] [US3] Frontend component tests para selector de workouts consumiendo catalogo admin en `frontend/tests/component/workouts/AdminExerciseSelector.test.tsx`

### Implementation for User Story 3

#### Domain & Value Objects

- [x] T084 [P] [US3] Crear value object `ExerciseSnapshot` en `backend/src/GymTracker.Domain/ValueObjects/ExerciseSnapshot.cs` con `exerciseId`, `name`, `coverStoragePath`, `formTypeId`, `capturedAt`
- [x] T085 [US3] Adaptar `ExerciseEntry` en `backend/src/GymTracker.Domain/Entities/ExerciseEntry.cs` para embeber o referenciar snapshot minimo

#### Repositories & Services

- [x] T086 [US3] Crear servicio de catalogo workout `WorkoutCatalogService` en `backend/src/GymTracker.Application/Workouts/WorkoutCatalogService.cs` que cargue ejercicios admin activos
- [x] T087 [US3] Extender `WorkoutService` en `backend/src/GymTracker.Application/Workouts/WorkoutService.cs` para crear snapshot minimo al registrar ejercicio

#### Endpoints & DTOs

- [x] T088 [US3] Crear DTOs para catalogo de workout `WorkoutCatalogExerciseDto` en `backend/src/GymTracker.Application/Workouts/WorkoutCatalogContracts.cs`
- [x] T089 [US3] Extender `WorkoutsController` en `backend/src/GymTracker.Api/Controllers/WorkoutsController.cs` con endpoint `GET /api/workouts/exercise-catalog`
- [x] T090 [US3] Implementar busqueda y filtrado por muscleGroupIds en el endpoint de catalogo de workout

#### Frontend

- [x] T091 [P] [US3] Extender `ExerciseSearchStep` en `frontend/src/features/workouts/components/ExerciseSearchStep.tsx` para consumir `/api/workouts/exercise-catalog` en lugar de `/api/catalog/exercises`
- [x] T092 [US3] Adaptador de vista de historial para renderizar snapshot minimo de ejercicio incluso si esta borrado logicamente
- [x] T093 [US3] Tests de regresion en selector de workouts para validar que sigue funcionando con nuevos ejercicios admin

#### Firestore Rules

- [x] T094 [US3] Actualizar `firebase/firestore.rules` para restringir acceso a colecciones admin y mantener datos maestros backend-only

#### OpenAPI Contract

- [x] T095 [US3] Agregar endpoint `/api/workouts/exercise-catalog` en `backend/src/GymTracker.Api/Contracts/openapi.yaml` y `specs/002-modulo-administracion-ejercicios-multimedia/contracts/openapi.yaml`
- [x] T096 [US3] Documentar cambios de contrato en workouts si hay adaptaciones de snapshot

**Checkpoint**: Todas las historias de usuario (US1, US2, US3) deben ser completamente funcionales e independientemente testeables.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras y validaciones finales de la feature completa.

- [x] T097 [P] Actualizar documentacion tecnica en README.md y specs/002-modulo-administracion-ejercicios-multimedia/quickstart.md
- [x] T098 Endurecer reglas de seguridad en Firestore para proteger datos admin en `firebase/firestore.rules` (MANDATORY)
- [x] T099 [P] Agregar tests unitarios adicionales para edge cases identificados
- [x] T100 Ejecutar validacion end-to-end del quickstart y registrar resultados
- [x] T101 [P] Code cleanup y refactoring para mantener consistencia con repo existente
- [x] T102 Sincronizacion final de contratos OpenAPI backend/spec/002

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - **BLOCKS** all user stories.
- **User Story 1 (Phase 3, P1)**: Can start after Foundational - No dependencies on other stories.
- **User Story 2 (Phase 4, P1)**: Can start after Foundational - Complements but independent of US1.
- **User Story 3 (Phase 5, P2)**: Can start after Foundational - Integrates with US2 (Exercise) but independently testeable.
- **Polish (Phase 6)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1**: `foundational complete` → can start immediately and complete independently
- **User Story 2**: `foundational complete` + `US1 Exercise domain entities` → can start once US1 entities exist
- **User Story 3**: `foundational complete` + `US2 Exercise complete` → consumes endpoint de workouts

### Within Each User Story

1. Tests MUST be written and FAIL before implementation
2. Domain entities/repositories before services
3. Services before controllers/endpoints
4. Backend contract sync before frontend consumption
5. Frontend pages/components after backend APIs exist

### Parallel Opportunities

**Phase 1 Setup**:
- T001-T004 todos marcan [P], pueden ejecutarse en paralelo

**Phase 2 Foundational**:
- T005-T008 (Authorization) pueden ejecutarse en paralelo entre sí
- T009-T012 (Storage) pueden ejecutarse en paralelo entre sí
- T013-T016 (Firestore) pueden ejecutarse en paralelo entre sí
- T017-T019 (Frontend Auth) pueden ejecutarse en paralelo entre sí

**Phase 3 User Story 1**:
- T020-T026 (tests) todos [P], pueden ejecutarse en paralelo
- T027-T031 (entities) todos [P], pueden ejecutarse en paralelo
- T032-T035 (repositorios) todos [P], pueden ejecutarse en paralelo
- T036-T039 (servicios) dependen de repositorios, pueden ejecutarse en paralelo entre sí
- T040-T047 (DTOs, endpoints) pueden ejecutarse en paralelo
- T048-T054 (Frontend) pueden ejecutarse en paralelo

**Phase 4 User Story 2**:
- T056-T061 (tests) todos [P], pueden ejecutarse en paralelo
- T062-T064 (entities) pueden ejecutarse en paralelo
- T065-T067 (repositorios, servicios) pueden ejecutarse en paralelo
- T068-T074 (DTOs, endpoints, Storage) pueden ejecutarse en paralelo
- T075-T078 (Frontend) pueden ejecutarse en paralelo

**Phase 5 User Story 3**:
- T080-T083 (tests) todos [P], pueden ejecutarse en paralelo
- T084-T090 (dominio, DTOs, endpoints) pueden ejecutarse en paralelo
- T091-T096 (Frontend, rules, OpenAPI) pueden ejecutarse en paralelo

---

## Parallel Example: Phase 2 Foundational

```
Launch Authorization tasks in parallel:
T005, T006, T007, T008

Launch Storage tasks in parallel:
T009, T010, T011, T012

Launch Firestore tasks in parallel:
T013, T014, T015, T016

Launch Frontend Auth tasks in parallel:
T017, T018, T019
```

---

## Parallel Example: User Story 2

```
Launch all tests for US2 together:
T056, T057, T058, T059, T060, T061

Launch all domain entities together:
T062, T063

Launch repository & services together:
T065, T066, T067

Launch DTOs & endpoints together:
T068, T069, T070, T071

Launch Storage integration together:
T072, T073, T074

Launch Frontend together:
T075, T076, T077, T078
```

---

## Implementation Strategy

### MVP First (US1 + US2 Complete)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (CRUD maestro)
4. Complete Phase 4: User Story 2 (Ejercicios + Multimedia)
5. **STOP and VALIDATE**: Test US1+US2 end-to-end
6. Deploy/demo if ready
7. Then add US3 (Workouts integration) in Phase 5

### Full Feature

1. Complete Phases 1-2: Setup + Foundational
2. Complete Phases 3-5: All User Stories in order or parallel
3. Complete Phase 6: Polish
4. Full end-to-end validation with quickstart.md

### Parallel Team Strategy (3+ developers)

1. Assign: Dev A → Phase 1, Dev B → Phase 2, Dev C → Polish prep
2. Once Phase 2 complete:
   - Dev A → US1 (CRUD maestro)
   - Dev B → US2 (Ejercicios + Multimedia)
   - Dev C → US3 (Workouts integration)
3. All integrate independently

---

## Notes

- [P] tasks = different files, no blocking dependencies → can run in parallel
- [Story] label maps task to specific user story for traceability (US1, US2, US3)
- Cada historia de usuario es completable e independientemente testeable
- Validar tests FAIL antes de implementar
- Commit después de cada tarea o grupo lógico
- Verificar en checkpoints de cada US que funciona de forma independiente
- Evitar: tareas vagas, conflictos de archivos, dependencias cross-story que rompan independencia
