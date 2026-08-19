# Data Model: Logs Metricos Dinamicos de Entrenamiento

## Overview

Modelo transaccional centrado en captura de metricas por set (`idGrupo`) con edicion restringida de valor y borrado logico por grupo.

## Entities

### TrainingMetricLog

- Purpose: Registro de una metrica especifica dentro de un set agrupado de entrenamiento.
- Suggested Storage: tabla transaccional SQL/Firebase collection equivalente.
- Fields:
  - `idLog` (string, requerido)
  - `idUsuario` (string, requerido)
  - `idRutina` (string, requerido)
  - `idSesion` (string, requerido)
  - `idEntrenamiento` (string, requerido)
  - `idMetrica` (string, requerido)
  - `idGrupo` (string, requerido, unico global, inmutable)
  - `fechaOperativa` (date, requerido)
  - `createdAt` (datetime, requerido)
  - `updatedAt` (datetime, requerido)
  - `valorMetrica` (decimal/string tipado por metrica, requerido)
  - `isDeleted` (boolean, requerido, default `false`)
  - `deletedAt` (datetime, nullable)
  - `deletedBy` (string, nullable)
- Validation:
  - `idMetrica` debe estar habilitada para el ejercicio/contexto.
  - `idGrupo` no puede repetirse con otro grupo.
  - Solo `valorMetrica` puede cambiar en edicion.

### MetricDefinition

- Purpose: Definicion de metrica disponible por ejercicio para validacion y render dinamico en frontend.
- Fields:
  - `idMetrica` (string, requerido)
  - `nombre` (string, requerido)
  - `tipoDato` (enum: number|time|distance|percent|custom, requerido)
  - `unidad` (string, opcional)
  - `activo` (boolean, requerido)

### MetricLogChangeEvent

- Purpose: Trazabilidad de operaciones mutables sobre logs metricos.
- Fields:
  - `idEvento` (string, requerido)
  - `idUsuario` (string, requerido)
  - `idGrupo` (string, requerido)
  - `tipoOperacion` (enum: create|update_value|soft_delete_group, requerido)
  - `entityId` (string, requerido)
  - `beforeValue` (string, opcional)
  - `afterValue` (string, opcional)
  - `actionAt` (datetime, requerido)
  - `actionBy` (string, requerido)

### LogsDetailViewState (frontend)

- Purpose: Estado global para detalle de logs y popups.
- Fields:
  - `list` (TrainingMetricLog[])
  - `selectedGroupId` (string|null)
  - `selectedMetricId` (string|null)
  - `loading` (boolean)
  - `error` (string|null)
  - `popupMode` (enum: none|create|edit|delete)

## Relationships

- `TrainingMetricLog (N) -> (1) MetricDefinition` por `idMetrica`.
- `TrainingMetricLog (N) -> (1) GrupoLogico` por `idGrupo` (conceptual).
- `MetricLogChangeEvent (N) -> (1) TrainingMetricLog/Grupo` por `entityId` y `idGrupo`.

## Business Rules

- BR-001: `idGrupo` es generado en backend, es privado para cliente y no editable.
- BR-002: No se permiten grupos identicos dentro del mismo usuario+contexto+fecha operativa.
- BR-003: Edicion solo modifica `valorMetrica`; otros campos son inmutables.
- BR-004: Borrado logico solo por `idGrupo` y afecta todo el grupo.
- BR-005: Consulta diaria usa `fechaOperativa` actual y desempata por mayor `CreatedAt`.
- BR-006: Si no hay registros del dia, la respuesta es `[]`.
- BR-007: Registros `isDeleted=true` no aparecen en listados activos.
- BR-008: Todas las mutaciones generan `MetricLogChangeEvent` con actor y timestamp.

## State Transitions

- `active -> active` (update_value de una metrica en mismo grupo)
- `active -> deleted` (soft_delete_group por `idGrupo`)
- `deleted -> deleted` (sin reactivacion en esta iteracion)
