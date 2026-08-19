# Tasks: Corrección de Estructura del Proyecto según Constitución

**Input**: Design documents from `specs/003-correccion-estructura-constitucion/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, quickstart.md ✓

**Scope**: Reorganización estructural pura — sin lógica nueva, sin cambios de comportamiento. Todos los módulos en una sola iteración.

**Tests**: No se requieren tests nuevos. Gate de calidad: `npx tsc --noEmit` (frontend) y `dotnet build` (backend) deben pasar con cero errores tras cada fase.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias incompletas)
- **[US#]**: User story de spec.md a la que pertenece

---

## Phase 1: Setup (Directorios destino)

**Purpose**: Crear los subdirectorios de módulo que aún no existen en las carpetas destino (algunos fueron creados en sesión anterior, los de módulo concreto no).

- [x] T001 Crear directorios destino de servicios API: `frontend/src/services/api/workouts/`, `frontend/src/services/api/diets/`, `frontend/src/services/api/meals/`, `frontend/src/services/api/routines/`, `frontend/src/services/api/settings/`, `frontend/src/services/api/admin/`
- [x] T002 Crear directorio destino de Redux admin: `frontend/src/redux/states/admin/`

---

## Phase 2: Foundational (Verificaciones previas)

**Purpose**: No hay prerequisitos que bloqueen todas las user stories. La interfaz de dominio `admin` requiere creación previa porque es nueva y es importada por US1 y US3.

**⚠️ NOTA**: US3 (interfaces) debe completarse antes de borrar archivos en US1/US2 que importen desde `shared/types/`. El orden de fases garantiza esto.

- [x] T003 Verificar que `frontend/src/shared/api/httpClient.ts` existe y exporta `apiFetch` (es el cliente HTTP canónico que todos los servicios deben importar). Si no existe, detener y crear el archivo antes de continuar.

**Checkpoint**: Directorios y cliente HTTP verificados — user stories pueden comenzar en orden de prioridad.

---

## Phase 3: User Story 1 — Estructura de Servicios del Frontend Corregida (Priority: P1)

**Goal**: Todos los servicios de API de cada módulo viven en `services/api/<modulo>/`. Los barrels y duplicados en `services/` raíz son eliminados.

**Independent Test**: `find frontend/src/services -maxdepth 1 -name "*.ts"` no retorna ningún archivo. `ls frontend/src/services/api/` muestra 6 subdirectorios de módulo con sus implementaciones.

### Migración de archivos de implementación

- [x] T004 [P] [US1] Mover `frontend/src/services/workouts/workoutsApi.ts` → `frontend/src/services/api/workouts/workoutsApi.ts` (actualizar imports relativos internos: `../httpClient` → `../../shared/api/httpClient`)
- [x] T005 [P] [US1] Mover `frontend/src/features/diets/api/dietsApi.ts` → `frontend/src/services/api/diets/dietsApi.ts` (actualizar imports relativos internos)
- [x] T006 [P] [US1] Mover `frontend/src/features/meals/api/mealsApi.ts` → `frontend/src/services/api/meals/mealsApi.ts` (actualizar imports relativos internos)
- [x] T007 [P] [US1] Mover `frontend/src/features/routines/api/routinesApi.ts` → `frontend/src/services/api/routines/routinesApi.ts` (actualizar imports relativos internos)
- [x] T008 [P] [US1] Mover `frontend/src/features/settings/api/preferencesApi.ts` → `frontend/src/services/api/settings/preferencesApi.ts` (actualizar imports relativos internos)
- [x] T009 [P] [US1] Mover `frontend/src/features/admin/api/adminApi.ts` → `frontend/src/services/api/admin/adminApi.ts` (actualizar imports relativos internos; los tipos del módulo admin se separarán en US3/T042)

### Actualización de imports externos (consume los archivos movidos)

- [x] T010 [US1] Actualizar todos los imports del proyecto que apuntaban a `features/diets/api/dietsApi`, `features/meals/api/mealsApi`, `features/routines/api/routinesApi`, `features/settings/api/preferencesApi`, `features/admin/api/adminApi` → rutas en `services/api/<modulo>/<modulo>Api`
- [x] T011 [US1] Actualizar todos los imports del proyecto que apuntaban a `services/workouts/workoutsApi` → `services/api/workouts/workoutsApi`

### Eliminación de barrels y duplicados

- [x] T012 [P] [US1] Eliminar barrel `frontend/src/services/dietsService.ts`
- [x] T013 [P] [US1] Eliminar barrel `frontend/src/services/mealsService.ts`
- [x] T014 [P] [US1] Eliminar barrel `frontend/src/services/routinesService.ts`
- [x] T015 [P] [US1] Eliminar barrel `frontend/src/services/preferencesService.ts`
- [x] T016 [P] [US1] Eliminar barrel `frontend/src/services/workoutsService.ts`
- [x] T017 [P] [US1] Eliminar duplicado `frontend/src/services/httpClient.ts` (implementación real vive en `frontend/src/shared/api/httpClient.ts`)
- [x] T018 [US1] Eliminar directorio vacío `frontend/src/services/workouts/` (queda vacío tras T004)
- [x] T019 [P] [US1] Eliminar directorio vacío `frontend/src/features/diets/api/` (queda vacío tras T005)
- [x] T020 [P] [US1] Eliminar directorio vacío `frontend/src/features/meals/api/` (queda vacío tras T006)
- [x] T021 [P] [US1] Eliminar directorio vacío `frontend/src/features/routines/api/` (queda vacío tras T007)
- [x] T022 [P] [US1] Eliminar directorio vacío `frontend/src/features/settings/api/` (queda vacío tras T008)
- [x] T023 [P] [US1] Eliminar directorio vacío `frontend/src/features/admin/api/` (queda vacío tras T009)

**Checkpoint**: `frontend/src/services/` contiene únicamente subcarpetas `api/`, `firebase/`, `observability/`, `storage/`. No hay archivos `.ts` sueltos. Ejecutar `npx tsc --noEmit` en frontend y verificar cero errores antes de continuar.

---

## Phase 4: User Story 2 — Estructura de Redux Corregida (Priority: P1)

**Goal**: `redux/` contiene exactamente `actions/`, `reducers/`, `states/`, `store.ts`, `hooks/`. No existe `stados/`. Los slices de cada módulo están en `redux/states/<modulo>/`. No hay barrels.

**Independent Test**: `ls frontend/src/redux/` muestra `actions/`, `hooks/`, `reducers/`, `states/`, `store.ts` y nada más. `ls frontend/src/redux/states/` muestra `admin/` y `workouts/`.

### Migración de slices y renombrado de store

- [x] T024 [US2] Mover `frontend/src/redux/stados/workoutsState.ts` → `frontend/src/redux/states/workouts/workoutsState.ts` corrigiendo TODOS los imports en el mismo paso (el cambio de profundidad de 2 a 3 niveles invalida todos los paths relativos): `'../globalState'` → `'../../store'`; `'../../shared/types/catalog'` → `'../../../interfaces/catalog'`; `'../../shared/types/workouts'` → `'../../../interfaces/workouts'`; `'../../services/api/workouts/workoutsApi'` → `'../../../services/api/workouts/workoutsApi'`. **No diferir ningún import a US3** — el checkpoint de compilación de Phase 4 requiere cero errores.
- [x] T025 [US2] Mover `frontend/src/features/admin/state/adminSlice.ts` → `frontend/src/redux/states/admin/adminState.ts` corrigiendo todos los imports: `import de adminApi` → `'../../../services/api/admin/adminApi'` (3 niveles desde `redux/states/admin/`; nota: T010 lo habrá actualizado a 3 niveles desde la ruta anterior que también era 3 niveles, pero verificar); `RootState` → importar desde `'../../store'`.
- [x] T025b [US2] Crear directorio `frontend/src/redux/reducers/admin/` (FR-011 — `redux/reducers/` MUST organizarse por módulo en subcarpetas; analogía con `redux/reducers/workouts/` ya existente)
- [x] T026 [US2] Renombrar `frontend/src/redux/globalState.ts` → `frontend/src/redux/store.ts` (actualizar imports internos: `./stados/workoutsState` → `./states/workouts/workoutsState`; `../features/admin/state/adminSlice` → `./states/admin/adminState`)

### Actualización de imports externos

- [x] T027 [US2] Actualizar todos los imports del proyecto que apuntaban a `redux/stados/workoutsState` → `redux/states/workouts/workoutsState`
- [x] T028 [US2] Actualizar todos los imports del proyecto que apuntaban a `features/admin/state/adminSlice` → `redux/states/admin/adminState`
- [x] T029 [US2] Actualizar todos los imports del proyecto que apuntaban a `redux/globalState` → `redux/store`
- [x] T030 [US2] Actualizar `frontend/src/app/store.ts` para que importe desde `../redux/store` (era `../redux/globalState`)

### Eliminación de barrels, directorios con typo y feature-state

- [x] T031 [US2] Eliminar barrel `frontend/src/redux/actions/workoutsActions.ts`
- [x] T032 [US2] Eliminar barrel `frontend/src/features/workouts/state/workoutsSlice.ts`
- [x] T033 [US2] Eliminar directorio vacío `frontend/src/redux/stados/` (queda vacío tras T024)
- [x] T034 [US2] Eliminar directorio vacío `frontend/src/features/workouts/state/` (queda vacío tras T032)
- [x] T035 [US2] Eliminar directorio vacío `frontend/src/features/admin/state/` (queda vacío tras T025)

**Checkpoint**: `frontend/src/redux/` tiene la estructura constitucional. Ejecutar `npx tsc --noEmit` y verificar cero errores.

---

## Phase 5: User Story 3 — Interfaces y Tipos sin Duplicados (Priority: P2)

**Goal**: `interfaces/<modulo>.ts` contiene los tipos reales (no barrels). `shared/types/` no existe. Todos los imports apuntan a `interfaces/`.

**Independent Test**: Búsqueda de `from '../shared/types` o `from '../../shared/types` en `frontend/src/` retorna cero resultados. `ls frontend/src/interfaces/` muestra 7 archivos con contenido real.

### Fusión de tipos: reemplazar barrels con contenido real

- [x] T036 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/catalog.ts` (barrel) con los tipos reales de `frontend/src/shared/types/catalog.ts` (`MuscleGroup`, `Exercise`, `Food`, `CatalogAvailability`)
- [x] T037 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/workouts.ts` (barrel) con los tipos reales de `frontend/src/shared/types/workouts.ts` (`WorkoutSummary`, `WorkoutSetInput`, `ExerciseEntryInput`, `CreateWorkoutRequest`, `MetricChange`, `ProgressMetrics`, `MetricDelta`, `WorkoutProgress`)
- [x] T038 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/diets.ts` (barrel) con los tipos reales de `frontend/src/shared/types/diets.ts`
- [x] T039 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/meals.ts` (barrel) con los tipos reales de `frontend/src/shared/types/meals.ts`
- [x] T040 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/routines.ts` (barrel) con los tipos reales de `frontend/src/shared/types/routines.ts`
- [x] T041 [P] [US3] Reemplazar contenido de `frontend/src/interfaces/settings.ts` (barrel) con los tipos reales de `frontend/src/shared/types/settings.ts`
- [x] T042 [US3] Crear `frontend/src/interfaces/admin.ts` con los tipos de dominio del módulo admin extraídos de `frontend/src/services/api/admin/adminApi.ts` (`AdminEntityBase`, `MuscleGroupDto`, `MuscleDto`, `ExerciseTypeDto`, `ExerciseFormTypeDto` y demás DTOs de admin)
- [x] T043 [US3] Actualizar `frontend/src/services/api/admin/adminApi.ts` para importar los tipos desde `../../interfaces/admin` en lugar de definirlos inline

