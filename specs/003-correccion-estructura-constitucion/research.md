# Research: Corrección de Estructura del Proyecto según Constitución

**Feature**: 003-correccion-estructura-constitucion  
**Date**: 2026-05-15  
**Status**: Complete — todos los NEEDS CLARIFICATION resueltos

---

## 1. Mapa completo de migraciones

### 1.1 Frontend — Servicios

**Hallazgo crítico**: Todos los archivos sueltos en `services/` raíz son barrels de re-exportación (prohibidos por constitución). Las implementaciones reales viven en `features/<modulo>/api/`. Deben moverse a `services/api/<modulo>/` y los barrels se eliminan.

| Archivo actual | Tipo | Acción | Destino |
|----------------|------|--------|---------|
| `services/dietsService.ts` | Barrel → `features/diets/api/dietsApi` | ELIMINAR | — |
| `services/mealsService.ts` | Barrel → `features/meals/api/mealsApi` | ELIMINAR | — |
| `services/routinesService.ts` | Barrel → `features/routines/api/routinesApi` | ELIMINAR | — |
| `services/preferencesService.ts` | Barrel → `features/settings/api/preferencesApi` | ELIMINAR | — |
| `services/workoutsService.ts` | Barrel → `services/workouts/workoutsApi` | ELIMINAR | — |
| `services/httpClient.ts` | Duplicado de `shared/api/httpClient.ts` | ELIMINAR | — |
| `services/workouts/workoutsApi.ts` | Implementación real | MOVER | `services/api/workouts/workoutsApi.ts` |
| `features/diets/api/dietsApi.ts` | Implementación real | MOVER | `services/api/diets/dietsApi.ts` |
| `features/meals/api/mealsApi.ts` | Implementación real | MOVER | `services/api/meals/mealsApi.ts` |
| `features/routines/api/routinesApi.ts` | Implementación real | MOVER | `services/api/routines/routinesApi.ts` |
| `features/settings/api/preferencesApi.ts` | Implementación real | MOVER | `services/api/settings/preferencesApi.ts` |
| `features/admin/api/adminApi.ts` | Implementación real | MOVER | `services/api/admin/adminApi.ts` |

**Post-migración**: Las carpetas `features/<modulo>/api/` vacías se eliminan.

**Decisión — Rationale**: La constitución exige `services/api/<modulo>/` para toda implementación de llamada a API. Las implementaciones en `features/<modulo>/api/` son semánticamente servicios de API, no lógica de componentes ni estado. Moverlas garantiza trazabilidad y previene que cada feature cree su propio subdirectorio de API ad hoc.

---

### 1.2 Frontend — Redux

**Hallazgo crítico**: Existen tres tipos de archivos en redux: (a) implementaciones reales de slices RTK, (b) barrels de re-exportación, (c) carpetas con typo. El store está en `redux/globalState.ts` en lugar de `redux/store.ts`.

| Archivo actual | Tipo | Acción | Destino |
|----------------|------|--------|---------|
| `redux/stados/workoutsState.ts` | Slice RTK completo | MOVER | `redux/states/workouts/workoutsState.ts` |
| `redux/stados/` (carpeta) | Typo | ELIMINAR (vacía tras move) | — |
| `redux/actions/workoutsActions.ts` | Barrel | ELIMINAR | — |
| `redux/actions/workouts/` (vacía) | Destino correcto | MANTENER | — |
| `redux/reducers/workouts/` (vacía) | Destino correcto | MANTENER | — |
| `redux/states/workouts/` (vacía) | Destino correcto | POBLADA con slice | — |
| `redux/globalState.ts` | Store config | RENOMBRAR | `redux/store.ts` |
| `features/workouts/state/workoutsSlice.ts` | Barrel → `redux/stados/workoutsState` | ELIMINAR | — |
| `features/admin/state/adminSlice.ts` | Slice RTK completo | MOVER | `redux/states/admin/adminState.ts` |
| `features/admin/state/` (carpeta) | Feature-slice | ELIMINAR (vacía tras move) | — |
| `app/store.ts` | Barrel → `redux/globalState` | ACTUALIZAR imports | apunta a `redux/store` |

**Decisión — Redux Toolkit Slice con estructura constitucional**:

