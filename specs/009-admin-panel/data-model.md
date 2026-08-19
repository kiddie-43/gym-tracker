# Data Model: Admin Catalogos (Musculos, Mediciones, Ejercicios)

## Overview

El modelo define tres catalogos administrables con soft delete y restauracion. Ejercicios depende de Musculos y MeasurementType.

## Entities

### Muscle

- Purpose: Catalogo de musculos disponibles para clasificar ejercicios.
- Fields:
  - `id` (string, requerido, unico)
  - `code` (string, requerido, unico entre registros activos)
  - `description` (string, opcional)
  - `deletedAt` (datetime, nullable)
  - `deleted` (boolean derivado, opcional segun persistencia)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - `code` requerido, trim, mayusculas recomendadas.
  - unicidad de `code` en registros no borrados.
- State transitions:
  - Active -> SoftDeleted (delete)
  - SoftDeleted -> Active (reactivar, si no hay conflicto de unicidad)

### MeasurementType

- Purpose: Define que campos dinamicos se capturan para un ejercicio/sesion.
- Fields:
  - `id` (string, requerido, unico)
  - `name` (string, requerido, unico entre activos)
  - `fields` (array requerido, minimo 1)
  - `deletedAt` (datetime, nullable)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - `name` requerido.
  - al menos un `MeasurementField`.

### MeasurementField

- Purpose: Descriptor de campo dinamico de captura.
- Fields:
  - `name` (string, requerido)
- Validation:
  - `name` no vacio.
  - nombres duplicados dentro de la misma medicion no permitidos.

### Exercise

- Purpose: Catalogo de ejercicios administrables.
- Fields:
  - `id` (string, requerido, unico)
  - `code` (string, requerido, unico entre activos)
  - `name` (string, requerido)
  - `description` (string, opcional)
  - `difficulty` (enum: Beginner, Intermediate, Advanced)
  - `category` (enum inicial: Strength, Cardio, Mobility, Stretching)
  - `primaryMuscleIds` (array requerido, minimo 1)
  - `secondaryMuscleIds` (array opcional)
  - `measurementTypeId` (string, requerido)
  - `images` (array opcional de URL/ID)
  - `videos` (array opcional de URL/ID)
  - `deletedAt` (datetime, nullable)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
- Validation:
  - `code` unico entre activos.
  - `primaryMuscleIds` y `measurementTypeId` deben referenciar registros no borrados al crear/editar.

## Relationships

- `Exercise.primaryMuscleIds[]` -> `Muscle.id` (N:1)
- `Exercise.secondaryMuscleIds[]` -> `Muscle.id` (N:1)
- `Exercise.measurementTypeId` -> `MeasurementType.id` (N:1)

## Import Batch (CSV)

### MuscleImportBatch

- Purpose: Resultado de una importacion masiva parcial.
- Fields:
  - `totalRows` (number)
  - `importedRows` (number)
  - `rejectedRows` (number)
  - `results[]` (array de `MuscleImportRowResult`)

### MuscleImportRowResult

- Fields:
  - `rowNumber` (number)
  - `code` (string)
  - `status` (enum: Imported, Rejected)
  - `reason` (string, opcional)

## Query Semantics

- Listado por defecto: solo no borrados.
- `includeDeleted=true`: incluye borrados para habilitar restauracion.
- Selectores de referencia (ejercicios): solo no borrados.

## Business Rules

- Reglas de unicidad aplican sobre activos.
- Reactivacion falla si entra en conflicto con unicidad activa.
- Soft delete conserva trazabilidad historica de referencias ya existentes.
- Importacion CSV es parcial: las filas validas no se revierten por fallos de otras filas.