### Actualización de imports externos

- [x] T044 [US3] Actualizar todos los imports del proyecto que apuntan a `shared/types/<modulo>` (cualquier módulo) → `interfaces/<modulo>` correspondiente

### Eliminación de `shared/types/` y carpetas vacías

- [x] T045 [US3] Eliminar `frontend/src/shared/types/` completo (todos sus archivos han sido fusionados en `interfaces/`)
- [x] T046 [US3] Eliminar `frontend/src/types/admin/` vacío

**Checkpoint**: `npx tsc --noEmit` con cero errores. Búsqueda de `shared/types` en el proyecto retorna cero coincidencias.

---

## Phase 6: User Story 4 — Componentes sin Duplicados (Priority: P2)

**Goal**: `AsyncState`, `FormPopupDialog`, `PageHeader` existen únicamente en `components/common/` con su implementación real. Las copias en `shared/components/` son eliminadas.

**Independent Test**: Búsqueda de `AsyncState` en `frontend/src/shared/components/` retorna cero resultados. Los componentes en `components/common/` contienen la implementación real (no `export { X } from ...`).

### Reemplazar barrels con implementaciones reales

- [x] T047 [P] [US4] Reemplazar `frontend/src/components/common/AsyncState.tsx` (barrel) con la implementación real de `frontend/src/shared/components/AsyncState.tsx`
- [x] T048 [P] [US4] Reemplazar `frontend/src/components/common/FormPopupDialog.tsx` (barrel) con la implementación real de `frontend/src/shared/components/FormPopupDialog.tsx`
- [x] T049 [P] [US4] Reemplazar `frontend/src/components/common/PageHeader.tsx` (barrel) con la implementación real de `frontend/src/shared/components/PageHeader.tsx`

