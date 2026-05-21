# Feature Specification: Corrección de Estructura del Proyecto según Constitución

**Feature Branch**: `003-correccion-estructura-constitucion`
**Created**: 2026-05-15
**Status**: Draft

## Contexto

La constitución del proyecto define estructuras obligatorias para frontend y backend. Al analizar el proyecto actual, se identificaron desvíos estructurales que deben corregirse para cumplir con los estándares definidos.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Estructura de Servicios del Frontend Corregida (Priority: P1)

Un desarrollador que trabaja en el frontend puede ubicar cualquier servicio en el lugar correcto según su tipo (API, Firebase, storage, observabilidad), sin ambigüedad ni duplicados.

**Why this priority**: La estructura de servicios es transversal a todo el proyecto. Si no está ordenada, cada nueva feature agrava el desorden.

**Independent Test**: Al inspeccionar `frontend/src/services/`, todos los servicios existentes están dentro de la subcarpeta correcta (`api/`, `firebase/`, `storage/`, `observability/`). No hay archivos `.ts` sueltos en la raíz de `services/`.

**Acceptance Scenarios**:

1. **Given** la carpeta `services/` con archivos sueltos (`dietsService.ts`, `mealsService.ts`, `routinesService.ts`, `preferencesService.ts`, `workoutsService.ts`, `httpClient.ts`), **When** se completa la migración, **Then** todos esos archivos están relocalizados dentro de `services/api/<modulo>/` o `services/firebase/<modulo>/` según corresponda.
2. **Given** que `httpClient.ts` actualmente vive en `services/`, **When** se migra, **Then** vive en `shared/api/httpClient.ts` (ya existe; el de `services/` se elimina).
3. **Given** las carpetas `services/api/`, `services/firebase/`, `services/storage/`, `services/observability/` vacías, **When** se completa la migración, **Then** cada una contiene subcarpetas por módulo con el servicio correspondiente.

---

### User Story 2 - Estructura de Redux Corregida (Priority: P1)

Un desarrollador puede encontrar actions, reducers y states de cualquier módulo en las carpetas correctas con nombres consistentes, sin carpetas con typos ni archivos sueltos.

**Why this priority**: Redux es el estado global de la app; la inconsistencia aquí afecta directamente la navegabilidad y el mantenimiento de todos los módulos existentes y futuros.

**Independent Test**: Al inspeccionar `frontend/src/redux/`, existen exactamente `actions/`, `reducers/`, `states/` (y opcionalmente `hooks/`). No existe `stados/`. Cada módulo tiene su subcarpeta en las tres capas.

**Acceptance Scenarios**:

1. **Given** que existe `redux/stados/` (typo) con `workoutsState.ts`, **When** se corrige, **Then** el contenido está en `redux/states/workouts/` y la carpeta `stados/` ha sido eliminada.
2. **Given** que `redux/actions/workoutsActions.ts` es un archivo suelto fuera de subcarpeta de módulo, **When** se corrige, **Then** el contenido de las acciones del módulo workouts está en `redux/actions/workouts/`.
3. **Given** que `redux/reducers/` está vacía, **When** se completa la migración, **Then** contiene subcarpetas por módulo con el reducer correspondiente.
4. **Given** que el estado de workouts vive en `features/workouts/state/workoutsSlice.ts`, **When** se migra, **Then** su lógica está distribuida en `redux/states/workouts/`, `redux/reducers/workouts/` y `redux/actions/workouts/`, y la carpeta `features/workouts/state/` es eliminada.

---

### User Story 3 - Interfaces y Tipos sin Duplicados (Priority: P2)

Un desarrollador puede importar interfaces de dominio desde una sola fuente de verdad por módulo, sin ambigüedad entre `interfaces/`, `shared/types/` y `types/`.

**Why this priority**: Actualmente existen interfaces duplicadas entre `interfaces/` y `shared/types/`, y `types/admin/` está vacía. Esto genera confusión sobre cuál es la fuente canónica.

