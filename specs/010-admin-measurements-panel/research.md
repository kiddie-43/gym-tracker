# Research: Panel de Administracion de Mediciones

**Feature**: 010-admin-measurements-panel  
**Date**: 2026-05-17  
**Status**: Complete

## Decision 1: Modelo de datos de mediciones en Firestore

- Decision: Usar `measurementTypes/{measurementTypeId}` con campos canonicos `key`, `name`, `unit`, `dataType`, `category`, `description`, `isActive`, `createdAt`, `updatedAt`, `deletedAt`.
- Rationale: Alinea la spec (FR-007, FR-020, modelo de coleccion esperado) y facilita uso directo en formularios de administracion/asignacion.
- Alternatives considered:
  - Modelo previo basado en `fields[]` dinamicos: descartado por no cumplir requerimientos actuales de columnas CSV ni categorias/tipos cerrados.
  - Uso de solo `deletedAt` sin `isActive`: descartado para esta feature por claridad en consultas UI y trazabilidad de estado.

## Decision 2: Regla de unicidad y activacion

- Decision: `key` es unico a nivel de catalogo activo; altas y reactivaciones validan conflicto.
- Rationale: Cumple FR-011 y evita duplicidad funcional en asignaciones de ejercicios.
- Alternatives considered:
  - Permitir duplicados con distinto estado: descartado por ambiguedad en selector de asignacion.
  - Unicidad compuesta (`key` + `category`): descartado por no estar en la spec.

## Decision 3: Importacion CSV parcial con reporte por fila

- Decision: Exponer endpoint de importacion que procese fila a fila, persista validas y reporte rechazadas con motivo.
- Rationale: Cumple FR-013 y FR-014, habilitando cargas masivas sin abortar lote completo.
- Alternatives considered:
  - Importacion atomica todo-o-nada: descartada por baja usabilidad para catalogos administrativos.
  - Parseo completo en frontend + llamadas individuales: descartado por falta de trazabilidad de lote y costo de red.

## Decision 4: Paridad UX con modulo de musculos

- Decision: Reutilizar patron UX del panel de musculos: acciones superiores (`Anadir`, `Importar CSV`, `Refrescar`), filtros, alternancia activos/todos, accion `Reactivar`, popup unificado crear/editar.
- Rationale: Cumple FR-001 y FR-018 con menor curva de aprendizaje para admins.
- Alternatives considered:
  - Flujo en pagina separada para create/edit: descartado por FR-019.
  - Wizard de varios pasos: descartado por complejidad innecesaria para CRUD de catalogo.

## Decision 5: Asignacion a ejercicios consumiendo solo activos

- Decision: Proveer `GET /api/admin/measurement-types/assignable` (solo activos) y usarlo en formularios de ejercicios para nuevas asignaciones.
- Rationale: Cumple FR-015, FR-016 y FR-022 sin romper referencias historicas existentes.
- Alternatives considered:
  - Reutilizar listado general con filtro en cliente: descartado por riesgo de errores y acoplamiento UI-reglas.
  - Borrar historicos al desactivar: descartado por perdida de trazabilidad.

## Decision 6: Enumeraciones cerradas y validacion consistente

- Decision: Validar en backend y frontend que `dataType` pertenezca a `{integer, decimal, time, boolean, text}` y `category` a `{strength, cardio, mobility, general}` para alta manual y CSV.
- Rationale: Cumple FR-008, FR-009 y FR-021 con comportamiento consistente entre canales.
- Alternatives considered:
  - Validar solo en frontend: descartado por falta de seguridad de datos.
  - Permitir valores libres con advertencias: descartado por romper estandarizacion del catalogo.

## Open Points Resueltos

- NEEDS CLARIFICATION sobre modelo de coleccion: resuelto con path `measurementTypes/{measurementTypeId}`.
- NEEDS CLARIFICATION sobre estrategia de asignacion: resuelto con endpoint asignable y exclusion de desactivados.
- NEEDS CLARIFICATION sobre importacion: resuelto con procesamiento parcial y resultado por fila.

## Validation Log (2026-05-18)

- Backend unit validadas:
  - `MeasurementTypeValidationTests`
  - `MeasurementTypeCsvImportTests`
- Backend integration validadas:
  - `MeasurementTypesAuthorizationTests` (401/403/200)
  - `MeasurementTypesCrudTests`
  - `MeasurementTypesImportCsvTests`
  - `MeasurementTypesImportCsvVolumeTests` (200 filas, >=95% procesadas)
  - `MeasurementTypesAssignableTests`
  - `MeasurementTypesHistoricalTraceabilityTests`
- Frontend tests validadas:
  - `MeasurementTypesPanel.popup.test.tsx`
  - `MeasurementTypesPanel.states.test.tsx`
  - `MeasurementTypesCsvImportDialog.test.tsx`
  - `ExercisesPanel.measurementAssignable.test.tsx`

Conclusiones QA:
- Se confirma consistencia de estados: desactivado (UI) == soft delete (tecnico) == `includeInactive` (API).
- Se confirma que nuevas asignaciones consumen solo catalogo assignable activo.
- Se confirma procesamiento parcial CSV con detalle por fila y control de conflictos de `key`.