### Actualización de imports externos

- [x] T050 [US4] Actualizar todos los imports del proyecto que apuntan a `shared/components/AsyncState`, `shared/components/FormPopupDialog` o `shared/components/PageHeader` → `components/common/<Componente>`

### Eliminación de copias en shared/components/

- [x] T051 [P] [US4] Eliminar `frontend/src/shared/components/AsyncState.tsx`
- [x] T052 [P] [US4] Eliminar `frontend/src/shared/components/FormPopupDialog.tsx`
- [x] T053 [P] [US4] Eliminar `frontend/src/shared/components/PageHeader.tsx`

**Checkpoint**: `npx tsc --noEmit` con cero errores.

---

## Phase 7: User Story 5 — Backend: Controladores en Subcarpeta de Módulo (Priority: P3)

**Goal**: Cada controlador en `GymTracker.Api/Controllers/` vive en su subcarpeta de módulo. Namespace actualizado. Archivos de scaffolding eliminados.

**Independent Test**: `ls backend/src/GymTracker.Api/Controllers/` muestra solo directorios (Admin, Catalog, Diets, Meals, Progress, Routines, Settings, Workouts). `dotnet build` pasa con cero errores.

### Mover controladores y actualizar namespaces

- [x] T054 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Catalog/`, mover `CatalogController.cs` y actualizar namespace de `GymTracker.Api.Controllers` → `GymTracker.Api.Controllers.Catalog`
- [x] T055 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Diets/`, mover `DietsController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Diets`
- [x] T056 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Meals/`, mover `MealsController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Meals`
- [x] T057 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Progress/`, mover `ProgressController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Progress`
- [x] T058 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Routines/`, mover `RoutinesController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Routines`
- [x] T059 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Settings/`, mover `SettingsController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Settings`
- [x] T060 [P] [US5] Crear `backend/src/GymTracker.Api/Controllers/Workouts/`, mover `WorkoutsController.cs` y actualizar namespace → `GymTracker.Api.Controllers.Workouts`