- Decision: Mantener el slice RTK completo en `redux/states/<modulo>/<modulo>State.ts`. Un slice RTK en un solo archivo ya define state, actions y reducers; no es necesario ni práctico fragmentarlo en tres archivos separados para este proyecto.
- Las carpetas `redux/actions/<modulo>/` y `redux/reducers/<modulo>/` se crean y mantienen vacías inicialmente; se usarán cuando un módulo requiera async thunks aislados o composición de reducers.
- Alternatives considered: Fragmentar el slice en tres archivos (state, actions, reducers) — rechazado porque crea acoplamiento circular con RTK y duplica esfuerzo sin beneficio real para el tamaño actual del proyecto.

**Decisión — `redux/globalState.ts` → `redux/store.ts`**:
- Decision: Renombrar el archivo a `store.ts` para cumplir exactamente la estructura constitucional (`redux/store.ts`).
- `app/store.ts` se mantiene como re-export transitorio (no es un servicio, sino acceso al store desde la capa de UI). Se actualizan sus imports.

---

### 1.3 Frontend — Interfaces y Tipos

**Hallazgo crítico**: `interfaces/<modulo>.ts` son barrels que re-exportan desde `shared/types/<modulo>.ts`. Las implementaciones reales están en `shared/types/`. La migración invierte esto: el contenido real pasa a `interfaces/` y los barrels desaparecen.

| Archivo actual | Tipo | Acción |
|----------------|------|--------|
| `interfaces/catalog.ts` | Barrel → `shared/types/catalog` | Reemplazar con contenido real de `shared/types/catalog.ts` |
| `interfaces/diets.ts` | Barrel | Reemplazar con contenido real de `shared/types/diets.ts` |
| `interfaces/meals.ts` | Barrel | Reemplazar con contenido real de `shared/types/meals.ts` |
| `interfaces/routines.ts` | Barrel | Reemplazar con contenido real de `shared/types/routines.ts` |
| `interfaces/settings.ts` | Barrel | Reemplazar con contenido real de `shared/types/settings.ts` |
| `interfaces/workouts.ts` | Barrel | Reemplazar con contenido real de `shared/types/workouts.ts` |
| `shared/types/` (carpeta) | Implementaciones reales | ELIMINAR tras fusión |
| `types/admin/` (carpeta vacía) | Sin contenido | ELIMINAR |

**Post-fusión**: Todos los imports que referencian `shared/types/<modulo>` se actualizan a `interfaces/<modulo>`. Los imports que ya usan `interfaces/<modulo>` no cambian (ya apuntaban al barrel que ahora tiene el contenido real).

**Decisión — Rationale**: La constitución nombra explícitamente `interfaces/` como carpeta obligatoria. `shared/types/` no está definida en la estructura constitucional y fue creada antes de la constitución. La fusión consolida sin pérdida de tipos.

---

### 1.4 Frontend — Componentes

**Hallazgo crítico**: Mismo patrón barrel. `components/common/*.tsx` son barrels apuntando a `shared/components/`. La implementación real está en `shared/components/`.

| Archivo actual | Tipo | Acción |
|----------------|------|--------|
| `components/common/AsyncState.tsx` | Barrel → `shared/components/AsyncState` | Reemplazar con contenido real |
| `components/common/FormPopupDialog.tsx` | Barrel → `shared/components/FormPopupDialog` | Reemplazar con contenido real |
| `components/common/PageHeader.tsx` | Barrel → `shared/components/PageHeader` | Reemplazar con contenido real |
| `shared/components/AsyncState.tsx` | Implementación real | ELIMINAR tras mover |
| `shared/components/FormPopupDialog.tsx` | Implementación real | ELIMINAR tras mover |
| `shared/components/PageHeader.tsx` | Implementación real | ELIMINAR tras mover |

**Fuera de alcance (FR-016b)**:
- `shared/components/AppHeader.tsx`
- `shared/components/AppFooter.tsx`
- `shared/components/AppLayout.tsx`
- `shared/components/WorkoutImmersiveLayout.tsx`
- `shared/components/ui/` (`CompactNumberInput.tsx`, `ImageAutocomplete.tsx`)

Estos componentes no tienen duplicado en `components/` y su clasificación (¿deben ir a `components/layout/`?) se define en la siguiente feature de ajuste fino de componentes.

**Decisión — Rationale**: La constitución define `components/` como la carpeta de componentes reutilizables. El movimiento de implementaciones desde `shared/components/` a `components/common/` es la dirección lógica.

