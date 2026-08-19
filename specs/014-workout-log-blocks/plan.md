# Implementation Plan: Registrar Entrenamiento por Bloques (Plan Mensual)

**Branch**: `014-workout-log-blocks` | **Date**: 2026-08-07 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/014-workout-log-blocks/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Sustituir el tab "Registro" (renombrado a "Registrar") de [ExerciseDetail.tsx](../../src/frontend/src/pages/routines/monthlyPlan/components/ExerciseDetail.tsx) por un modelo de registro de entrenamiento basado en resumen rapido (3 metricas dependientes del perfil del `ExerciseType`), estructura de bloques (`SessionBlock`) y notas opcionales, persistido como `TrainingSession`. Se añade un modulo backend nuevo (`TrainingSessions`) siguiendo el patron ya validado de `TrainingLog` (misma tripleta `WeekNumber`/`DayNumber`/ejercicio, `AuditableEntity`, `CurrentUserControllerBase`), un catalogo estatico de `BlockTemplate` por perfil (fuerza/cardio, sin tabla), y un modulo Redux + servicio API nuevos en frontend. Se reutiliza `PopupDialog` para los formularios de bloque, `Units` para las unidades de las metricas, y se elimina por completo el codigo obsoleto de captura por set en `ExerciseDetail.tsx`.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (backend); TypeScript 5 / React 19 (frontend)
**Primary Dependencies**: ASP.NET Core Web API, EF Core + SQL Server (backend); MUI 7 (`@mui/material` + `@mui/icons-material` unicamente), Redux Toolkit, i18next, Vite (frontend)
**Storage**: SQL Server via EF Core (`AdminDbContext`) — nuevas tablas `TrainingSessions` y `SessionBlocks`; `BlockTemplate` es contenido estatico en memoria (Application layer), sin tabla
**Testing**: `GymTracker.Application.UnitTests` y `GymTracker.Api.IntegrationTests` (backend); tests de componentes/reducers existentes en frontend (Vitest/Testing Library, siguiendo el patron ya usado en el repo)
**Target Platform**: Web (SPA React servida por Vite, API ASP.NET Core sobre Linux/Windows containers)
**Project Type**: Web application (backend + frontend ya existentes en el monorepo)
**Performance Goals**: Consistente con el resto del proyecto — sin objetivo nuevo de throughput; listados de historial por ejercicio planificado son de bajo volumen (decenas de sesiones), no requieren paginacion segun el spec
**Constraints**: Sin dependencias externas nuevas (INT-001/INT-003); no se toca `TrainingLog` ni el flujo `routines/exercices` (FR-022); migraciones EF Core deben ser aditivas y limpias, sin arrastrar cambios de esquema no relacionados con esta feature
**Scale/Scope**: 1 pantalla rediseñada (`ExerciseDetail.tsx`), 2 entidades nuevas (`TrainingSession`, `SessionBlock`), 1 catalogo estatico (`BlockTemplate`), ~4 endpoints nuevos, 1 modulo Redux nuevo

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- OpenAPI-first: los endpoints nuevos de `TrainingSessions` (crear, obtener por id, listar historial por ejercicio planificado, plantillas de bloques) se documentan en `contracts/openapi.yaml` de esta feature y se integran en el contrato oficial (`src/backend/src/GymTracker.Api/Contracts/openapi.yaml`) durante la implementacion. No hay cambios breaking sobre contratos existentes.
- Security-first: todos los endpoints nuevos heredan `CurrentUserControllerBase` (mismo mecanismo de identidad de desarrollo ya usado por `TrainingLogsController`); el acceso a una `TrainingSession` se filtra siempre por `UserId` del usuario autenticado (FR-023).
- Data boundary: `Exercice`/`ExerciseType`/`Units` se tratan como catalogo de referencia ya existente (sin proveedor externo nuevo); `TrainingSession`/`SessionBlock` son datos transaccionales del usuario en SQL, igual que `TrainingLog` (INT-001/INT-002).
- Reliability: no hay dependencia externa nueva que degradar (INT-003); si el catalogo de ejercicios/unidades no esta disponible, la pantalla reutiliza los mismos estados de error/vacio ya usados en el resto del Plan Mensual.
- Quality gates: alcance de test definido en [quickstart.md](./quickstart.md) — unit tests de `TrainingSessionService`/entidades de dominio, integration tests de los endpoints nuevos, y tests de reducer/componentes en frontend para el modulo Redux y `ExerciseDetail.tsx`.
- Operations: logging estructurado y manejo de errores reutilizan la infraestructura ya existente en `GymTracker.Api` (sin health checks nuevos porque no hay dependencia externa nueva que vigilar).