### Eliminar scaffolding

- [x] T061 [P] [US5] Eliminar `backend/src/GymTracker.Api/Controllers/WeatherForecastController.cs`
- [x] T062 [P] [US5] Eliminar `backend/src/GymTracker.Api/WeatherForecast.cs`

**Checkpoint**: `dotnet build backend/GymTracker.sln` con cero errores. `dotnet test backend/GymTracker.sln` en verde.

---

## Phase 8: Polish & Validación Final

**Purpose**: Confirmar que la migración está completa y el proyecto compila limpio.

- [x] T063 [P] Verificar que no quedan archivos `.ts` sueltos en `frontend/src/services/` raíz (SC-003)
- [x] T063b [P] Verificar que `frontend/src/redux/states/workouts/workoutsState.ts` y `frontend/src/redux/states/admin/adminState.ts` exponen campos equivalentes al contrato constitucional (`list`/`form`/`error`/`loading`/`popUpCode`). Si los nombres difieren (p.ej. `items` en lugar de `list`), documentar la excepción justificada por escrito en el archivo de estado (FR-012).
- [x] T064 [P] Verificar que `frontend/src/redux/stados/` no existe (SC-004)
- [x] T065 [P] Verificar que `frontend/src/shared/types/` no existe (SC-006)
- [x] T066 Ejecutar `npx tsc --noEmit` en `frontend/` — debe retornar cero errores (SC-001)
- [x] T067 Ejecutar `dotnet build backend/GymTracker.sln` — debe retornar cero errores (SC-002)
- [ ] T068 Ejecutar `npm test` en `frontend/` — todos los tests deben pasar *(DIFERIDO: corregir imports en tests con modo agente)*
- [ ] T069 Ejecutar `dotnet test backend/GymTracker.sln` — todos los tests deben pasar *(DIFERIDO)*

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Sin dependencias — empezar aquí.
- **Phase 2 (Foundational)**: Depende de Phase 1.
- **Phase 3 (US1 — Servicios)**: Depende de Phase 2. Bloqueante para US3 (T042-T043 dependen de que `adminApi.ts` ya esté en su ubicación final).
- **Phase 4 (US2 — Redux)**: Independiente de Phase 3 — puede ejecutarse en paralelo con Phase 3 si hay capacidad.
- **Phase 5 (US3 — Interfaces)**: Depende de Phase 3 completa (T009). Preferiblemente después de Phase 4 para evitar conflictos de imports en los slices migrados.
- **Phase 6 (US4 — Componentes)**: Independiente de Phases 3-5 — puede ejecutarse en cualquier momento después de Phase 2.
- **Phase 7 (US5 — Backend)**: Totalmente independiente del frontend — puede ejecutarse en paralelo con cualquier fase frontend.
- **Phase 8 (Polish)**: Depende de todas las fases anteriores completas.

