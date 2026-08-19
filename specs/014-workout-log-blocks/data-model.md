# Data Model: Registrar Entrenamiento por Bloques (Plan Mensual)

## Overview

Modelo transaccional propiedad del usuario, analogo a `TrainingLog`: una `TrainingSession` por cada guardado de "Registrar entrenamiento" para un ejercicio planificado (`WeekNumber`/`DayNumber`/`ExerciseId`) concreto, con su resumen rapido embebido y su lista ordenada de `SessionBlock`. `BlockTemplate` es contenido estatico de referencia (no persistido).

## Entities

### TrainingSession (`GymTracker.Domain.Entities.TrainingSession`)

- Purpose: Sesion de entrenamiento guardada por el usuario para un ejercicio planificado, en un momento dado (fecha/hora real de guardado).
- Storage: tabla SQL `TrainingSessions`, hereda `AuditableEntity` (soft delete + auditoria de creacion/edicion).
- Fields:
  - `Id` (Guid, PK)
  - `UserId` (Guid, requerido) — propietario (FR-023)
  - `WeekNumber` (int, requerido, 1-4)
  - `DayNumber` (int, requerido, 1-7)
  - `ExerciseId` (Guid, requerido, FK -> `Exercices.Id`)
  - `Timestamp` (DateTimeOffset, requerido, UTC, generado por servidor) — distingue repeticiones del mismo ejercicio planificado en distintos meses (FR-015)
  - `DurationMinutes` (decimal, requerido, >= 0) — metrica 1 del resumen rapido, comun a ambos perfiles
  - `SecondaryMetricValue` (decimal, requerido, >= 0) — Volumen total (perfil fuerza) o Calorias aproximadas (perfil cardio)
  - `SecondaryMetricUnitCode` (string, requerido) — codigo de `Units` (p. ej. volumen en kg / calorias en kcal), sin FK (Decision 6 de research.md)
  - `TertiaryMetricValue` (decimal, requerido, >= 0) — num. de ejercicios/bloques (perfil fuerza, 0 decimales de facto) o Ritmo objetivo RPE 1-10 (perfil cardio)
  - `Notes` (string?, opcional, max 150 caracteres)
  - `Blocks` (coleccion de `SessionBlock`, ordenada por `OrderIndex`)
- Validation (en `Create`/`Update` de dominio):
  - `WeekNumber` en [1,4]; `DayNumber` en [1,7] (mismo patron que `TrainingLog`/`PlannedExercise`).
  - `ExerciseId` y `UserId` != `Guid.Empty`.
  - `DurationMinutes`, `SecondaryMetricValue`, `TertiaryMetricValue` >= 0 (FR-024).
  - `Notes` <= 150 caracteres si no es null (FR-012).
  - `SecondaryMetricUnitCode` requerido, no vacio.

### SessionBlock (`GymTracker.Domain.Entities.SessionBlock`)

- Purpose: Bloque individual de la estructura de entrenamiento de una `TrainingSession`.
- Storage: tabla SQL `SessionBlocks`, hereda `AuditableEntity`; FK `TrainingSessionId -> TrainingSessions.Id` (`OnDelete(DeleteBehavior.Cascade)` — un bloque no tiene sentido sin su sesion).
- Fields:
  - `Id` (Guid, PK)
  - `TrainingSessionId` (Guid, requerido, FK)
  - `BlockType` (`SessionBlockType` enum, requerido)
  - `Name` (string, requerido, no vacio) — FR-009
  - `DurationValue` (decimal, requerido, > 0) — FR-009
  - `DurationUnitCode` (string, requerido) — codigo de `Units` (tiempo, p. ej. minutos), sin FK (Decision 6)
  - `Description` (string?, opcional, max 100 caracteres) — FR-008
  - `IntensityRpe` (int?, opcional, 1-10) — solo aplica a tipos de bloque del perfil cardio que lo soportan (FR-008); se descarta (`null`) si el tipo de bloque cambia a uno que no lo soporta (edge case del spec)
  - `OrderIndex` (int, requerido, >= 0) — posicion dentro de la estructura del entrenamiento
- Validation:
  - `Name` no vacio; `DurationValue` > 0 (FR-009).
  - `BlockType` debe pertenecer al mismo perfil (fuerza/cardio) que el `ExerciseType` de la `TrainingSession` padre — validado en el servicio de aplicacion, no en el modelo de dominio aislado (necesita conocer el `ExerciseType` del ejercicio).
  - `IntensityRpe`, si informado, en [1,10]; MUST ser `null` si `BlockType` no soporta intensidad.
  - `Description`, si informado, <= 100 caracteres.

### SessionBlockType (enum, `GymTracker.Domain.Enum`)

