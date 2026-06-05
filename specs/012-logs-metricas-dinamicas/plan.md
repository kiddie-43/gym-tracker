# Implementation Plan: Logs Metricos Dinamicos de Entrenamiento

**Branch**: `012-git-feature-hook` | **Date**: 2026-05-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/012-logs-metricas-dinamicas/spec.md`

## Summary

Reemplazar el modelo actual de logs de ejercicio por un esquema dinamico de metricas agrupadas por `idGrupo` (set de captura), con reglas estrictas de inmutabilidad (solo `valorMetrica` editable), borrado logico exclusivo por grupo completo, consulta diaria por fecha actual y resolucion de vigencia por mayor `CreatedAt`. El alcance incluye backend API REST con contrato OpenAPI `2.0.0` sobre `/api/training-metric-logs/*` como unica superficie soportada, retiro del flujo legacy en el mismo despliegue, trazabilidad de cambios, estado Redux en frontend, servicio de consumo y pagina de detalle de logs con `PopupDialog` y `ActionMenu` del sistema existente.

## Technical Context

**Language/Version**: Backend C# .NET 8; Frontend TypeScript 5.8 + React 19  
**Primary Dependencies**: ASP.NET Core, middleware de autenticacion del proveedor configurado, SQL persistence layer actual del backend, React, Redux Toolkit, Material UI  
**Storage**: SQL transaccional del backend para logs metricos y trazas; catalogo de metricas desde dominio admin/catalogo  
**Testing**: xUnit (unit + integration) en backend; Vitest + Testing Library en frontend  
**Target Platform**: Web SPA + API REST en entorno local y despliegue actual del proyecto
**Project Type**: Aplicacion web full-stack en monorepo  
**Performance Goals**: p95 < 2s para consultas de logs diarios con 20 usuarios concurrentes durante 10 minutos y dataset semilla de 10.000 registros; operaciones CRUD perceptiblemente inmediatas en UI  
**Constraints**: Edicion solo `valorMetrica`; borrado logico solo por `idGrupo`; `idGrupo` unico global e inmutable; paginacion obligatoria en listados grandes; sin `any` injustificado en frontend; estados carga/error/vacio obligatorios  
**Scale/Scope**: 1 modulo de logs metricos dinamicos (backend + frontend) con CRUD, trazabilidad, contratos y detalle UI
**Terminologia Canonica**: `idGrupo` como nombre de dominio; `groupId` solo como alias de contrato cuando aplique

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: PASS. Contrato definido en `contracts/openapi.yaml` para obtencion de metricas, consulta diaria, create, update de valor, delete logico por grupo y version contractual `2.0.0` en `/api/training-metric-logs/*` como unica superficie soportada.
- Security-first: PASS. Endpoints planificados bajo usuario autenticado y ownership por `idUsuario`.
- Data boundary: PASS. Datos transaccionales de logs y eventos de cambio quedan en almacenamiento principal; catalogo/definiciones se consumen como datos de referencia.
- Reliability: PASS. Regla diaria determinista (fecha actual + mayor `CreatedAt`) y respuesta `[]` sin datos elimina ambiguedad operativa.
- Quality gates: PASS. Cobertura de pruebas backend/frontend definida por historia y criterios medibles.
- Operations: PASS. Trazabilidad de mutaciones, health checks de dependencias criticas y criterios de comportamiento observables incluidos en el plan.

## Project Structure

### Documentation (this feature)

```text
specs/012-logs-metricas-dinamicas/
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
│   │   └── Controllers/Training/
│   ├── GymTracker.Application/
│   │   ├── Contracts/Training/
│   │   └── Features/Training/
│   ├── GymTracker.Domain/
│   │   └── Entities/Training/
│   └── GymTracker.Infrastructure/
│       └── Sql/
└── tests/
    ├── GymTracker.Application.UnitTests/
    └── GymTracker.Api.IntegrationTests/

src/frontend/
├── src/
│   ├── interfaces/
│   ├── redux/
│   ├── services/
│   ├── pages/
│   │   └── routines/
│   │       └── exercices/
│   │           └── detail/
│   └── components/
└── tests/
```

**Structure Decision**: Se mantiene la arquitectura full-stack existente. Backend implementa el modulo en capas Training/* y frontend integra estado global, servicios y pagina de detalle respetando componentes reutilizables. El formulario de detalle se implementa en `pages/routines/exercices/detail`.

## Implementation Strategy and Dependencies

1. Foundation de dominio y persistencia (bloqueante): nuevo modelo `idGrupo`, reglas de inmutabilidad, borrado logico por grupo y consulta diaria.
2. API + contratos OpenAPI (depende 1): endpoints de metricas, consulta diaria, create, edit valor y delete logico grupo.
3. Frontend estado/servicios/vista (depende 2): interfaz unica, redux module, servicio y pagina detalle con popups/menu.
4. Hardening y pruebas (depende 1-3): validaciones de ownership, no edicion de campos inmutables, respuesta `[]`, aislamiento por grupo, health checks de dependencias criticas, degradacion controlada y verificacion de p95.

## Phase 0: Research

Resultados en [research.md](research.md):

- Modelo de agrupacion por set con `idGrupo` unico.
- Edicion restringida a `valorMetrica`.
- Borrado logico solo por grupo completo.
- Resolucion diaria por fecha actual + mayor `CreatedAt`.
- Respuesta vacia sin datos diarios.

## Phase 1: Design

### Data model

Entidades, relaciones y reglas de negocio en [data-model.md](data-model.md).

### API contracts

Contrato OpenAPI en [contracts/openapi.yaml](contracts/openapi.yaml), cubriendo:

- Catalogo de metricas habilitadas.
- Listado de logs metricos paginado para consultas potencialmente grandes.
- Consulta diaria de logs metricos.
- Creacion de grupo de metricas.
- Actualizacion de valor por `groupId + metricId`.
- Borrado logico del grupo completo.
- Convenciones de versionado/sunset deterministas: contrato `2.0.0`, namespace soportado `/api/training-metric-logs/*` y retiro del flujo legacy en el mismo despliegue.

### Quickstart

Flujo de validacion manual y alcance de pruebas en [quickstart.md](quickstart.md).

## Constitution Check (post-design)

- OpenAPI-first: PASS. Endpoints principales y errores estan contractualmente definidos.
- Security-first: PASS. Contrato asume autenticacion y ownership por usuario.
- Data boundary: PASS. Separa metricas de referencia de datos transaccionales del usuario.
- Reliability: PASS. Regla determinista de vigencia diaria y retorno `[]` sin datos.
- Quality gates: PASS. Matriz de pruebas backend/frontend especificada.
- Operations: PASS. Eventos de cambio, health checks de dependencias criticas y borrado logico trazables.

## Complexity Tracking

ADR registrado en `docs/adr/ADR-012-logs-metricos-dinamicos.md` para documentar el rediseño y la estrategia de sunset.
