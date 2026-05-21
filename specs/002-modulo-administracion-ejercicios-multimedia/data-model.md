# Data Model: Modulo de Administracion de Ejercicios con Multimedia

## Overview

La feature introduce un catalogo maestro propio administrado desde backend .NET, persistido en Firestore con el patron actual de claves logicas, y binarios multimedia en Firebase Storage. El flujo de workouts pasa a consumir solo ejercicios admin activos para nuevas selecciones, pero conserva snapshots minimos para proteger el historico.

## Firestore Logical Keys

```text
admin/muscle-groups/{muscleGroupId}
admin/muscles/{muscleId}
admin/exercise-types/{exerciseTypeId}
admin/exercise-form-types/{formTypeId}
admin/exercises/{exerciseId}
admin-index/muscle-groups/code/{normalizedCode}
admin-index/muscles/code/{normalizedCode}
admin-index/exercise-types/code/{normalizedCode}
admin-index/exercise-form-types/code/{normalizedCode}
admin-index/exercises/code/{normalizedCode}
users/{userId}/workouts/{workoutId}
users/{userId}/routines/{routineId}
```

## Storage Paths

```text
admin/exercises/{exerciseId}/{mediaId}/{safeFileName}
```

## Entities

### MuscleGroup
- **Purpose**: Zona muscular configurable por administracion.
- **Fields**:
  - `id`
  - `name`
  - `code`
  - `description`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
- **Rules**:
  - `code` es unico a nivel de entidad.
  - Un grupo borrado logicamente no puede aparecer en selectores operativos.

### Muscle
- **Purpose**: Musculo reusable en definicion de ejercicios.
- **Fields**:
  - `id`
  - `name`
  - `code`
  - `description`
  - `muscleGroupIds[]`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
- **Rules**:
  - Debe pertenecer al menos a un `MuscleGroup` activo al momento de creacion o actualizacion.
  - Puede pertenecer a multiples grupos musculares.

### ExerciseType
- **Purpose**: Define el tipo de actividad y su unidad principal.
- **Fields**:
  - `id`
  - `name`
  - `code`
  - `description`
  - `primaryUnit`
  - `requiresUnits`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
- **Rules**:
  - Si `requiresUnits=true`, `primaryUnit` es obligatorio.

### ExerciseFormType
- **Purpose**: Define la captura dinamica usada por workouts.
- **Fields**:
  - `id`
  - `name`
  - `code`
  - `description`
  - `fields[]`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
- **Field Definition**:
  - `id`
  - `name`
  - `label`
  - `type` (`number` | `decimal` | `text` | `select` | `duration`)
  - `required`
  - `unit` (nullable)
  - `min` (nullable)
  - `max` (nullable)
  - `options[]` (nullable)
  - `sortOrder`
- **Rules**:
  - `min <= max` cuando ambos existan.
  - `options[]` es obligatoria y no vacia para `type=select`.

### Exercise
- **Purpose**: Agregado maestro consumido por panel admin y selector de workouts.
- **Fields**:
  - `id`
  - `code`
  - `name`
  - `description`
  - `exerciseTypeId`
  - `exerciseTypeCode`
  - `formTypeId`
  - `formTypeCode`
  - `primaryMuscleIds[]`
  - `secondaryMuscleIds[]`
  - `muscleGroupIds[]`
  - `difficulty`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
  - `media[]`
- **Derived fields**:
  - `primaryMediaId` (nullable)
  - `coverStoragePath` (nullable)
  - `coverThumbnailPath` (nullable)
- **Rules**:
  - `exerciseTypeId` y `formTypeId` son obligatorios.
  - `primaryMuscleIds[]` y `secondaryMuscleIds[]` deben referenciar musculos activos no borrados.
  - `muscleGroupIds[]` debe cubrir la union de grupos asociados a los musculos seleccionados.
  - Un ejercicio inactivo o borrado logicamente no debe aparecer en el selector de workouts.

### ExerciseMedia
- **Purpose**: Metadato multimedia asociado a un `Exercise`.
- **Fields**:
  - `mediaId`
  - `mediaType` (`image` | `video`)
  - `title`
  - `storagePath`
  - `thumbnailPath` (nullable)
  - `contentType`
  - `fileName`
  - `sizeBytes`
  - `sortOrder`
  - `isPrimary`
  - `active`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `deletedAt` (nullable)
- **Rules**:
  - Maximo 8 items por ejercicio.
  - Maximo 6 imagenes y 2 videos.
  - Imagenes <= 5 MB; videos <= 100 MB.
  - Solo una media puede tener `isPrimary=true`.
  - `sortOrder` debe ser unico dentro del ejercicio activo.

### ExerciseUploadTicket
- **Purpose**: Contrato efimero para subir binarios de forma segura.
- **Fields**:
  - `exerciseId`
  - `mediaId`
  - `storagePath`
  - `uploadUrl`
  - `expiresAt`
  - `contentType`
  - `maxSizeBytes`
- **Rules**:
  - No se persiste como documento duradero; se usa como respuesta transitoria de API.

### WorkoutExerciseSnapshot
- **Purpose**: Snapshot minimo para historico y progreso.
- **Fields**:
  - `exerciseId`
  - `name`
  - `coverStoragePath` (nullable)
  - `coverMediaId` (nullable)
  - `formTypeId`
  - `formTypeCode`
  - `capturedAt`
- **Rules**:
  - Debe guardarse en cada nuevo workout y rutina planificada.
  - El historico debe renderizarse aunque el ejercicio luego quede inactivo o borrado logicamente.

## Relationships

- Un `Muscle` pertenece a uno o multiples `MuscleGroup`.
- Un `Exercise` referencia un `ExerciseType`, un `ExerciseFormType`, multiples `Muscle` y multiples `ExerciseMedia`.
- `Exercise.media[]` se mantiene como agregado embebido para garantizar invariantes de portada, orden y limites.
- `WorkoutExerciseSnapshot` referencia el ejercicio por `exerciseId`, pero conserva su propio snapshot operativo.

## Query Patterns

- Listar panel admin por entidad con `active`, `isDeleted`, `query`, `page` y `pageSize`.
- Resolver unicidad por `normalizedCode` via `admin-index/...`.
- Obtener selector de workouts por `active=true`, `isDeleted=false` y texto de busqueda.
- Obtener galeria de media de un ejercicio cargando un solo agregado `Exercise`.
- Reordenar media actualizando todo el array `media[]` en una sola operacion logica.

## Consistency Rules

- Si falla la persistencia del metadato despues de subir un archivo, el backend debe compensar eliminando el objeto en Storage.
- Si se marca una media como principal, todas las demas del ejercicio deben quedar con `isPrimary=false`.
- El borrado logico de entidades maestras no elimina workouts ni rutinas historicas; solo las excluye de nuevas selecciones.
