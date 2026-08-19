# Data Model: Admin Measurements Panel

## Overview

El modelo define un catalogo de tipos de medicion administrable y su consumo en asignaciones de ejercicios. La persistencia primaria usa Firestore en la coleccion `measurementTypes`.

## Entities

### MeasurementType

- Purpose: Define una medicion reutilizable configurable para ejercicios.
- Firestore Path: `measurementTypes/{measurementTypeId}`
- Fields:
  - `id` (string, requerido, unico)
  - `key` (string, requerido, unico entre activos)
  - `name` (string, requerido)
  - `unit` (string, requerido)
  - `dataType` (enum, requerido: `integer|decimal|time|boolean|text`)
  - `category` (enum, requerido: `strength|cardio|mobility|general`)
  - `description` (string, opcional)
  - `isActive` (boolean, requerido, default `true`)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
  - `deletedAt` (datetime, nullable)
- Validation:
  - `key`, `name`, `unit`, `dataType`, `category` requeridos.
  - `key` normalizada (trim/lowercase recomendado para comparacion).
  - `key` unica entre registros activos.
  - `dataType` y `category` deben pertenecer a los conjuntos permitidos.
- State transitions:
  - `Active` -> `Inactive` (soft delete: `isActive=false`, `deletedAt` seteado)
  - `Inactive` -> `Active` (reactivate: `isActive=true`, `deletedAt=null`, valida unicidad de `key`)

### CsvImportResult

- Purpose: Resume la importacion CSV de tipos de medicion con resultado parcial.
- Fields:
  - `totalRows` (number, requerido)
  - `createdRows` (number, requerido)
  - `rejectedRows` (number, requerido)
  - `rows` (array requerido de `CsvImportRowResult`)

### CsvImportRowResult

- Purpose: Trazabilidad por fila procesada en importacion.
- Fields:
  - `rowNumber` (number, requerido)
  - `status` (enum requerido: `created|rejected`)
  - `key` (string, opcional)
  - `message` (string, requerido para `rejected`)

### ExerciseMeasurementAssignment

- Purpose: Representa mediciones configuradas para un ejercicio.
- Persistence: Referencia en documento de ejercicio (por ejemplo `exercises/{exerciseId}.measurementTypeKeys[]` o `measurementTypeIds[]`, segun modelo existente del modulo de ejercicios).
- Fields:
  - `exerciseId` (string, requerido)
  - `measurementTypeId` (string, requerido)
  - `measurementKey` (string, requerido para trazabilidad funcional)
  - `assignedAt` (datetime, requerido)
- Validation:
  - Solo `MeasurementType.isActive=true` puede asignarse en nuevas configuraciones.
  - Referencias historicas existentes a tipos desactivados se preservan para lectura/auditoria.

## Relationships

- `ExerciseMeasurementAssignment.measurementTypeId` -> `MeasurementType.id` (N:1)
- Un ejercicio puede tener multiples mediciones asignadas (1:N logico respecto a `ExerciseMeasurementAssignment`).

## Query Semantics

- Panel admin por defecto: lista solo `isActive=true`.
- Vista extendida (filtro reactivacion): incluye `isActive=false`.
- Selector de asignacion en ejercicios: consume solo endpoint/listado de activos.

## Business Rules

- BR-001: No se permite crear ni reactivar un tipo con `key` activa duplicada.
- BR-002: La importacion CSV es parcial; filas validas se crean aunque existan rechazadas.
- BR-003: Los errores de importacion deben reportarse por fila con mensaje accionable.
- BR-004: Desactivar una medicion no elimina referencias historicas ya vinculadas a ejercicios.
- BR-005: El popup de alta/edicion comparte reglas de validacion con importacion CSV.