**Independent Test**: Al inspeccionar el proyecto, las interfaces de dominio de cada módulo existen en un único lugar canónico definido por la constitución. No hay definiciones duplicadas del mismo tipo en dos carpetas distintas.

**Acceptance Scenarios**:

1. **Given** que `interfaces/` y `shared/types/` contienen archivos con los mismos módulos, **When** se completa la fusión, **Then** `interfaces/<modulo>.ts` contiene todos los tipos de ambas fuentes sin duplicados y `shared/types/` ha sido eliminada.
2. **Given** todos los imports existentes que apuntan a `shared/types/<modulo>`, **When** se completa la migración, **Then** todos apuntan a `interfaces/<modulo>` y el proyecto compila sin errores.
3. **Given** que `types/admin/` está vacía, **When** se consolida, **Then** se elimina o se documenta su propósito diferenciado respecto a `interfaces/`.

---

### User Story 4 - Componentes sin Duplicados entre `components/` y `shared/components/` (Priority: P2)

Un desarrollador sabe dónde vive cada componente reutilizable y no encuentra el mismo componente (o uno equivalente) en dos lugares distintos.

**Why this priority**: Actualmente `AsyncState.tsx`, `FormPopupDialog.tsx` y `PageHeader.tsx` existen tanto en `components/common/` como en `shared/components/`.

**Independent Test**: Al buscar `AsyncState`, `FormPopupDialog` y `PageHeader` en el proyecto, aparecen en una sola ubicación canónica.

**Acceptance Scenarios**:

1. **Given** que `components/common/AsyncState.tsx` y `shared/components/AsyncState.tsx` existen simultáneamente, **When** se consolida, **Then** solo existe en `components/common/` y todos los imports del proyecto apuntan ahí.
2. **Given** que lo mismo ocurre con `FormPopupDialog.tsx` y `PageHeader.tsx`, **When** se consolida, **Then** ambos tienen únicamente la copia en `components/common/` y sus duplicados en `shared/components/` son eliminados.

---

### User Story 5 - Backend: Controladores dentro de subcarpeta de módulo en `Controllers/` (Priority: P3)

Los controladores del backend siguen la misma convención de organización por módulo que ya existe en `Controllers/Admin/`.

**Why this priority**: La constitución exige organización por módulo en todas las capas. Los controladores actualmente viven sueltos en `Controllers/` excepto Admin.

**Independent Test**: Al inspeccionar `GymTracker.Api/Controllers/`, cada controlador vive dentro de una subcarpeta con el nombre de su módulo.

**Acceptance Scenarios**:

1. **Given** que `CatalogController.cs`, `DietsController.cs`, `MealsController.cs`, etc. están sueltos en `Controllers/`, **When** se reorganiza, **Then** cada uno está en `Controllers/<Modulo>/`.
2. **Given** que `WeatherForecastController.cs` es un archivo de andamiaje inicial, **When** se reorganiza, **Then** se elimina junto con `WeatherForecast.cs` del proyecto.

---

### Edge Cases

- ¿Qué pasa si un servicio de frontend cumple roles de múltiples categorías (por ejemplo, un servicio que hace llamadas API y también usa Firebase)? → Se ubica según su responsabilidad principal; si es genuinamente mixto, se divide.
- ¿Qué pasa con los imports rotos al mover archivos? → Todos los imports deben actualizarse como parte de cada corrección.
- Los archivos en `redux/states/workouts/` y `redux/reducers/workouts/` que están vacíos MUST poblarse con el contenido migrado desde `features/workouts/state/workoutsSlice.ts`.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Frontend — Servicios

- **FR-001**: Los servicios de llamadas a APIs REST de TODOS los módulos (workouts, diets, meals, routines, catalog, settings, progress, admin) MUST ubicarse en `services/api/<modulo>/`.
- **FR-002**: Los servicios de Firebase MUST ubicarse en `services/firebase/<modulo>/`.
- **FR-003**: Los servicios de almacenamiento local/sesión MUST ubicarse en `services/storage/<modulo>/`.
- **FR-004**: Los servicios de logging y métricas MUST ubicarse en `services/observability/<modulo>/`.
- **FR-005**: No MUST existir archivos `.ts` sueltos en la raíz de `services/`.
- **FR-005b**: No MUST existir archivos barrel (re-exportaciones centralizadas) en `services/`. `workoutsService.ts` (que solo re-exporta desde `workouts/workoutsApi.ts`) MUST eliminarse; los imports deben actualizarse para apuntar directamente a `services/api/workouts/workoutsApi.ts`.
- **FR-006**: `httpClient.ts` MUST vivir en `shared/api/httpClient.ts` (ya existe; el duplicado en `services/` MUST eliminarse).

