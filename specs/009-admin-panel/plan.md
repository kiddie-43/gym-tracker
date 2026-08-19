# Implementation Plan: Panel de Administracion - Catalogos (Musculos, Mediciones, Ejercicios)

**Branch**: `009-admin-panel` | **Date**: 2026-05-17 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/009-admin-panel/spec.md`

## Summary

Implementar tres paneles CRUD administrativos (Musculos, Measurement Types, Ejercicios) con soft delete, restauracion controlada por filtro de borrados, y importacion CSV parcial para musculos. La solucion cubre frontend (tabs/paginas, estado, UI), backend (.NET endpoints CRUD + reactivate + import), contratos OpenAPI y pruebas backend/frontend.

## Endpoint Migration Plan

- Reemplazar endpoints admin legacy no alineados con la spec 009 por el set canonico de catalogos:
    - `/api/admin/muscles`
    - `/api/admin/measurement-types`
    - `/api/admin/exercises`
- Eliminar del contrato OpenAPI los endpoints legacy fuera de alcance de esta feature para evitar deriva de contrato.
- Mantener relaciones obligatorias en backend:
    - Ejercicio -> PrimaryMuscles/SecondaryMuscles (FK a musculos no borrados)
    - Ejercicio -> MeasurementType (FK a medicion no borrada)
- Asegurar endpoints de ciclo completo por catalogo: list, get by id, create, update, soft delete, reactivate.
- Añadir endpoint de importacion masiva CSV en musculos con procesamiento parcial y reporte por fila.

## Technical Context

**Language/Version**: Backend C# .NET 8, Frontend TypeScript 5.8 + React 19  
**Primary Dependencies**: ASP.NET Core, Firebase Auth/Admin, Firestore, React, Material UI v7, Redux Toolkit, react-i18next  
**Storage**: Firebase Firestore para catalogos administrables y referencias  
**Testing**: xUnit (backend), integration tests API, Vitest + Testing Library (frontend)  
**Target Platform**: Web SPA + API REST self-hosted on ASP.NET Core  
**Project Type**: Web application monorepo (frontend + backend)  
**Performance Goals**: Importar >=100 filas CSV en una operacion con respuesta en <3s en entorno local de desarrollo  
**Constraints**: Soft delete como unica fuente de estado, unicidad por `code` en activos, TypeScript estricto sin `any`, OpenAPI obligatorio  
**Scale/Scope**: 3 modulos CRUD + 1 importador CSV + restauracion en tabla con filtro de borrados

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: PASS. Se define `contracts/openapi.yaml` para endpoints CRUD, reactivacion e importacion CSV.
- Security-first: PASS WITH DEFERRED AUTHZ DETAIL. Token Firebase obligatorio en endpoints admin; autorizacion por rol se difiere por decision de spec y queda como deuda explicitada.
- Data boundary: PASS. Datos de catalogo administrables permanecen en Firebase; no se acopla a Wger en esta feature.
- Reliability: PASS (N/A for Wger dependency). No hay consumo Wger en esta funcionalidad.
- Quality gates: PASS. Matriz de pruebas backend/frontend definida en `quickstart.md`.
- Operations: PASS. Se planifica mantener logging estructurado y codigos de error de conflicto/validacion en endpoints nuevos.

## Project Structure

### Documentation (this feature)

```text
specs/009-admin-panel/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── backend/
│   ├── src/
│   │   ├── GymTracker.Api/
│   │   │   ├── Controllers/
│   │   │   └── Contracts/
│   │   ├── GymTracker.Application/
│   │   ├── GymTracker.Domain/
│   │   └── GymTracker.Infrastructure/
│   └── tests/
│       ├── GymTracker.Api.IntegrationTests/
│       └── GymTracker.Application.UnitTests/
└── frontend/
    ├── src/
    │   ├── pages/
    │   ├── components/
    │   ├── redux/
    │   └── services/
    └── tests/
```

**Structure Decision**: Web application (frontend + backend) con cambios en capas API/Application/Domain/Infrastructure para backend, y pages/redux/services/components en frontend.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Detalle de autorizacion por rol diferido | Decision explicita del alcance de feature | Implementar RBAC ahora rompe alcance acordado de clarificacion |

## Phase 0: Research

Resultados consolidados en [research.md](research.md):

- Soft delete como unico modelo de activacion.
- Restauracion manual via filtro + accion de tabla.
- Bloqueo por conflicto de unicidad en reactivacion.
- Importacion CSV parcial con reporte por fila.
- Endpoints dedicados para reactivar e importar.

## Phase 1: Design

### Data model

- Entidades y relaciones definidas en [data-model.md](data-model.md).
- Reglas de validacion y transiciones de estado incluidas.

### API contracts

- Contrato OpenAPI definido en [contracts/openapi.yaml](contracts/openapi.yaml).
- Incluye CRUD, soft delete, reactivacion e importacion CSV.

### Quickstart

- Flujo de validacion manual y matriz minima de pruebas en [quickstart.md](quickstart.md).

## Constitution Check (post-design)

- OpenAPI-first: PASS (contrato completo generado).
- Security-first: PASS WITH DEFERRED AUTHZ DETAIL (sin bloqueo para esta phase).
- Data boundary: PASS.
- Reliability: PASS.
- Quality gates: PASS.
- Operations: PASS.
