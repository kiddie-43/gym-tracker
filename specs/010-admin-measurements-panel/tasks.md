# Tasks: Panel de Administracion de Mediciones

**Input**: Design documents from `/specs/010-admin-measurements-panel/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar contratos y estructuras base para la feature.

- [X] T001 Alinear contrato de feature en specs/010-admin-measurements-panel/contracts/openapi.yaml con nombres finales de query (`includeInactive`) y esquemas de respuesta
- [X] T002 [P] Crear/actualizar tipos de frontend para mediciones en src/frontend/src/interfaces/admin/measurementTypes/measurementTypes.ts
- [X] T003 [P] Agregar claves de i18n para panel, popup y CSV de mediciones en src/frontend/src/i18n/resources.ts

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura de backend bloqueante para todas las historias.

**CRITICAL**: Ninguna historia puede iniciar hasta completar esta fase.

- [X] T004 Definir contratos canonicos (`key`, `name`, `unit`, `dataType`, `category`, `description`) en src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeContracts.cs
- [X] T005 [P] Actualizar interfaz de repositorio para CRUD, reactivacion, import CSV y listado asignable en src/backend/src/GymTracker.Application/Admin/MeasurementTypes/IMeasurementTypeRepository.cs
- [X] T006 Implementar validaciones de negocio y unicidad de `key` en src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeService.cs
- [X] T007 Implementar persistencia Firestore `measurementTypes/{measurementTypeId}` con soft delete/reactivate en src/backend/src/GymTracker.Infrastructure/Firebase/MeasurementTypeRepository.cs
- [X] T008 [P] Exponer endpoints base CRUD y reactivate de mediciones en src/backend/src/GymTracker.Api/Controllers/Admin/MeasurementTypesController.cs
- [X] T009 [P] Sincronizar contrato global de API para mediciones en src/backend/src/GymTracker.Api/Contracts/openapi.yaml

**Checkpoint**: Base backend lista para iniciar historias de usuario.

---

## Phase 3: User Story 1 - Gestionar tipos de medicion dinamicos (Priority: P1) 🎯 MVP

**Goal**: Permitir alta/edicion/desactivacion/reactivacion por popup con paridad UX respecto a musculos.

**Independent Test**: Crear, editar, desactivar y reactivar una medicion desde el panel sin usar importacion CSV.

### Tests for User Story 1

- [X] T010 [P] [US1] Crear pruebas unitarias de validacion de `dataType` y `category` en src/backend/tests/GymTracker.Application.UnitTests/Admin/MeasurementTypes/MeasurementTypeValidationTests.cs
- [X] T011 [P] [US1] Crear pruebas de integracion CRUD/reactivate de mediciones en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesCrudTests.cs
- [X] T012 [P] [US1] Crear prueba de flujo UI popup create/edit en src/frontend/tests/component/administration/MeasurementTypesPanel.popup.test.tsx
- [X] T037 [P] [US1] Crear pruebas de autorizacion admin para endpoints de mediciones (401/403/200) en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesAuthorizationTests.cs
- [X] T038 [P] [US1] Crear prueba de estados loading/error/vacio del panel en src/frontend/tests/component/administration/MeasurementTypesPanel.states.test.tsx

### Implementation for User Story 1

- [X] T013 [US1] Actualizar mapeo DTO/API de mediciones con nuevos campos en src/frontend/src/services/api/admin/measurementTypes/measurementTypesApi.ts
- [X] T014 [P] [US1] Extender estado del formulario de mediciones con campos canonicos en src/frontend/src/pages/administration/measurement-types/form/measurementTypeForm.ts
- [X] T015 [P] [US1] Implementar popup unificado create/edit con validaciones de campos en src/frontend/src/pages/administration/measurement-types/form/MeasurementTypeFormDialog.tsx
- [X] T016 [US1] Actualizar tabla de mediciones con columnas `key`, `name`, `unit`, `dataType`, `category`, estado y acciones en src/frontend/src/pages/administration/measurement-types/table/MeasurementTypesTable.tsx
- [X] T017 [US1] Implementar acciones top (anadir, refrescar), filtros y alternancia activos/desactivados en src/frontend/src/pages/administration/measurement-types/MeasurementTypesPanel.tsx
- [X] T018 [US1] Ajustar drawer de filtros para busqueda y toggle includeInactive en src/frontend/src/pages/administration/measurement-types/filters/MeasurementTypesFiltersDrawer.tsx
- [X] T039 [US1] Implementar paginacion con tamano de pagina configurable y estabilidad al filtrar en src/frontend/src/pages/administration/measurement-types/MeasurementTypesPanel.tsx

**Checkpoint**: US1 funcional e independiente (panel claro con popup CRUD y soft delete/reactivate).

---

## Phase 4: User Story 2 - Importar tipos de medicion por CSV (Priority: P1)

**Goal**: Permitir carga masiva CSV con procesamiento parcial y resultado por fila.

**Independent Test**: Subir CSV mixto (validas/invalidas) y verificar resumen + detalle de rechazos.

### Tests for User Story 2

- [X] T019 [P] [US2] Crear pruebas unitarias del importador CSV parcial por fila en src/backend/tests/GymTracker.Application.UnitTests/Admin/MeasurementTypes/MeasurementTypeCsvImportTests.cs
- [X] T020 [P] [US2] Crear prueba de integracion del endpoint import-csv en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesImportCsvTests.cs
- [X] T021 [P] [US2] Crear prueba de UI del flujo de importacion CSV en src/frontend/tests/component/administration/MeasurementTypesCsvImportDialog.test.tsx
- [X] T040 [US2] Crear prueba de volumen CSV (200 filas, >=95% validas procesadas) en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesImportCsvVolumeTests.cs

### Implementation for User Story 2

- [X] T022 [US2] Implementar contratos de importacion CSV (`ImportMeasurementTypesRequest/Result`) en src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeContracts.cs
- [X] T023 [US2] Implementar servicio de importacion parcial con errores por fila en src/backend/src/GymTracker.Application/Admin/MeasurementTypes/MeasurementTypeService.cs
- [X] T024 [US2] Exponer endpoint `POST /api/admin/measurement-types/import-csv` en src/backend/src/GymTracker.Api/Controllers/Admin/MeasurementTypesController.cs
- [X] T025 [US2] Agregar cliente API `importMeasurementTypesCsv` en src/frontend/src/services/api/admin/measurementTypes/measurementTypesApi.ts
- [X] T026 [P] [US2] Crear dialogo de importacion CSV para mediciones en src/frontend/src/pages/administration/measurement-types/MeasurementTypesCsvImportDialog.tsx
- [X] T027 [US2] Integrar boton/importador CSV y render de resultados en src/frontend/src/pages/administration/measurement-types/MeasurementTypesPanel.tsx

**Checkpoint**: US2 funcional e independiente (importacion parcial con reporte por fila).

---

## Phase 5: User Story 3 - Usar mediciones en asignacion de ejercicios (Priority: P2)

**Goal**: Permitir que ejercicios consuman solo tipos de medicion activos para nuevas asignaciones.

**Independent Test**: Desactivar una medicion y validar que deja de aparecer en selector de ejercicios.

### Tests for User Story 3

- [X] T028 [P] [US3] Crear prueba de integracion del endpoint assignable (solo activos) en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesAssignableTests.cs
- [X] T029 [P] [US3] Crear prueba de UI del selector de mediciones en ejercicios excluyendo desactivadas en src/frontend/tests/component/administration/ExercisesPanel.measurementAssignable.test.tsx
- [X] T041 [US3] Crear prueba de preservacion historica de asignaciones existentes tras desactivar una medicion en src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesHistoricalTraceabilityTests.cs

### Implementation for User Story 3

- [X] T030 [US3] Exponer endpoint `GET /api/admin/measurement-types/assignable` en src/backend/src/GymTracker.Api/Controllers/Admin/MeasurementTypesController.cs
- [X] T031 [US3] Implementar consulta de asignables activos en src/backend/src/GymTracker.Infrastructure/Firebase/MeasurementTypeRepository.cs
- [X] T032 [US3] Agregar cliente API `listAssignableMeasurementTypes` en src/frontend/src/services/api/admin/measurementTypes/measurementTypesApi.ts
- [X] T033 [US3] Consumir listado asignable en formulario de ejercicios en src/frontend/src/components/administration/ExercisesPanel.tsx

**Checkpoint**: US3 funcional e independiente (asignacion usa solo mediciones activas).

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cierre transversal de calidad, contrato y regresion.

- [X] T034 [P] Actualizar documentacion de uso/QA en specs/010-admin-measurements-panel/quickstart.md
- [X] T035 [P] Ejecutar validacion manual completa de quickstart y registrar resultados en specs/010-admin-measurements-panel/research.md
- [X] T036 Verificar y sincronizar contrato final en specs/010-admin-measurements-panel/contracts/openapi.yaml y src/backend/src/GymTracker.Api/Contracts/openapi.yaml
- [X] T042 Alinear terminologia documental de estado (desactivado/soft delete/includeInactive) en specs/010-admin-measurements-panel/spec.md, plan.md y contracts/openapi.yaml

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: inicia inmediatamente
- **Phase 2 (Foundational)**: depende de Phase 1 y bloquea historias
- **Phase 3 (US1)**: depende de Phase 2
- **Phase 4 (US2)**: depende de Phase 2 (puede correr en paralelo con US1, aunque se recomienda despues de US1 por reuso de panel)
- **Phase 5 (US3)**: depende de Phase 2 y de disponibilidad de API/frontend de mediciones de US1
- **Phase 6 (Polish)**: depende de historias completadas

### User Story Dependencies

- **US1 (P1)**: sin dependencia funcional de otras historias
- **US2 (P1)**: sin dependencia funcional de US3; reutiliza piezas de US1
- **US3 (P2)**: depende del catalogo de mediciones activo ya implementado

### Within Each User Story

- Implementar pruebas antes de cerrar la historia
- Completar backend antes de integrar frontend cuando haya endpoint nuevo
- Completar UI antes de ejecutar validacion E2E manual

---

## Parallel Opportunities

- T002 y T003 en paralelo (tipos e i18n)
- T008 y T009 en paralelo (controlador y contrato global)
- T010, T011 y T012 en paralelo (pruebas US1)
- T037 y T038 en paralelo con T010-T012 (pruebas adicionales US1)
- T014 y T015 en paralelo (estado/formulario US1)
- T019, T020 y T021 en paralelo (pruebas US2)
- T040 despues de T020 (mismo endpoint, escenario de volumen)
- T026 en paralelo con T024/T025 una vez definidos contratos de importacion
- T028 y T029 en paralelo (pruebas US3)
- T041 despues de T028 (expande cobertura de trazabilidad historica)

---

## Parallel Example: User Story 1

```bash
# Pruebas US1 en paralelo
T010: src/backend/tests/GymTracker.Application.UnitTests/Admin/MeasurementTypes/MeasurementTypeValidationTests.cs
T011: src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesCrudTests.cs
T012: src/frontend/tests/component/administration/MeasurementTypesPanel.popup.test.tsx