---

### 1.5 Backend — Controladores

**Hallazgo crítico**: Solo `Controllers/Admin/` sigue la convención de subcarpeta por módulo. Los demás controladores están sueltos. El namespace en C# debe reflejar la nueva ubicación.

| Archivo actual | Namespace actual | Acción | Namespace destino |
|----------------|-----------------|--------|-------------------|
| `Controllers/CatalogController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Catalog/` | `GymTracker.Api.Controllers.Catalog` |
| `Controllers/DietsController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Diets/` | `GymTracker.Api.Controllers.Diets` |
| `Controllers/MealsController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Meals/` | `GymTracker.Api.Controllers.Meals` |
| `Controllers/ProgressController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Progress/` | `GymTracker.Api.Controllers.Progress` |
| `Controllers/RoutinesController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Routines/` | `GymTracker.Api.Controllers.Routines` |
| `Controllers/SettingsController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Settings/` | `GymTracker.Api.Controllers.Settings` |
| `Controllers/WorkoutsController.cs` | `GymTracker.Api.Controllers` | MOVER a `Controllers/Workouts/` | `GymTracker.Api.Controllers.Workouts` |
| `Controllers/WeatherForecastController.cs` | `GymTracker.Api.Controllers` | ELIMINAR | — |
| `WeatherForecast.cs` (raíz del proyecto) | `GymTracker.Api` | ELIMINAR | — |

**Decisión — Namespace**: En ASP.NET Core, el namespace del controlador no afecta el routing HTTP (que se define por `[Route]` attribute). Cambiar el namespace es obligatorio para consistencia de convención pero no rompe ningún endpoint existente.

**Decisión — WeatherForecast**: Confirmado como scaffolding de dotnet new que nunca fue limpiado. No existen tests activos que lo cubran (verificado en `tests/GymTracker.Api.IntegrationTests/`).

---

## 2. Impacto en imports — mapa de actualización

### 2.1 Imports que cambian por migración de servicios

Todos los imports de `features/<modulo>/api/<modulo>Api` → `services/api/<modulo>/<modulo>Api`.  
Todos los imports de `services/<modulo>Service` → `services/api/<modulo>/<modulo>Api` (directo, sin barrel).

### 2.2 Imports que cambian por migración de interfaces

Todos los imports de `shared/types/<modulo>` → `interfaces/<modulo>`.  
Los imports que ya usan `interfaces/<modulo>` no cambian (el contenido ahora está en ese archivo directamente).

### 2.3 Imports que cambian por migración de Redux

- `redux/stados/workoutsState` → `redux/states/workouts/workoutsState`
- `features/admin/state/adminSlice` → `redux/states/admin/adminState`
- `redux/globalState` → `redux/store`

### 2.4 Imports de componentes comunes

Los imports que usan `components/common/AsyncState|FormPopupDialog|PageHeader` no cambian (misma ruta, el contenido ahora es real).  
Los imports que usan `shared/components/AsyncState|FormPopupDialog|PageHeader` → `components/common/<Componente>`.

---

## 3. Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Import roto tras mover archivos | Build error (TypeScript) | Ejecutar `tsc --noEmit` tras cada grupo de cambios |
| Namespace incorrecto en C# tras mover controladores | Build error (.NET) | Verificar con `dotnet build` tras cada move |
| Barrel file olvidado que referencia ruta antigua | Runtime error silencioso | Buscar referencias con grep tras cada delete |
| `app/store.ts` con import a `globalState` después del rename | Build error | Actualizar en el mismo paso del rename |
| `features/workouts/state/workoutsSlice.ts` importado en algún lugar no encontrado | Import roto | Grep exhaustivo antes de eliminar |

---

## 4. Resolución de NEEDS CLARIFICATION

Todos los puntos resueltos durante la fase de clarificación. No quedan elementos sin resolver.

| NEEDS CLARIFICATION | Resolución |
|--------------------|------------|
| Redux feature-slices vs centralizado | Centralizado en `redux/` |
| Fuente canónica de interfaces | `interfaces/` (fusión desde `shared/types/`) |
| Componentes canónicos | `components/` (fusión desde `shared/components/` para duplicados) |
| Alcance de módulos | Todos los módulos en esta iteración |
| `workoutsService.ts` vs `workoutsApi.ts` | `workoutsService.ts` es barrel obsoleto; se elimina |
