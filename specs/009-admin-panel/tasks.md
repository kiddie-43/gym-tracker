# Tasks: 009 Admin Panel - Backend Endpoints and Contract Cleanup

**Input**: Design documents from `specs/009-admin-panel/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend changes in this feature require both unit and integration tests.

**Organization**: Tasks are grouped by user story to support independent implementation and validation.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare backend module skeleton and contract sync points for canonical admin APIs.

- [X] T001 Create canonical admin route constants and shared endpoint naming in src/backend/src/GymTracker.Api/Controllers/Admin/AdminRoutes.cs
- [X] T002 [P] Create feature-level task validation checklist entries in specs/009-admin-panel/checklists/requirements.md for canonical endpoints and contract cleanup
- [X] T003 [P] Add canonical admin integration test fixture for authenticated requests in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/AdminEndpointsTestFixture.cs
- [X] T004 Add shared admin unit test data builder for catalogs and soft delete states in src/backend/tests/GymTracker.Application.UnitTests/Admin/AdminTestDataBuilder.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement shared building blocks required by all admin catalog endpoints.

**⚠️ CRITICAL**: No user story work should begin before this phase is complete.

- [X] T005 Add shared admin request normalization helpers in src/backend/src/GymTracker.Application/Admin/Common/AdminRequestSanitizer.cs
- [X] T006 [P] Add shared soft delete + reactivate conflict guard service in src/backend/src/GymTracker.Application/Admin/Common/AdminReactivationGuard.cs
- [X] T007 [P] Add cross-catalog relation validator contract for exercises in src/backend/src/GymTracker.Application/Admin/Exercises/IExerciseRelationsValidator.cs
- [X] T008 Implement cross-catalog relation validator (muscles + measurement types active and non-deleted) in src/backend/src/GymTracker.Application/Admin/Exercises/ExerciseRelationsValidator.cs
- [X] T009 [P] Register new admin services/validators in src/backend/src/GymTracker.Api/Program.cs
- [X] T010 [P] Add Firestore collection constants for canonical admin catalogs (muscles, measurement-types, exercises) in src/backend/src/GymTracker.Infrastructure/Firebase/FirestoreContext.cs
- [X] T011 Remove legacy admin route declarations from API contract and add canonical placeholders in src/backend/src/GymTracker.Api/Contracts/openapi.yaml

**Checkpoint**: Foundation ready. User story implementation can proceed.

---

## Phase 3: User Story 1 - Canonical Muscles API + CSV Import (Priority: P1) 🎯 MVP

**Goal**: Deliver canonical `/api/admin/muscles` CRUD + soft delete/reactivate and CSV import with partial processing and row-level report.

**Independent Test**: Call muscles canonical endpoints end-to-end, soft delete/reactivate one record, then import mixed-validity CSV rows and verify partial persistence plus per-row result reasons.

### Tests for User Story 1 (MANDATORY) ⚠️

- [X] T012 [P] [US1] Add integration tests for canonical muscles CRUD + soft delete + reactivate in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MusclesCanonicalEndpointsTests.cs
- [X] T013 [P] [US1] Add integration tests for `includeDeleted` behavior in muscles listing in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MusclesIncludeDeletedTests.cs
- [X] T014 [P] [US1] Add integration tests for muscles CSV partial import and row-level report in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MusclesCsvImportEndpointsTests.cs
- [X] T015 [P] [US1] Add unit tests for CSV row validation/duplicate detection in src/backend/tests/GymTracker.Application.UnitTests/Admin/MuscleCsvImportServiceTests.cs

### Implementation for User Story 1

- [X] T016 [US1] Create canonical muscles controller with GET/GET by id/POST/PUT/DELETE/reactivate/import-csv endpoints in src/backend/src/GymTracker.Api/Controllers/Admin/MusclesController.cs
- [X] T017 [US1] Refactor muscles contracts to canonical request/response models (code, description, deletedAt) in src/backend/src/GymTracker.Application/Admin/Muscles/MuscleContracts.cs
- [X] T018 [US1] Update muscle domain rules to support canonical shape and explicit reactivate transition in src/backend/src/GymTracker.Domain/Entities/Muscle.cs
- [X] T019 [US1] Implement muscles service methods for soft delete + reactivate conflict handling in src/backend/src/GymTracker.Application/Admin/Muscles/MuscleService.cs
- [X] T020 [US1] Implement muscles repository query methods for includeDeleted filtering and code conflict checks in src/backend/src/GymTracker.Application/Admin/Muscles/IMuscleRepository.cs
- [X] T021 [US1] Implement CSV partial import service and row-level result model in src/backend/src/GymTracker.Application/Admin/Muscles/MuscleCsvImportService.cs
- [X] T022 [US1] Wire muscles CSV import orchestration into application service in src/backend/src/GymTracker.Application/Admin/Muscles/MuscleService.cs

**Checkpoint**: US1 is independently testable and provides MVP backend value.

---

## Phase 4: User Story 2 - Canonical Measurement Types API (Priority: P1)

**Goal**: Replace legacy exercise-form/exercise-type admin APIs with canonical `/api/admin/measurement-types` CRUD + soft delete/reactivate.

**Independent Test**: Create/update/list/soft-delete/reactivate measurement types via canonical endpoints and verify deleted rows only appear with `includeDeleted=true`.

### Tests for User Story 2 (MANDATORY) ⚠️

- [X] T023 [P] [US2] Add integration tests for canonical measurement-types CRUD + reactivate in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypesEndpointsTests.cs
- [X] T024 [P] [US2] Add unit tests for measurement type field rules (min 1 field, unique field names) in src/backend/tests/GymTracker.Application.UnitTests/Admin/MeasurementTypeRulesTests.cs

### Implementation for User Story 2

- [X] T025 [US2] Add measurement type domain entity aligned with canonical fields list in src/backend/src/GymTracker.Domain/Entities/MeasurementType.cs
- [X] T026 [US2] Add measurement type contracts and DTOs in src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeContracts.cs
- [X] T027 [US2] Implement measurement type service (list/get/create/update/delete/reactivate) in src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeService.cs
- [X] T028 [US2] Add measurement type repository interface and Firestore-backed implementation in src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeRepository.cs
- [X] T029 [US2] Create canonical measurement types controller in src/backend/src/GymTracker.Api/Controllers/Admin/MeasurementTypesController.cs
- [X] T030 [US2] Remove legacy exercise-form-types and exercise-types endpoint definitions and schemas from API contract in src/backend/src/GymTracker.Api/Contracts/openapi.yaml

**Checkpoint**: US2 is independently testable and canonical measurement-types API is available.

---

## Phase 5: User Story 3 - Canonical Exercises API + FK Relation Validation (Priority: P2)

**Goal**: Deliver canonical `/api/admin/exercises` CRUD + soft delete/reactivate with strict FK validation against active muscles and measurement types.

**Independent Test**: Create exercise with valid muscle/measurement references (success), then try with deleted/nonexistent FK references (validation failure), and verify delete/reactivate lifecycle.

### Tests for User Story 3 (MANDATORY) ⚠️

- [X] T031 [P] [US3] Add integration tests for canonical exercises CRUD + soft delete/reactivate in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/ExercisesCanonicalEndpointsTests.cs
- [X] T032 [P] [US3] Add integration tests for exercise FK validation errors (primary/secondary muscles, measurementType) in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/ExercisesRelationsValidationTests.cs
- [X] T033 [P] [US3] Add unit tests for exercise relations validator and reactivation conflicts in src/backend/tests/GymTracker.Application.UnitTests/Admin/ExerciseRelationsValidatorTests.cs

### Implementation for User Story 3

- [X] T034 [US3] Refactor exercise contracts from legacy type/form references to canonical measurementType reference in src/backend/src/GymTracker.Application/Admin/Exercises/ExerciseContracts.cs
- [X] T035 [US3] Update exercise domain model to replace form/type dependencies with measurementTypeId and canonical invariants in src/backend/src/GymTracker.Domain/Entities/Exercise.cs
- [X] T036 [US3] Implement FK validation flow in exercise service using active muscles and active measurement types in src/backend/src/GymTracker.Application/Admin/Exercises/ExerciseService.cs
- [X] T037 [US3] Create canonical exercises controller in src/backend/src/GymTracker.Api/Controllers/Admin/ExercisesController.cs
- [X] T038 [US3] Update exercise repository contract to support FK validation lookups and includeDeleted filtering in src/backend/src/GymTracker.Application/Admin/Exercises/IExerciseRepository.cs
- [X] T039 [US3] Replace legacy exercises admin endpoints with canonical schemas in API contract in src/backend/src/GymTracker.Api/Contracts/openapi.yaml

**Checkpoint**: US3 is independently testable with strict cross-catalog relation rules.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final contract alignment, regression safety, and execution docs.

- [X] T040 [P] Sync feature contract with implemented canonical backend endpoints in specs/009-admin-panel/contracts/openapi.yaml
- [X] T041 [P] Add integration regression tests to assert legacy admin routes return expected status (removed/replaced behavior) in src/backend/tests/GymTracker.Api.IntegrationTests/Admin/LegacyAdminRoutesCompatibilityTests.cs
- [X] T042 Update backend quickstart validation section with canonical admin smoke calls in specs/009-admin-panel/quickstart.md
- [X] T043 Execute backend unit/integration suite and document command outputs in specs/009-admin-panel/research.md

---

## Phase 7: Frontend Admin Panels (Priority: P1)

**Purpose**: Implement and validate the `/administracion` UI for muscles, measurement-types and exercises with load/error/empty states, soft-delete lifecycle and CSV import.

### Frontend Tests (MANDATORY) ⚠️

- [X] T044 [P] Add component tests for muscles panel states (loading/error/empty/list) in src/frontend/tests/component/administration/MusclesPanelStates.test.tsx
- [X] T045 [P] Add component tests for muscles CSV import partial result summary in src/frontend/tests/component/administration/MusclesCsvImport.test.tsx
- [X] T046 [P] Add component tests for measurement-types CRUD modal flows in src/frontend/tests/component/administration/MeasurementTypesPanel.test.tsx
- [X] T047 [P] Add component tests for exercises form reference selectors excluding deleted records in src/frontend/tests/component/administration/ExercisesPanelReferences.test.tsx

### Frontend Implementation

- [X] T048 [US1-FE] Implement muscles admin page section with table + includeDeleted toggle + reactivate action in src/frontend/src/pages/AdministrationPage.tsx
- [X] T049 [US1-FE] Implement muscles create/edit/delete/reactivate modal flows and error banners in src/frontend/src/components/administration/MusclesPanel.tsx
- [X] T050 [US1-FE] Implement muscles CSV upload workflow with row-level result rendering in src/frontend/src/components/administration/MusclesCsvImportDialog.tsx
- [X] T051 [US2-FE] Implement measurement-types admin panel with fields editor and CRUD lifecycle in src/frontend/src/components/administration/MeasurementTypesPanel.tsx
- [X] T052 [US3-FE] Implement exercises admin panel form/table with category, difficulty and measurement-type selectors in src/frontend/src/components/administration/ExercisesPanel.tsx
- [X] T053 [US3-FE] Wire Redux slices/actions/selectors for muscles, measurement-types and exercises admin modules in src/frontend/src/redux/
- [X] T054 [US3-FE] Implement admin service clients for canonical endpoints (`/api/admin/muscles`, `/api/admin/measurement-types`, `/api/admin/exercises`) in src/frontend/src/services/
- [X] T055 [US-FE] Add i18n keys for administration errors, empty states, import summaries and reactivation conflicts in src/frontend/src/i18n/resources.ts

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup and blocks all user stories.
- **User Stories (Phase 3-5)**: Depend on Foundational.
- **Polish (Phase 6)**: Depends on completion of target user stories.
- **Frontend (Phase 7)**: Depends on canonical backend endpoints (Phases 3-5) and should complete before final release validation.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2, independent of US2/US3.
- **US2 (P1)**: Starts after Phase 2, should complete before US3 for clean FK target availability.
- **US3 (P2)**: Starts after Phase 2 and depends functionally on canonical measurement-types and muscles catalogs.

### Within Each User Story

- Tests first (must fail initially).
- Contracts/domain updates before service/controller wiring.
- Service logic before repository/controller finalization.
- Story-specific integration tests pass before moving forward.

---

## Parallel Execution Examples

### User Story 1

- Run T012, T013, T014, T015 in parallel (separate test files).
- Run T017 and T021 in parallel before integrating into T019/T022.

### User Story 2

- Run T023 and T024 in parallel.
- Run T026 and T028 in parallel before T027 and T029.

### User Story 3

- Run T031, T032, T033 in parallel.
- Run T034 and T038 in parallel before T036 and T037.

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 and Phase 2.
2. Implement Phase 3 (US1) fully.
3. Validate CSV partial import + soft delete/reactivate lifecycle.
4. Demo/deploy MVP backend endpoints for muscles.

### Incremental Delivery

1. Deliver US1 (muscles canonical API + CSV).
2. Deliver US2 (measurement-types canonical API).
3. Deliver US3 (exercises canonical API + FK validation).
4. Execute Phase 6 for contract parity and regression coverage.

### Team Parallelization

1. Team aligns on shared foundation (Phase 2).
2. Developer A leads US1.
3. Developer B leads US2.
4. Developer C leads US3 once US2 contract/references are stable.

---

## Notes

- Tasks with [P] are parallelizable by file independence.
- Every user-story task includes [USx] label for traceability.
- Canonical backend routes in scope:
  - `/api/admin/muscles`
  - `/api/admin/measurement-types`
  - `/api/admin/exercises`
- Mandatory endpoint lifecycle per catalog: list, get by id, create, update, soft delete, reactivate.
- CSV import scope applies only to muscles with partial processing and row-level report.
