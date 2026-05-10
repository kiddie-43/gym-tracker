# Data Model: App de Seguimiento Fitness y Progreso

## Overview

El modelo separa claramente datos transaccionales del usuario en Firestore y catálogo externo de Wger cacheado por el backend. Los documentos personales siempre se particionan por `userId`.

## Firestore Collections

```text
users/{userId}
users/{userId}/preferences/profile
users/{userId}/routines/{routineId}
users/{userId}/diets/{dietId}
users/{userId}/workouts/{workoutId}
users/{userId}/mealLogs/{mealLogId}
users/{userId}/progressSnapshots/{snapshotId}
users/{userId}/dashboardSummaries/{summaryId}

catalogCache/exercises/{exerciseId}
catalogCache/foods/{foodId}
catalogCache/muscleGroups/{muscleGroupId}
```

## Entities

### UserPreferences
- **Purpose**: Configuración funcional del usuario.
- **Fields**:
  - `userId`
  - `calorieTrackingEnabled` (boolean)
  - `dailyCalorieGoal` (number | null)
  - `units` (metric/imperial)
  - `updatedAt`
- **Rules**:
  - `dailyCalorieGoal` solo es válido cuando `calorieTrackingEnabled=true`.
  - Si el seguimiento está desactivado, la UI no debe exigir calorías en ningún flujo.

### Workout
- **Purpose**: Sesión completa de entrenamiento.
- **Fields**:
  - `workoutId`
  - `userId`
  - `performedAt`
  - `status` (`completed` | `incomplete`)
  - `routineId` (nullable)
  - `notes`
  - `exerciseEntries[]`
  - `createdAt`
  - `updatedAt`
- **Derived fields**:
  - `totalVolume`
  - `totalLoad`
  - `totalRepetitions`
- **Rules**:
  - Un `Workout` incompleto no invalida el histórico, pero su peso en comparativas debe controlarse según reglas de dominio.

### ExerciseEntry
- **Purpose**: Ejercicio individual realizado dentro de un entrenamiento.
- **Fields**:
  - `exerciseEntryId`
  - `externalExerciseId`
  - `exerciseNameSnapshot`
  - `muscleGroupIds[]`
  - `sets[]`
  - `notes`
  - `imageUrl` (nullable)
- **Set fields**:
  - `repetitions`
  - `weight`
  - `restSeconds`
  - `completed`
- **Derived fields**:
  - `entryVolume`
  - `entryLoad`
  - `entryRepetitions`

### ProgressSnapshot
- **Purpose**: Resultado persistido de comparación de progreso por ejercicio y fecha.
- **Fields**:
  - `snapshotId`
  - `userId`
  - `exerciseId`
  - `workoutId`
  - `comparisonBasis`
  - `lastSessionComparison`
  - `rollingAverageComparison`
  - `bestRecentComparison`
  - `trend` (`improving` | `stable` | `declining` | `no-reference`)
  - `createdAt`
- **Rules**:
  - Debe guardar suficiente detalle para auditar el cálculo sin recalcular todo el histórico.

### Routine
- **Purpose**: Plantilla reutilizable de entrenamiento.
- **Fields**:
  - `routineId`
  - `userId`
  - `name`
  - `dayLabel`
  - `primaryFocus`
  - `muscleGroupIds[]`
  - `plannedExercises[]`
  - `createdAt`
  - `updatedAt`
  - `archived`
- **PlannedExercise fields**:
  - `externalExerciseId`
  - `exerciseNameSnapshot`
  - `targetSets`
  - `targetRepetitions`
  - `targetWeight` (nullable)
  - `targetRestSeconds`
  - `notes`

### Diet
- **Purpose**: Plan de comidas por días.
- **Fields**:
  - `dietId`
  - `userId`
  - `name`
  - `days[]`
  - `createdAt`
  - `updatedAt`
  - `archived`
- **DietDay fields**:
  - `dayKey` (monday, tuesday, etc. o etiqueta equivalente)
  - `mealSlots[]`
- **MealSlot fields**:
  - `slotType` (`breakfast` | `lunch` | `snack` | `dinner`)
  - `items[]`

### MealLog
- **Purpose**: Registro real de comidas consumidas.
- **Fields**:
  - `mealLogId`
  - `userId`
  - `loggedDate`
  - `slotType`
  - `items[]`
  - `calorieStatus` (`disabled` | `known` | `partial` | `unknown`)
  - `knownCalories` (nullable)
  - `createdAt`
  - `updatedAt`
- **MealItem fields**:
  - `externalFoodId`
  - `foodNameSnapshot`
  - `quantity`
  - `unit`
  - `calories` (nullable)
  - `protein` (nullable)
  - `carbs` (nullable)
  - `fat` (nullable)
- **Rules**:
  - No se deben inferir calorías cuando falte un valor explícito confiable.

### CatalogCacheItem
- **Purpose**: Réplica parcial cacheada del catálogo Wger.
- **Fields**:
  - `externalId`
  - `type` (`exercise` | `food` | `muscle-group`)
  - `name`
  - `metadata`
  - `imageUrl` (nullable)
  - `fetchedAt`
  - `expiresAt`
  - `stale`

## Relationships

- Un usuario tiene muchas rutinas, dietas, sesiones, comidas y snapshots de progreso.
- Un entrenamiento puede originarse desde una rutina, pero conserva snapshots de nombre e IDs externos para estabilidad histórica.
- Una dieta define planificación; los `MealLog` representan ejecución real y no deben sobrescribir la plantilla.
- Los elementos de catálogo cacheado no son fuente de verdad del historial del usuario, solo referencia externa.

## Query Patterns

- Historial de entrenamientos por `userId + performedAt desc`.
- Comparativas por `userId + exerciseId + performedAt desc`.
- Rutinas por `userId + archived=false + updatedAt desc`.
- Dietas por `userId + archived=false + updatedAt desc`.
- Comidas por `userId + loggedDate`.
- Catálogo por `type + muscleGroupIds` o por búsqueda textual.