# Formulario US1 en paralelo
T014: src/frontend/src/pages/administration/measurement-types/form/measurementTypeForm.ts
T015: src/frontend/src/pages/administration/measurement-types/form/MeasurementTypeFormDialog.tsx
```

---

## Parallel Example: User Story 2

```bash
# Pruebas US2 en paralelo
T019: src/backend/tests/GymTracker.Application.UnitTests/Admin/MeasurementTypes/MeasurementTypeCsvImportTests.cs
T020: src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesImportCsvTests.cs
T021: src/frontend/tests/component/administration/MeasurementTypesCsvImportDialog.test.tsx
```

---

## Parallel Example: User Story 3

```bash
# Implementacion + pruebas US3
T028: src/backend/tests/GymTracker.Api.IntegrationTests/Admin/MeasurementTypes/MeasurementTypesAssignableTests.cs
T029: src/frontend/tests/component/administration/ExercisesPanel.measurementAssignable.test.tsx
T031: src/backend/src/GymTracker.Infrastructure/Firebase/MeasurementTypeRepository.cs
T032: src/frontend/src/services/api/admin/measurementTypes/measurementTypesApi.ts
```

---

## Implementation Strategy

### MVP First (US1)

1. Completar Phase 1 y Phase 2
2. Completar Phase 3 (US1)
3. Validar CRUD popup + soft delete/reactivate
4. Demostrar MVP del panel claro y no ambiguo

### Incremental Delivery

1. Entregar US1 (panel CRUD)
2. Entregar US2 (CSV parcial con reporte)
3. Entregar US3 (asignacion en ejercicios)
4. Cerrar con Phase 6 (polish/validacion final)

### Team Parallel Strategy

1. Backend completa Phase 2 mientras frontend prepara T002/T003
2. Tras Phase 2:
   - Dev A: US1 frontend panel
   - Dev B: US2 backend importador
   - Dev C: pruebas integracion/componentes
3. Integrar US3 al finalizar endpoints assignable
