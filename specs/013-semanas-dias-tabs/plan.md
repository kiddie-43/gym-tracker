# Implementation Plan: Configuracion Semanas y Dias

**Branch**: `013-replace-semanas-dias-tabs` | **Date**: 2026-07-27 | **Spec**: `/specs/013-semanas-dias-tabs/spec.md`
**Input**: Feature specification from `/specs/013-semanas-dias-tabs/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Reemplazar el flujo de planificacion `Rutina > Sesion` por `Semana > Dia` con 4 semanas fijas y un unico numero de dias global (1..7) aplicado al mes completo. La implementacion requiere migracion dura de datos historicos al nuevo esquema, control de acceso exclusivo por propietario del plan, y regla de recorte al reducir dias (vaciar plan activo fuera de rango preservando historico de estadisticas).

## Technical Context

**Language/Version**: Backend C#/.NET 8, Frontend TypeScript 5 + React 19  
**Primary Dependencies**: ASP.NET Core, React Router, Redux Toolkit, Material UI 7, i18next  
**Storage**: Persistencia transaccional de planes/ejercicios de usuario en almacenamiento principal del proyecto; catalogo externo Wger solo para referencia  
**Testing**: Backend xUnit (unit + integration), Frontend Vitest + Testing Library  
**Target Platform**: Web app (SPA + API REST) en entorno local y despliegues server-side
**Project Type**: Aplicacion web full-stack (frontend + backend)  
**Performance Goals**: p95 < 300 ms en lecturas de plan mensual y p95 < 500 ms en guardado/recorte de plan  
**Constraints**: 4 semanas fijas, dias globales en rango 1..7, migracion en una sola entrega sin ventana de compatibilidad, edicion solo por propietario  
**Scale/Scope**: Alcance funcional de planificacion mensual del usuario autenticado; UI con doble nivel de tabs y configuracion de ejercicios por Semana > Dia

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: every planned endpoint includes OpenAPI contract update and versioning impact (if breaking).
- Security-first: Firebase Auth token validation + authorization model defined per endpoint category.
- Data boundary: Wger used only as external catalog; Firebase stores all user transactional history.
- Reliability: Wger cache strategy, expiration policy, and degraded-mode behavior are explicitly planned.
- Quality gates: required backend/frontend test scope is defined and mapped to user stories.
- Operations: structured logging, correlation IDs, health checks, and p95 latency goals are specified.

Resultado pre-Phase 0: PASS

- OpenAPI-first: se agrega contrato en `specs/013-semanas-dias-tabs/contracts/openapi.yaml` con impacto breaking por migracion dura.
- Security-first: reglas de autorizacion definidas (propietario exclusivo para create/update/read).
- Data boundary: se mantiene separacion catalogo externo vs datos transaccionales de usuario.
- Reliability: se define plan de degradacion para lectura/guardado y operacion de recorte por dias.
- Quality gates: pruebas backend/frontend mapeadas por historias P1 y P2 en `quickstart.md`.
- Operations: se exigen logs estructurados, correlacion y health checks en endpoints nuevos.

## Project Structure

### Documentation (this feature)

```text
specs/013-semanas-dias-tabs/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
```text
src/
├── backend/
│   ├── src/
│   │   ├── GymTracker.Api/
│   │   ├── GymTracker.Application/
│   │   ├── GymTracker.Domain/
│   │   └── GymTracker.Infrastructure/
│   └── tests/
│       ├── GymTracker.Api.IntegrationTests/
│       └── GymTracker.Application.UnitTests/
└── frontend/
  ├── src/
  │   ├── components/
  │   ├── pages/
  │   ├── redux/
  │   └── services/
  └── tests/
    ├── component/
    └── redux/
```

**Structure Decision**: Se adopta arquitectura web full-stack existente (`src/backend` + `src/frontend`) con cambios focalizados en modulo de planificacion mensual y contratos API del backend.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Ninguna | N/A | N/A |

## Constitution Check (Post-Design)

Resultado post-Phase 1: PASS

- Contrato OpenAPI definido para CRUD de plan mensual y accion de migracion.
- Seguridad y autorizacion del propietario reflejadas en reglas de acceso.
- Fronteras de datos y estrategia de degradacion documentadas en `research.md`.
- Cobertura de pruebas y pasos de verificacion definidos en `quickstart.md`.
- Objetivos operativos de latencia y observabilidad incorporados para implementacion.
