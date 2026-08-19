# Implementation Plan: Corrección de Estructura del Proyecto según Constitución

**Branch**: `003-correccion-estructura-constitucion` | **Date**: 2026-05-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/003-correccion-estructura-constitucion/spec.md`

## Summary

Reorganización estructural completa de frontend y backend para cumplir las estructuras mandatorias de la constitución del proyecto. Elimina barrels prohibidos, consolida interfaces en `interfaces/`, centraliza Redux en `redux/`, migra API services a `services/api/<modulo>/`, y organiza los controladores de backend por subcarpeta de módulo. Alcance: todos los módulos existentes en una sola iteración. Sin lógica nueva ni cambios de comportamiento en runtime.

## Technical Context

**Language/Version**: TypeScript 5 (frontend) / C# .NET 8 (backend)  
**Primary Dependencies**: React 18, Redux Toolkit, Material UI, Vite (frontend); ASP.NET Core 8, Firebase Admin SDK (backend)  
**Storage**: Firebase Firestore (usuario), Wger API (catálogo externo) — sin cambios  
**Testing**: Vitest (frontend), xUnit (backend)  
**Target Platform**: Web (browser + REST API)  
**Project Type**: Full-stack web application  
**Performance Goals**: N/A — reorganización sin impacto en runtime  
**Constraints**: Zero errores de compilación TypeScript y .NET tras cada grupo de cambios  
**Scale/Scope**: ~30 archivos a mover/modificar, ~10 barrels a eliminar, 7 controladores a reorganizar

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **OpenAPI-first**: ✅ PASS — No se agregan ni modifican endpoints. Sin impacto en contrato OpenAPI.
- **Security-first**: ✅ PASS — No se modifican endpoints ni lógica de autenticación.
- **Data boundary**: ✅ PASS — No se modifica el modelo de datos ni la integración con Firebase/Wger.
- **Reliability**: ✅ PASS — No se modifican estrategias de cache ni comportamiento degradado.
- **Quality gates**: ✅ PASS — Los tests existentes deben seguir en verde. SC-001 (tsc) y SC-002 (dotnet build) son los gates de calidad de esta feature.
- **Operations**: ✅ PASS — No se modifica logging, correlación ni health checks.

**Post-design re-check**: ✅ PASS — Research confirma que ninguna decisión de diseño viola principios constitucionales. El único riesgo real es imports rotos durante la migración, mitigado con compilación verificada por grupo de cambios.

## Project Structure

### Documentation (this feature)

```text
specs/003-correccion-estructura-constitucion/
├── plan.md              ← este archivo
├── research.md          ← mapa completo de migraciones
├── data-model.md        ← nota sobre ausencia de cambios en entidades
├── quickstart.md        ← estructura resultante + comandos de validación
└── tasks.md             ← generado por /speckit.tasks
```

### Source Code — Estructura resultante

```text
frontend/src/
├── components/
│   └── common/
│       ├── AsyncState.tsx           ← implementación real (fusión desde shared/components/)
│       ├── FormPopupDialog.tsx      ← implementación real
│       └── PageHeader.tsx           ← implementación real
├── features/
│   ├── admin/     (components/, pages/)
│   ├── catalog/   (components/)
│   ├── diets/     (components/, pages/)
│   ├── meals/     (components/, pages/)
│   ├── progress/  (components/, pages/)
│   ├── routines/  (components/, pages/)
│   ├── settings/  (components/, pages/)
│   ├── user/      (pages/)
│   └── workouts/  (components/, pages/)
│   [sin subcarpetas api/ ni state/ — migradas a services/ y redux/]
├── interfaces/
│   ├── admin.ts       ← nuevo; tipos extraídos de features/admin/api/adminApi.ts
│   ├── catalog.ts     ← contenido real fusionado desde shared/types/catalog.ts
│   ├── diets.ts       ← contenido real
│   ├── meals.ts       ← contenido real
│   ├── routines.ts    ← contenido real
│   ├── settings.ts    ← contenido real
│   └── workouts.ts    ← contenido real
├── redux/
│   ├── actions/
│   │   └── workouts/  ← vacía (lista para thunks futuros)
│   ├── hooks/
│   │   └── reduxHooks.ts
│   ├── reducers/
│   │   └── workouts/  ← vacía (lista para reducers futuros)
│   ├── states/
│   │   ├── admin/
│   │   │   └── adminState.ts     ← migrado desde features/admin/state/adminSlice.ts
│   │   └── workouts/
│   │       └── workoutsState.ts  ← migrado desde redux/stados/workoutsState.ts
│   └── store.ts                  ← renombrado desde redux/globalState.ts
├── services/
│   ├── api/
│   │   ├── admin/
│   │   │   └── adminApi.ts       ← migrado desde features/admin/api/adminApi.ts
│   │   ├── diets/
│   │   │   └── dietsApi.ts       ← migrado desde features/diets/api/dietsApi.ts
│   │   ├── meals/
│   │   │   └── mealsApi.ts       ← migrado desde features/meals/api/mealsApi.ts
│   │   ├── routines/
│   │   │   └── routinesApi.ts    ← migrado desde features/routines/api/routinesApi.ts
│   │   ├── settings/
│   │   │   └── preferencesApi.ts ← migrado desde features/settings/api/preferencesApi.ts
│   │   └── workouts/
│   │       └── workoutsApi.ts    ← migrado desde services/workouts/workoutsApi.ts
│   ├── firebase/
│   ├── observability/
│   └── storage/
└── shared/
    ├── api/
    │   └── httpClient.ts          ← sin cambios
    └── components/
        ├── AppHeader.tsx          ← fuera de alcance (sin duplicado en components/)
        ├── AppFooter.tsx          ← fuera de alcance
        ├── AppLayout.tsx          ← fuera de alcance
        ├── WorkoutImmersiveLayout.tsx ← fuera de alcance
        └── ui/                    ← fuera de alcance

backend/src/GymTracker.Api/
├── Controllers/
│   ├── Admin/      (sin cambios)
│   ├── Catalog/    └── CatalogController.cs
│   ├── Diets/      └── DietsController.cs
│   ├── Meals/      └── MealsController.cs
│   ├── Progress/   └── ProgressController.cs
│   ├── Routines/   └── RoutinesController.cs
│   ├── Settings/   └── SettingsController.cs
│   └── Workouts/   └── WorkoutsController.cs
└── [WeatherForecast.cs eliminado]
```

**Structure Decision**: Full-stack web application (frontend + backend en el mismo repositorio). La reorganización no agrega proyectos ni cambia la topología del repo.

## Complexity Tracking

Sin violaciones constitucionales. No aplica.