## Project Structure

### Documentation (this feature)

```text
specs/014-workout-log-blocks/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
# Web application (backend + frontend ya existentes) — estructura real usada por esta feature

src/backend/src/
  GymTracker.Domain/Entities/TrainingSession/
    TrainingSession.cs
    SessionBlock.cs
  GymTracker.Domain/Enum/
    SessionBlockType.cs          # WARMUP/APPROACH/WORK/REST (fuerza) + SWIM/SERIES/TECHNIQUE/REST (cardio)

  GymTracker.Application/TrainingSession/
    ITrainingSessionRepository.cs
    TrainingSessionContracts.cs
    TrainingSessionService.cs
    BlockTemplateCatalog.cs      # contenido estatico por perfil, sin tabla (INT-004)

  GymTracker.Infrastructure/TrainingSession/
    SqlTrainingSessionRepository.cs

  GymTracker.Api/Controllers/TrainingSessions/
    TrainingSessionsController.cs

  GymTracker.Infrastructure/Admin/AdminDbContext.cs      # + DbSet y Configure<Entidad> nuevos
  GymTracker.Infrastructure/Migrations/
    <timestamp>_AddTrainingSessionsModule.cs
    AdminDbContextModelSnapshot.cs                        # actualizado, sin cambios de otros modulos

src/backend/tests/
  GymTracker.Application.UnitTests/TrainingSession/
  GymTracker.Api.IntegrationTests/TrainingSessions/

src/frontend/src/
  pages/routines/monthlyPlan/components/
    ExerciseDetail.tsx                         # se limpia el codigo obsoleto y se reescribe el tab "Registrar"
    QuickSummaryCard/QuickSummaryCard.tsx
    SessionBlocksList/SessionBlocksList.tsx
    SessionBlockFormDialog/SessionBlockFormDialog.tsx
    EditQuickSummaryDialog/EditQuickSummaryDialog.tsx
    SessionHistoryList/SessionHistoryList.tsx

  interfaces/trainingSessions/
    trainingSessions.ts

  services/api/trainingSessions/
    trainingSessionsApi.ts

  redux/actions/trainingSessions/trainingSessionsActions.ts
  redux/reducers/trainingSessions/trainingSessionsReducer.ts
  redux/states/trainingSessions/trainingSessionsState.ts
  redux/store.ts                                # + registro del reducer nuevo

  i18n/lenguage/es.ts                           # + claves monthlyPlan.* nuevas, record -> "Registrar"
  i18n/lenguage/en.ts                           # + claves equivalentes en ingles
```

**Structure Decision**: Se reutiliza la arquitectura por capas ya existente del backend (`GymTracker.Domain/Application/Infrastructure/Api`) y la estructura de carpetas del frontend (`pages/<dominio>/components`, `redux/{actions,reducers,states}/<modulo>`, `services/api/<modulo>`, `interfaces/<dominio>`) descritas en las skills `backend-modular`, `backend-module-structure-crud`, `frontend-architecture`, `frontend-redux` y `frontend-services`. No se crea ningun proyecto, paquete ni capa nueva fuera de las ya existentes.

## Complexity Tracking

> No hay violaciones de la constitucion ni desviaciones de las skills que requieran justificacion mas alla de las documentadas explicitamente en [research.md](./research.md) (Decisiones 3, 5 y 8), que son simplificaciones deliberadas, no incrementos de complejidad.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