#### Frontend — Redux

- **FR-007**: La carpeta `redux/stados/` (typo) MUST eliminarse.
- **FR-008**: El contenido de `redux/stados/workoutsState.ts` MUST migrarse a `redux/states/workouts/`.
- **FR-009**: La carpeta `redux/actions/` MUST organizarse por módulo en subcarpetas (`redux/actions/<modulo>/`).
- **FR-010**: El archivo suelto `redux/actions/workoutsActions.ts` MUST eliminarse — es un barrel de re-exportación prohibido por la constitución. Las acciones del módulo workouts se importan directamente desde el slice en `redux/states/workouts/workoutsState.ts`. La carpeta `redux/actions/workouts/` MUST existir y quedará lista para albergar thunks independientes futuros.
- **FR-011**: La carpeta `redux/reducers/` MUST organizarse por módulo en subcarpetas (`redux/reducers/<modulo>/`).
- **FR-012**: El estado de cada módulo MUST cumplir el contrato: `list`, `form`, `error`, `loading`, `popUpCode`.
- **FR-012b**: El patrón de organización Redux MUST ser centralizado (`redux/actions/<modulo>/`, `redux/reducers/<modulo>/`, `redux/states/<modulo>/`). No se permite el patrón feature-slices (`features/<modulo>/state/`).

#### Frontend — Interfaces y Tipos

- **FR-013**: `interfaces/` MUST ser la única fuente canónica de interfaces de dominio. Todo tipo existente en `shared/types/` MUST fusionarse en `interfaces/<modulo>.ts` sin duplicar definiciones existentes.
- **FR-013b**: La carpeta `shared/types/` MUST eliminarse una vez completada la fusión, y todos los imports en el proyecto MUST actualizarse para apuntar a `interfaces/<modulo>`.
- **FR-014**: La carpeta `types/admin/` vacía MUST eliminarse o poblarse con contenido justificado.

#### Frontend — Componentes

- **FR-015**: Los componentes `AsyncState.tsx`, `FormPopupDialog.tsx` y `PageHeader.tsx` MUST existir únicamente en `components/common/`. Las copias en `shared/components/` MUST eliminarse.
- **FR-016**: Todos los imports de dichos componentes MUST actualizarse para apuntar a `components/common/<Componente>`.
- **FR-016b**: Los componentes exclusivos de `shared/components/` (`AppHeader.tsx`, `AppFooter.tsx`, `AppLayout.tsx`, `WorkoutImmersiveLayout.tsx`, `ui/`) que no tienen duplicado en `components/` quedan fuera del alcance de esta feature hasta que el plan determine su ubicación definitiva.

#### Backend — Controladores

- **FR-017**: Cada controlador en `GymTracker.Api/Controllers/` MUST residir dentro de una subcarpeta con el nombre de su módulo.
- **FR-018**: `WeatherForecastController.cs` y `WeatherForecast.cs` (andamiaje inicial) MUST eliminarse del proyecto.

### Key Entities

