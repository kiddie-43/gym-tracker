# Implementation Plan: Panel de Administracion de Mediciones

**Branch**: `010-create-feature-branch` | **Date**: 2026-05-17 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/010-admin-measurements-panel/spec.md`

## Summary

Implementar un panel de administracion de mediciones configurable con paridad UX respecto al modulo de musculos: CRUD por popup, soft delete/reactivacion, importacion CSV parcial con reporte por fila y exposicion del catalogo activo para asignacion a ejercicios. La solucion cubre frontend (tabla + acciones + popup + importador), backend (endpoints CRUD/import/listado asignable), modelo Firestore en `measurementTypes/{measurementTypeId}`, contrato OpenAPI y pruebas para los tres escenarios P1/P2.

Terminologia oficial de estado para esta feature:
- UI: "desactivado"
- Persistencia: soft delete logico (`isDeleted=true`, `active=false`)
- API: `includeInactive=true` para incluir desactivados en listados administrativos

## Technical Context

**Language/Version**: Backend C# .NET 8, Frontend TypeScript 5.8 + React 19  
**Primary Dependencies**: ASP.NET Core, Firebase Auth/Admin SDK, Firestore, React, Material UI v7, Redux Toolkit, react-i18next  
**Storage**: Firebase Firestore (`measurementTypes`, `exercises`) con soft delete logico  
**Testing**: xUnit (unit + integration) en backend, Vitest + Testing Library en frontend  
**Target Platform**: Web SPA + API REST ASP.NET Core  
**Project Type**: Monorepo web app (frontend + backend)  
**Performance Goals**: Importar 200 filas CSV con resultado parcial en <= 3 segundos en entorno local; operaciones CRUD p95 <= 250ms  
**Constraints**: OpenAPI obligatorio, Firebase Auth obligatorio en endpoints admin, TypeScript estricto, estados de carga/error/vacio obligatorios, no usar tipos de medicion desactivados para nuevas asignaciones  
**Scale/Scope**: 1 modulo admin (mediciones) + integracion de asignacion en ejercicios + importador CSV con validacion por fila

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: PASS. Se planifica contrato en `contracts/openapi.yaml` para CRUD, import CSV, reactivate y listado asignable.
- Security-first: PASS. Endpoints admin protegidos por Firebase Auth; autorizacion de administracion aplicada en categoria de endpoints `/api/admin/*`.
- Data boundary: PASS con excepcion acotada. Esta feature administra un catalogo interno (`measurementTypes`) y no consume catalogo externo Wger por alcance funcional.
- Reliability: PASS (N/A Wger). No integra Wger; no requiere cache/fallback externo en esta feature.
- Quality gates: PASS. Cobertura de pruebas backend/frontend definida y trazada a historias P1/P2.
- Operations: PASS. Se mantienen codigos de error estandar, logging estructurado y objetivos p95 para CRUD/import.

## Project Structure

### Documentation (this feature)

```text
specs/010-admin-measurements-panel/
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
src/backend/
├── src/
│   ├── GymTracker.Api/
│   ├── GymTracker.Application/
│   ├── GymTracker.Domain/
│   └── GymTracker.Infrastructure/
└── tests/
    ├── GymTracker.Api.IntegrationTests/
    └── GymTracker.Application.UnitTests/

src/frontend/
├── src/
│   ├── pages/
│   ├── components/
│   ├── redux/
│   ├── services/
│   └── features/
└── tests/
```

**Structure Decision**: Se usa estructura web app existente en `src/backend` y `src/frontend` con implementacion end-to-end en backend por capas y frontend por features/pages con servicios y estado Redux.

## Implementation Strategy and Dependencies

1. Foundation backend (bloqueante): modelo `MeasurementType` canonico (`key`, `name`, `unit`, `dataType`, `category`, `description`, soft delete), validaciones y repositorio Firestore.
2. API contract and endpoints (depende 1): CRUD popup-friendly, soft delete/reactivate, `POST /import-csv`, `GET /assignable` para asignacion a ejercicios.
3. Frontend admin panel parity (depende 2): UX equivalente a musculos (acciones superiores, filtros, refrescar, popup unificado alta/edicion, import CSV).
4. Exercises assignment integration (depende 2 y 3): consumir catalogo activo para selector de mediciones en formulario de ejercicios; excluir desactivadas para nuevas asignaciones.
5. Tests and non-functional checks (depende 1-4): unit/integration/component tests, validacion de estados de carga/error/vacio y reporte por fila en importacion.
6. Compliance hardening (depende 1-5): pruebas explicitas de auth/authz en endpoints admin, verificacion de paginacion en listados grandes y prueba de volumen CSV (200 filas).

## Phase 0: Research

Resultados consolidados en [research.md](research.md):

- Definicion final del modelo `measurementTypes` y reglas de unicidad por `key`.
- Estrategia de CSV parcial con detalle por fila y errores de negocio/validacion.
- Patron de paridad UX con modulo de musculos (acciones, popup, filtros, reactivacion).
- Patron de integracion para asignacion de mediciones activas en ejercicios.

## Phase 1: Design

### Data model

- Entidades, relaciones, validaciones y transiciones documentadas en [data-model.md](data-model.md).

### API contracts

- Contrato OpenAPI en [contracts/openapi.yaml](contracts/openapi.yaml).
- Incluye endpoints de CRUD, soft delete/reactivate, importacion CSV y listado asignable para ejercicios.

### Quickstart

- Flujo de validacion manual y matriz minima de pruebas en [quickstart.md](quickstart.md).

## Constitution Check (post-design)

- OpenAPI-first: PASS (contrato definido y alineado a FR-002/003/012/014/015/016).
- Security-first: PASS (todos los endpoints admin y de asignacion definidos como protegidos).
- Data boundary: PASS con excepcion acotada al modulo admin de mediciones (sin consumo Wger en este alcance).
- Reliability: PASS (N/A Wger en este alcance).
- Quality gates: PASS (matriz de pruebas por historia definida).
- Operations: PASS (errores tipificados y metas de latencia declaradas).

## Complexity Tracking

Excepcion constitucional documentada y acotada: no uso de Wger para este modulo admin interno de mediciones.