### User Story Dependencies

- **US1 (P1)**: Puede iniciar después de Phase 2. Sin dependencias de otras US.
- **US2 (P1)**: Puede iniciar después de Phase 2. Sin dependencias de otras US. Paralelo con US1.
- **US3 (P2)**: Depende de US1 completa (para que `adminApi.ts` esté en su ubicación final antes de T042).
- **US4 (P2)**: Sin dependencias de otras US. Puede ejecutarse en paralelo con cualquier otra.
- **US5 (P3)**: Sin dependencias del frontend. Puede ejecutarse en cualquier momento en paralelo.

### Parallel Opportunities

- **Dentro de Phase 3**: T004–T009 en paralelo; T012–T017 en paralelo; T019–T023 en paralelo.
- **Dentro de Phase 4**: T024–T026 son secuenciales (T026 rename del store requiere T024-T025 para saber las rutas a actualizar); T027–T030 pueden hacerse en paralelo tras T026; T031–T035 en paralelo.
- **Dentro de Phase 5**: T036–T041 en paralelo; T051-T053 en paralelo tras T047–T049.
- **Dentro de Phase 6**: T047–T049 en paralelo; T051–T053 en paralelo.
- **Dentro de Phase 7**: T054–T062 todos en paralelo.
- **Phase 7 completa** en paralelo con cualquier fase frontend.

---

## Implementation Strategy

**MVP (scope mínimo que entrega valor)**: Phase 3 (US1) + Phase 7 (US5) — servicios del frontend y controladores del backend conforman las correcciones de mayor impacto en navegabilidad del proyecto.

**Entrega incremental recomendada**:
1. Phase 1 + Phase 3 (US1) → frontend services limpios
2. Phase 4 (US2) → Redux organizado
3. Phase 5 (US3) + Phase 6 (US4) → interfaces y componentes consolidados
4. Phase 7 (US5) → backend controllers
5. Phase 8 → validación final

**Regla de oro**: Ejecutar `npx tsc --noEmit` (frontend) y `dotnet build` (backend) después de cada fase. No avanzar si hay errores de compilación.
