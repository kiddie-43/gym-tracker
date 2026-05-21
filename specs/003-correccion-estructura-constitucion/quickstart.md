# Quickstart: Corrección de Estructura del Proyecto según Constitución

**Feature**: 003-correccion-estructura-constitucion  
**Date**: 2026-05-15

## Descripción del cambio

Reorganización de archivos en frontend y backend para cumplir la estructura mandatoria definida en la constitución. No hay lógica nueva ni cambios de comportamiento en runtime.

## Comandos de validación

### Frontend — verificar compilación

```bash
cd frontend
npm run build
# o solo check de tipos sin build:
npx tsc --noEmit
```

### Frontend — ejecutar tests existentes

```bash
cd frontend
npm test
```

### Backend — verificar compilación

```bash
cd backend
dotnet build GymTracker.sln
```

### Backend — ejecutar tests existentes

```bash
cd backend
dotnet test GymTracker.sln
```

## Estructura resultante tras la migración

### Frontend

```
frontend/src/
├── components/
│   ├── common/
│   │   ├── AsyncState.tsx           ← implementación real (ya no barrel)
│   │   ├── FormPopupDialog.tsx      ← implementación real (ya no barrel)
│   │   └── PageHeader.tsx           ← implementación real (ya no barrel)
│   ├── catalog/
│   ├── layout/
│   ├── meals/
│   ├── progress/
│   ├── routines/
│   ├── settings/
│   └── workouts/
├── features/
│   ├── admin/
│   │   ├── components/              ← sin cambios
│   │   └── pages/                   ← sin cambios
│   │   (sin subcarpeta api/ ni state/)
│   ├── catalog/
│   ├── diets/
│   │   ├── components/
│   │   └── pages/
│   │   (sin subcarpeta api/)
│   ├── meals/
│   ├── routines/
│   ├── settings/
│   ├── workouts/
│   │   ├── components/
│   │   └── pages/
│   │   (sin subcarpeta state/ ni api/)
│   ├── progress/
│   └── user/
├── interfaces/
│   ├── admin.ts                     ← nuevo, tipos de admin separados
│   ├── catalog.ts                   ← contenido real (ya no barrel)
│   ├── diets.ts                     ← contenido real (ya no barrel)
│   ├── meals.ts                     ← contenido real (ya no barrel)
│   ├── routines.ts                  ← contenido real (ya no barrel)
│   ├── settings.ts                  ← contenido real (ya no barrel)
│   └── workouts.ts                  ← contenido real (ya no barrel)
├── redux/
│   ├── actions/
│   │   └── workouts/                ← vacía (para thunks futuros)
│   ├── hooks/
│   │   └── reduxHooks.ts
│   ├── reducers/
│   │   └── workouts/                ← vacía (para reducers futuros)
│   ├── states/
│   │   ├── admin/
│   │   │   └── adminState.ts        ← migrado desde features/admin/state/adminSlice.ts
│   │   └── workouts/
│   │       └── workoutsState.ts     ← migrado desde redux/stados/workoutsState.ts
│   └── store.ts                     ← renombrado desde globalState.ts
├── services/
│   ├── api/
│   │   ├── admin/
│   │   │   └── adminApi.ts          ← migrado desde features/admin/api/adminApi.ts
│   │   ├── diets/
│   │   │   └── dietsApi.ts          ← migrado desde features/diets/api/dietsApi.ts
│   │   ├── meals/
│   │   │   └── mealsApi.ts          ← migrado desde features/meals/api/mealsApi.ts
│   │   ├── routines/
│   │   │   └── routinesApi.ts       ← migrado desde features/routines/api/routinesApi.ts
│   │   ├── settings/
│   │   │   └── preferencesApi.ts    ← migrado desde features/settings/api/preferencesApi.ts
│   │   └── workouts/
│   │       └── workoutsApi.ts       ← migrado desde services/workouts/workoutsApi.ts
│   ├── firebase/
│   ├── observability/
│   └── storage/
└── shared/
    ├── api/
    │   └── httpClient.ts            ← sin cambios
    └── components/
        ├── AppHeader.tsx            ← fuera de alcance
        ├── AppFooter.tsx            ← fuera de alcance
        ├── AppLayout.tsx            ← fuera de alcance
        ├── WorkoutImmersiveLayout.tsx ← fuera de alcance
        └── ui/                      ← fuera de alcance
```

### Backend

```
backend/src/GymTracker.Api/
├── Controllers/
│   ├── Admin/
│   │   ├── ExerciseFormTypesController.cs
│   │   ├── ExercisesController.cs
│   │   ├── ExerciseTypesController.cs
│   │   ├── MuscleGroupsController.cs
│   │   └── MusclesController.cs
│   ├── Catalog/
│   │   └── CatalogController.cs     ← movido + namespace actualizado
│   ├── Diets/
│   │   └── DietsController.cs       ← movido + namespace actualizado
│   ├── Meals/
│   │   └── MealsController.cs       ← movido + namespace actualizado
│   ├── Progress/
│   │   └── ProgressController.cs    ← movido + namespace actualizado
│   ├── Routines/
│   │   └── RoutinesController.cs    ← movido + namespace actualizado
│   ├── Settings/
│   │   └── SettingsController.cs    ← movido + namespace actualizado
│   └── Workouts/
│       └── WorkoutsController.cs    ← movido + namespace actualizado
│   (WeatherForecastController.cs eliminado)
└── WeatherForecast.cs               ← eliminado
```

## Notas de migración

### TypeScript — patrón de migración de barrels

Antes (barrel en `interfaces/workouts.ts`):
```typescript
export type { WorkoutSummary, ... } from '../shared/types/workouts';
```

Después (contenido real en `interfaces/workouts.ts`):
```typescript
export type WorkoutSummary = { ... };
export type WorkoutSetInput = { ... };
// ...
```

### C# — patrón de migración de namespace

Antes (`Controllers/WorkoutsController.cs`):
```csharp
namespace GymTracker.Api.Controllers;
```

Después (`Controllers/Workouts/WorkoutsController.cs`):
```csharp
namespace GymTracker.Api.Controllers.Workouts;
```
