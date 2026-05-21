# Implementation Plan: Pagina de Rutinas de Entrenamiento

**Branch**: `011-prepare-spec-branch` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/011-rutinas-entrenamiento/spec.md`

## Summary

Implementar la experiencia de rutinas de usuario con vista en tarjetas y ejecucion guiada en stepper full-screen dentro de la misma ruta, incluyendo planificacion jerarquica (`rutina -> sesion -> ejercicio -> datos`), inicio/cancelacion de entrenamiento con bloqueo de navegacion externa, restauracion de estado tras recarga/reingreso, y registro por usuario de series realizadas, notas y adjuntos multimedia (maximo 5). La solucion cubre backend API REST con Firebase Auth, persistencia transaccional en Firebase, frontera de catalogo externo (Wger) y frontend React con estados de carga/error/vacio, ademas de comparativas de progreso por ejercicio (ultima sesion, promedio ultimas 4, mejor marca) con metricas obligatorias.

## Technical Context

**Language/Version**: Backend C# .NET 8; Frontend TypeScript 5.8 + React 19  
**Primary Dependencies**: ASP.NET Core, Firebase Auth/Admin SDK, Firestore, React, Material UI v7, Redux Toolkit, react-i18next  
**Storage**: Firebase/Firestore para estado transaccional de usuario; Wger solo para catalogo externo via backend boundary  
**Testing**: xUnit (unit/integration) en backend, Vitest + Testing Library en frontend  
**Target Platform**: Web SPA + API REST ASP.NET Core
**Project Type**: Aplicacion web full-stack en monorepo  
**Performance Goals**: p95 <= 250ms en endpoints CRUD principales; restauracion de estado de entrenamiento <= 1.5s en entorno local; flujo stepper sin bloqueos perceptibles entre pasos  
**Constraints**: OpenAPI obligatorio; Firebase Auth obligatorio en endpoints protegidos; borrado logico obligatorio; maximo 5 adjuntos por log; bloqueo de navegacion solo dentro del arbol de entrenamiento cuando `trainingLocked=true`; TypeScript estricto sin `any` no justificado  
**Scale/Scope**: 1 modulo de rutinas de usuario + flujo de entrenamiento activo + persistencia de logs por ejercicio + contratos API para frontend

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: PASS. Contrato definido en `contracts/openapi.yaml` para rutas de rutinas, sesiones, training flow y logs de ejercicio.
- Security-first: PASS. Endpoints de usuario autenticado requieren Firebase Auth y aislamiento por `userId`.
- Data boundary: PASS. Wger se usa solo como catalogo de ejercicios; datos transaccionales (rutinas, flow state, logs, notas, adjuntos) en Firebase.
- Reliability: PASS con alcance acotado. Se planifica modo degradado para catalogo externo no disponible, manteniendo operativas rutinas guardadas.
- Quality gates: PASS. Cobertura de pruebas backend/frontend trazada a historias P1/P2.
- Operations: PASS. Logging estructurado, codigos de error consistentes y metas p95 incluidas en el plan.

## Project Structure

### Documentation (this feature)

```text
specs/011-rutinas-entrenamiento/
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
│   ├── features/
│   ├── redux/
│   ├── services/
│   └── interfaces/
└── tests/
```

**Structure Decision**: Se mantiene arquitectura web existente por capas en backend y modular por features/pages en frontend para implementar end-to-end rutinas + training flow sin introducir nuevos subproyectos.

## Implementation Strategy and Dependencies

1. Foundation domain/backend (bloqueante): entidades y reglas de negocio de rutina/sesion/vinculo/log/estado activo con soft delete y aislamiento por usuario.
2. API + contracts (depende 1): endpoints de rutinas, sesiones, training flow y exercise training logs con OpenAPI sincronizado.
3. Frontend rutinas/cards + stepper (depende 2): vista tarjetas, detalle, stepper full-screen en misma ruta y botones iniciar/cancelar.
4. Persistencia de estado y recuperacion (depende 2 y 3): guardado/restore de `trainingFlowState` para F5/reingreso.
5. Multimedia y notas por ejercicio (depende 2 y 3): formulario de log con max 5 adjuntos, validaciones y privacidad por usuario.
6. Quality and hardening (depende 1-5): pruebas unitarias/integracion/componentes, estados de error/degradado, validacion de authn/authz, comparativas de progreso y performance p95.

## Phase 0: Research

Resultados documentados en [research.md](research.md):

- Flujo stepper full-screen en misma ruta con arbol fijo.
- Bloqueo de navegacion durante entrenamiento activo y cancelacion explicita.
- Restauracion de estado tras recarga/reingreso.
- Reglas de catalogo, desvinculacion logica y limite de adjuntos.
- Estrategia de concurrencia simplificada `last write wins` para esta fase.

## Phase 1: Design

### Data model

Entidades, relaciones, reglas de validacion y transiciones en [data-model.md](data-model.md).

### API contracts

Contrato OpenAPI en [contracts/openapi.yaml](contracts/openapi.yaml), cubriendo:

- CRUD logico de rutinas.
- Gestion de sesiones y ejercicios vinculados.
- Inicio/cancelacion/actualizacion/restauracion de flujo de entrenamiento.
- Registro y consulta de logs de entrenamiento por ejercicio.

### Quickstart

Flujo manual y matriz minima de pruebas en [quickstart.md](quickstart.md).

## Constitution Check (post-design)

- OpenAPI-first: PASS. Contratos alineados a FR-001..FR-041.
- Security-first: PASS. Endpoints protegidos, ownership por usuario y lectura privada de notas/adjuntos.
- Data boundary: PASS. Catalogo externo desacoplado de transacciones de usuario.
- Reliability: PASS. Restauracion de estado + degradacion catalogo consideradas en diseño.
- Quality gates: PASS. Plan de pruebas backend/frontend definido por historia.
- Operations: PASS. Logging y metas operativas identificadas para endpoints criticos.

## Complexity Tracking

No se registran violaciones constitucionales que requieran excepcion en esta fase.