Valores fijos, cerrados por el spec (FR-008):

| Valor | Perfil | Soporta `IntensityRpe` |
|---|---|---|
| `WARMUP` | fuerza | No |
| `APPROACH` | fuerza | No |
| `WORK` | fuerza | No |
| `REST_STRENGTH` | fuerza | No |
| `SWIM` | cardio | Si |
| `SERIES` | cardio | Si |
| `TECHNIQUE` | cardio | Si |
| `REST_CARDIO` | cardio | No |

> Nota: el spec no fija nombres tecnicos para "Descanso" en cada perfil; se distinguen `REST_STRENGTH`/`REST_CARDIO` para poder acotar el perfil valido por tipo sin ambiguedad. El label visible en UI (i18n) es "Descanso" en ambos casos.

### BlockTemplate (contenido estatico, no persistido)

- Purpose: Estructura de bloques predefinida por perfil, usada solo para precargar al pulsar "Ver plantillas" (FR-006). Vive en `GymTracker.Application.TrainingSession.BlockTemplateCatalog` como datos en memoria; expuesta por un endpoint de solo lectura.
- Shape (por item de plantilla, no es una entidad de EF):
  - `BlockType` (`SessionBlockType`)
  - `Name` (string) — nombre por defecto, localizable via i18n en frontend (backend devuelve el codigo, no el texto)
  - `DefaultDurationMinutes` (decimal)
  - `OrderIndex` (int)
- No tiene `Id` externo, no es editable, no se persiste (INT-004).

### ExerciseProfile (concepto, no entidad)

- Purpose: Deriva de `ExerciseType` el perfil de resumen/bloques aplicable. No es una tabla ni un enum persistido; es una funcion pura (`ExerciseTypeProfileMapper` en Application/backend y un util equivalente en frontend) que mapea:
  - Perfil `STRENGTH`: `STRENGTH`, `BODYWEIGHT`, `PLYOMETRIC`, `REHABILITATION`.
  - Perfil `CARDIO`: `CARDIO`, `MOBILITY`, `STRETCHING`, `SPORTS`.

### PlannedExercise, Exercice/ExerciseType/Units, TrainingLog (existentes, sin cambios)

- Se reutilizan tal cual. `TrainingSession.ExerciseId` referencia `Exercices.Id` (mismo catalogo que usa `PlannedExercise.ExerciseId`). `TrainingLog` no se modifica (FR-022).

## Relationships

- `TrainingSession (1) -> (N) SessionBlock` por `TrainingSessionId`, cascade delete.
- `TrainingSession (N) -> (1) Exercice` por `ExerciseId`, `OnDelete(Restrict)` (igual criterio que `PlannedExercise -> Exercice`).
- `TrainingSession` no tiene FK directa a `PlannedExercise`; se relaciona logicamente por la tripleta `(UserId, WeekNumber, DayNumber, ExerciseId)` (Decision 2 de research.md).
- `BlockTemplate (N) -> perfil (STRENGTH|CARDIO)`, indexado solo por perfil, sin persistencia.

## Business Rules

- BR-001: Una `TrainingSession` MUST pertenecer a un unico `UserId`; toda lectura/escritura MUST filtrar por el usuario autenticado (FR-023).
- BR-002: Pueden existir multiples `TrainingSession` para la misma tripleta `(UserId, WeekNumber, DayNumber, ExerciseId)`; cada guardado crea una fila nueva, nunca sobrescribe una existente (FR-015).
- BR-003: `Blocks` es opcional; una sesion valida puede guardarse con lista vacia (FR-014 edge case).
- BR-004: El perfil (fuerza/cardio) de una `TrainingSession` se deriva del `ExerciseType` de `Exercice` en el momento de guardar; determina que `SessionBlockType` son validos y el significado de `SecondaryMetricValue`/`TertiaryMetricValue`.
- BR-005: Si `IntensityRpe` esta informado y el `BlockType` no soporta intensidad, el valor MUST descartarse antes de persistir (edge case del spec).
- BR-006: Eliminacion logica estandar (`AuditableEntity`) aplica a `TrainingSession`/`SessionBlock`; las lecturas de historial MUST excluir registros con `DeletedAt != null`.
- BR-007: `BlockTemplate` MUST NOT persistirse ni exponerse como editable; es solo lectura derivada del perfil solicitado.

## State Transitions

- No hay maquina de estados: una `TrainingSession` se crea una vez (`active`) y no se edita despues de guardada (el historial es de solo lectura, US5). El unico transito soportado es `active -> deleted` (soft delete estandar), no solicitado explicitamente por ninguna FR pero heredado de `AuditableEntity` de forma consistente con el resto del proyecto.
