# Research: Panel de Administracion - Catalogos (Musculos, Mediciones, Ejercicios)

**Feature**: 009-admin-panel  
**Date**: 2026-05-17  
**Status**: Complete

## Decision 1: Modelo de activacion/desactivacion

- Decision: Usar exclusivamente soft delete para activar/desactivar en los 3 catalogos.
- Rationale: Evita doble fuente de verdad (`isActive` + borrado) y simplifica reglas de negocio en listados y selectores.
- Alternatives considered:
  - `isActive` separado de soft delete: descartado por complejidad y ambiguedad de estado.
  - borrado fisico: descartado porque rompe trazabilidad historica en ejercicios.

## Decision 2: Restauracion de registros

- Decision: Permitir restauracion manual mediante filtro de borrados y accion `reactivar` por fila.
- Rationale: Permite recuperar errores operativos sin recrear registros ni romper referencias historicas.
- Alternatives considered:
  - sin restauracion: descartado por costo operativo alto.
  - restauracion automatica al crear mismo `code`: descartado por efectos implicitos no transparentes.

## Decision 3: Conflictos de unicidad al reactivar

- Decision: Si existe otro registro activo con el mismo `code`, bloquear reactivacion y devolver conflicto.
- Rationale: Preserva regla de unicidad y evita sobrescrituras semanticas.
- Alternatives considered:
  - permitir duplicados activos: descartado por inconsistencias funcionales.
  - renombrar automaticamente `code`: descartado por romper intencionalidad de negocio.

## Decision 4: Importacion CSV de musculos

- Decision: Importacion parcial. Se importan filas validas y se rechazan invalidas con reporte por fila.
- Rationale: Mejora UX para cargas masivas y evita reprocesar archivos completos por pocos errores.
- Alternatives considered:
  - importacion atomica: descartada por baja eficiencia operativa.
  - modo configurable atomico/parcial: descartado para mantener alcance simple en esta iteracion.

## Decision 5: Contrato de importacion

- Decision: Endpoint dedicado `POST /api/admin/muscles/import-csv` con payload estructurado JSON (`rows`) para desacoplar del transporte y facilitar pruebas.
- Rationale: Mantiene contrato OpenAPI claro y testeable; evita complejidad inicial de `multipart/form-data` en esta feature de plan.
- Alternatives considered:
  - subir archivo binario CSV (`multipart/form-data`): util en produccion, pero agrega complejidad de parsing HTTP al contrato inicial.
  - parseo CSV solo frontend y altas individuales: descartado por baja eficiencia y falta de trazabilidad de lote.

## Decision 6: Seguridad para esta feature

- Decision: Requerir autenticacion Firebase Auth en endpoints admin; autorizacion por rol queda diferida explicitamente a feature posterior.
- Rationale: Respeta decision de alcance de la spec y mantiene minima barrera de seguridad.
- Alternatives considered:
  - abrir endpoints sin auth: descartado por incumplir constitucion.
  - implementar RBAC completo ahora: descartado por decision explicita de alcance diferido.

## Decision 7: Seleccion de datos en UI admin

- Decision: Vista por defecto muestra no borrados; check "Ver borrados" habilita vista extendida con accion de reactivar.
- Rationale: Mantiene flujo cotidiano simple y expone recuperacion solo cuando se necesita.
- Alternatives considered:
  - mostrar todo siempre: descartado por ruido visual y mayor riesgo operativo.

## Decision 8: Limites y rendimiento inicial

- Decision: Definir objetivo de importacion minima de 100 filas por operacion con reporte de resultados.
- Rationale: Cubre necesidad operativa inmediata y alinea con SC-007.
- Alternatives considered:
  - sin objetivo cuantitativo: descartado por falta de criterio verificable.

## Execution Log: Backend Test Outputs (2026-05-17)

- Command: `dotnet test src/backend/tests/GymTracker.Application.UnitTests/GymTracker.Application.UnitTests.csproj`
  - Result: PASS
  - Summary: total 29, passed 29, failed 0, skipped 0

- Command: `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj`
  - Result: FAIL (legacy/other module baseline outside 009 scope)
  - Summary: total 30, failed 15
  - Representative failures observed:
    - `Admin.MediaCompensationTests.ConfirmMedia_ShouldReturnValidationProblem_WhenExerciseDoesNotExist` (expected 400, got 404)
    - `Catalog.CatalogFallbackEndpointsTests.GetStatus_ShouldReturnAvailabilityPayload` (expected 200, got 404)
    - `Admin.ExerciseTypesEndpointsTests.Crud_ShouldCreateUpdateListAndDeleteExerciseType` (expected 201, got 404)
    - `Admin.ExerciseFormTypesEndpointsTests.Crud_ShouldCreateUpdateListAndDeleteExerciseFormType` (expected 201, got 404)

- Command: `dotnet test src/backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "ExercisesCanonicalEndpointsTests|ExercisesRelationsValidationTests|ExercisesEndpointsTests|LegacyAdminRoutesCompatibilityTests"`
  - Result: PASS
  - Summary: total 6, passed 6, failed 0, skipped 0
