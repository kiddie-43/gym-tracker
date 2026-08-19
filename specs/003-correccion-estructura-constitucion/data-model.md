# Data Model: Corrección de Estructura del Proyecto según Constitución

**Feature**: 003-correccion-estructura-constitucion  
**Date**: 2026-05-15

## Nota

Esta feature es exclusivamente una **reorganización estructural de archivos y carpetas**. No introduce entidades de dominio nuevas ni modifica el modelo de datos existente en Firebase o el backend.

No se crean, modifican ni eliminan colecciones de Firestore.  
No se agregan, modifican ni eliminan tablas, columnas ni índices en ninguna base de datos.  
No se modifican los DTOs ni contratos OpenAPI.

## Entidades afectadas (solo estructuralmente)

Las siguientes entidades de dominio ya existen y no cambian en definición. Solo cambia la ubicación de sus interfaces TypeScript:

| Entidad | Módulo | Interfaz canónica (destino) |
|---------|--------|----------------------------|
| `WorkoutSummary`, `CreateWorkoutRequest`, `WorkoutProgress` | workouts | `interfaces/workouts.ts` |
| `MuscleGroup`, `Exercise`, `Food`, `CatalogAvailability` | catalog | `interfaces/catalog.ts` |
| `Diet` (tipos existentes) | diets | `interfaces/diets.ts` |
| `MealLog` (tipos existentes) | meals | `interfaces/meals.ts` |
| `Routine` (tipos existentes) | routines | `interfaces/routines.ts` |
| `UserPreferences` (tipos existentes) | settings | `interfaces/settings.ts` |
| `AdminEntityBase`, `MuscleGroupDto`, `MuscleDto`, `ExerciseTypeDto`, `ExerciseFormTypeDto` | admin | `interfaces/admin.ts` *(nuevo archivo, fusiona desde `features/admin/api/adminApi.ts`)* |

## Nota sobre `interfaces/admin.ts`

Los tipos del módulo `admin` actualmente están co-ubicados en `features/admin/api/adminApi.ts`. Al migrar el API service a `services/api/admin/adminApi.ts`, los tipos de dominio deben separarse a `interfaces/admin.ts` para cumplir la constitución.
