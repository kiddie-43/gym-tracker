# Data Model: Pagina de Rutinas de Entrenamiento

## Overview

El modelo cubre planificacion de rutinas y ejecucion real de entrenamiento por usuario, incluyendo flujo guiado y recuperacion de estado.

## Entities

### Routine

- Purpose: Representa la rutina creada por el usuario.
- Suggested Storage Path: `users/{userId}/routines/{routineId}`
- Fields:
  - `id` (string, requerido)
  - `userId` (string, requerido)
  - `title` (string, requerido)
  - `goal` (string, opcional)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
  - `isDeleted` (boolean, requerido, default `false`)
  - `deletedAt` (datetime, nullable)
- Validation:
  - `title` obligatorio y no vacio.
- State transitions:
  - `active -> archived` (soft delete)
  - `archived -> active` (reactivate)

### RoutineSession

- Purpose: Sesion de trabajo dentro de una rutina.
- Suggested Storage: subcoleccion o estructura embebida en rutina.
- Fields:
  - `id` (string, requerido)
  - `routineId` (string, requerido)
  - `name` (string, requerido)
  - `daysOfWeek` (array requerido, valores `monday..sunday`, min 1)
  - `isDeleted` (boolean, requerido)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - No repetir dia dentro de `daysOfWeek`.
  - No permitir dos sesiones activas con el mismo dia en la misma rutina.

### SessionExerciseLink

- Purpose: Vinculo logico de un ejercicio del catalogo dentro de una sesion.
- Fields:
  - `id` (string, requerido)
  - `routineId` (string, requerido)
  - `sessionId` (string, requerido)
  - `exerciseId` (string, requerido, catalogo)
  - `order` (number, opcional)
  - `isDeleted` (boolean, requerido)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - `exerciseId` debe existir en catalogo permitido.
- State transitions:
  - `linked -> unlinked` (soft unlink, sin borrar ejercicio base)

### PlannedSet

- Purpose: Serie planificada del ejercicio dentro de sesion.
- Fields:
  - `id` (string, requerido)
  - `sessionExerciseId` (string, requerido)
  - `repetitions` (number, requerido, >0)
  - `weightKg` (number, requerido, >0)
  - `order` (number, requerido)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)

### ExerciseTrainingLog

- Purpose: Registro real de entrenamiento del usuario sobre un ejercicio.
- Suggested Storage Path: `users/{userId}/exerciseTrainingLogs/{logId}`
- Fields:
  - `id` (string, requerido)
  - `userId` (string, requerido)
  - `routineId` (string, requerido)
  - `sessionId` (string, requerido)
  - `exerciseId` (string, requerido)
  - `performedSets` (array requerido de `PerformedSet`)
  - `notes` (string, opcional)
  - `attachments` (array de `TrainingAttachment`, max 5)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - Cada `PerformedSet` debe tener `repetitions > 0` y `weightKg > 0`.
  - Maximo 5 adjuntos por log.

### PerformedSet

- Fields:
  - `repetitions` (number, requerido, >0)
  - `weightKg` (number, requerido, >0)
  - `order` (number, requerido)

### TrainingAttachment

- Fields:
  - `id` (string, requerido)
  - `type` (enum: `photo|video`, requerido)
  - `url` (string, requerido)
  - `uploadedAt` (datetime, requerido)

### TrainingFlowState

- Purpose: Estado del stepper para recuperacion tras F5/reingreso.
- Suggested Storage Path: `users/{userId}/activeTrainingState/current`
- Fields:
  - `userId` (string, requerido)
  - `isLocked` (boolean, requerido)
  - `routineId` (string, requerido)
  - `sessionId` (string, nullable)
  - `exerciseId` (string, nullable)
  - `stepNode` (enum: `routine|session|exercise|exerciseData`, requerido)
  - `lastUpdatedAt` (datetime, requerido)
- Validation:
  - Si `isLocked=true`, `routineId` debe estar informado.

## Relationships

- `Routine (1) -> (N) RoutineSession`
- `RoutineSession (1) -> (N) SessionExerciseLink`
- `SessionExerciseLink (1) -> (N) PlannedSet`
- `User (1) -> (N) ExerciseTrainingLog`
- `ExerciseTrainingLog (N) -> (1) SessionExerciseLink` (referencia logica por IDs)
- `User (1) -> (1) TrainingFlowState`

## Business Rules

- BR-001: Todo borrado de rutina/sesion/vinculo debe ser logico.
- BR-002: Solo ejercicios de catalogo existente pueden vincularse a sesiones.
- BR-003: Una rutina no puede tener dos sesiones activas compartiendo el mismo dia.
- BR-004: Notas y adjuntos de log son privados por usuario; no se exponen a otros usuarios.
- BR-005: Con `isLocked=true`, la navegacion se restringe al arbol de entrenamiento.
- BR-006: Al recargar/reingresar con `isLocked=true`, se restaura el ultimo `TrainingFlowState` persistido.
- BR-007: Last write wins aplica para ediciones concurrentes de rutina en esta fase.