- **Módulo**: unidad funcional del dominio (workouts, diets, meals, routines, catalog, settings, admin, progress). Nombre consistente en todas las capas.
- **Servicio**: clase o función que encapsula lógica de negocio o acceso a datos, ubicada en la subcarpeta de su tipo (`api/`, `firebase/`, `storage/`, `observability/`).
- **Slice / State**: unidad de estado Redux por módulo, con contrato de campos obligatorios.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: Esta feature no introduce nuevas integraciones externas; es puramente una reorganización estructural.
- **INT-002**: Los servicios de Firebase que ya existen en `Infrastructure/Firebase/` en backend no se ven afectados.
- **INT-003**: No aplica fallback a externos; si los imports se rompen, los errores de compilación/tipado son la señal de alerta.
- **INT-004**: No hay entidades de catálogo externo involucradas.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al ejecutar la compilación del frontend, cero errores de import roto como resultado de la reorganización.
- **SC-002**: Al ejecutar la compilación del backend, cero errores de compilación tras eliminar los archivos de andamiaje y reorganizar los controladores.
- **SC-003**: Al inspeccionar `frontend/src/services/`, cero archivos `.ts` sueltos en la raíz de la carpeta. Todos los módulos tienen su subcarpeta dentro de `api/`, `firebase/`, `storage/` u `observability/` según corresponda.
- **SC-004**: Al inspeccionar `frontend/src/redux/`, no existe la carpeta `stados/` y existen exactamente `actions/`, `reducers/`, `states/` organizados por módulo.
- **SC-005**: Al buscar `AsyncState`, `FormPopupDialog` y `PageHeader` en el proyecto, aparecen únicamente en `components/common/`. No existen copias en `shared/components/`.
- **SC-006**: Al buscar interfaces de cualquier módulo, se encuentran únicamente en `interfaces/<modulo>.ts`. La carpeta `shared/types/` no existe en el proyecto.
- **SC-007**: Al inspeccionar `GymTracker.Api/Controllers/`, todos los controladores están dentro de subcarpetas por módulo y no existe `WeatherForecastController.cs`.

---

## Clarifications

### Session 2026-05-15

- Q: ¿El estado Redux de los módulos debe consolidarse en `redux/` centralizado o mantenerse como feature-slices en `features/<modulo>/state/`? → A: Migrar todo a `redux/actions/`, `redux/reducers/`, `redux/states/` centralizados, cumpliendo la constitución.
- Q: ¿Cuál es la fuente canónica de interfaces de dominio: `interfaces/` o `shared/types/`? → A: `interfaces/` es la fuente canónica. Todo el contenido de `shared/types/` se fusiona en `interfaces/` (sin duplicar ni borrar tipos), y la carpeta `shared/types/` se elimina.
- Q: ¿Cuál es la ubicación canónica de componentes reutilizables: `components/` o `shared/components/`? → A: `components/` es la fuente canónica. Los duplicados en `shared/components/` se eliminan y sus imports se actualizan. El contenido exclusivo de `shared/components/` (layouts, ui base) se evalúa caso a caso en el plan.
- Q: ¿Esta feature cubre todos los módulos o solo `workouts` como piloto? → A: Todos los módulos existentes (workouts, diets, meals, routines, catalog, settings, progress, admin) se reorganizan en esta única iteración.

## Assumptions

- El estado Redux activo del módulo workouts (`features/workouts/state/workoutsSlice.ts`) MUST migrarse a `redux/states/workouts/`, `redux/reducers/workouts/` y `redux/actions/workouts/` siguiendo la estructura centralizada de la constitución. No se usa el patrón feature-slices.
- La fuente canónica de interfaces es `interfaces/<modulo>.ts`. La carpeta `shared/types/` debe fusionarse completamente en `interfaces/` y luego eliminarse. No se conserva como complemento.
- Todos los módulos existentes (workouts, diets, meals, routines, catalog, settings, progress, admin) MUST reorganizarse en esta feature. No se permite dejar ningún módulo en estado intermedio.
- Los archivos de servicio sueltos en `services/` de cada módulo migran a `services/api/<modulo>/`: `dietsService.ts` → `services/api/diets/`, `mealsService.ts` → `services/api/meals/`, `routinesService.ts` → `services/api/routines/`, `preferencesService.ts` → `services/api/settings/`, `workoutsApi.ts` → `services/api/workouts/`.
- Se asume que no existen tests activos sobre `WeatherForecastController.cs` que deban conservarse.
- El alcance de esta feature es exclusivamente reorganización estructural; no introduce lógica nueva ni modifica comportamiento en runtime.
