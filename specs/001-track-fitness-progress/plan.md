# Implementation Plan: App de Seguimiento Fitness y Progreso

**Branch**: `[001-track-fitness-progress]` | **Date**: 2026-05-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-track-fitness-progress/spec.md`

## Summary

Construir una aplicación web con backend .NET y frontend React que permita registrar entrenamientos y comidas, crear rutinas y dietas, y calcular comparativas de progreso por ejercicio. El backend actuará como gateway único para Firestore y Wger, aplicando autorización con Firebase Auth, caché de catálogo, snapshots de progreso y contrato OpenAPI consumido por el frontend.

## Technical Context

**Language/Version**: C# 12 con .NET 8 LTS; TypeScript 5.x con React 19  
**Primary Dependencies**: ASP.NET Core Web API, Swashbuckle/OpenAPI, Firebase Admin SDK, Google Cloud Firestore SDK, HttpClient para Wger, React, Material UI, React Router, TanStack Query, React Hook Form  
**Storage**: Cloud Firestore para datos del usuario y caché persistente de catálogo; caché en memoria en backend para lecturas frecuentes  
**Testing**: xUnit + FluentAssertions + WebApplicationFactory en backend; Vitest + React Testing Library en frontend  
**Target Platform**: Web app responsive para navegadores modernos en escritorio y móvil  
**Project Type**: Web application con frontend SPA + backend API  
**Performance Goals**: p95 < 500 ms para lecturas cacheadas de catálogo; p95 < 1.5 s para cálculos y consultas de progreso; visualización de comparativa < 3 s extremo a extremo  
**Constraints**: OpenAPI obligatorio; validación de Firebase Auth en endpoints protegidos; modo degradado ante caída de Wger; no estimar calorías sin dato explícito confiable  
**Scale/Scope**: Primera versión para usuarios autenticados individuales, decenas de pantallas/flows y cientos de registros por usuario

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **OpenAPI-first**: PASS. Todos los endpoints del feature quedan definidos en `contracts/openapi.yaml` y deberán mantenerse sincronizados con Swagger.
- **Security-first**: PASS. Firebase Auth se valida en backend; autorización basada en `userId` del token para aislar datos por usuario.
- **Data boundary**: PASS. Wger solo se usa para catálogo externo; Firestore almacena entrenamientos, comidas, rutinas, dietas, preferencias y snapshots de progreso.
- **Reliability**: PASS. Se define caché híbrida y modo degradado con lectura de catálogo persistido.
- **Quality gates**: PASS. El plan define pruebas unitarias, de integración de API y de componentes frontend por historia de usuario.
- **Operations**: PASS. Se incluyen health checks, logging estructurado, correlación por request y objetivos p95.

## Project Structure

### Documentation (this feature)

```text
specs/001-track-fitness-progress/
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
backend/
├── src/
│   ├── GymTracker.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Contracts/
│   │   └── Program.cs
│   ├── GymTracker.Application/
│   │   ├── Workouts/
│   │   ├── Routines/
│   │   ├── Diets/
│   │   ├── Meals/
│   │   ├── Progress/
│   │   └── Catalog/
│   ├── GymTracker.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Services/
│   └── GymTracker.Infrastructure/
│       ├── Firebase/
│       ├── Wger/
│       ├── Caching/
│       └── Observability/
└── tests/
    ├── GymTracker.Application.UnitTests/
    └── GymTracker.Api.IntegrationTests/

frontend/
├── src/
│   ├── app/
│   ├── features/
│   │   ├── workouts/
│   │   ├── progress/
│   │   ├── routines/
│   │   ├── diets/
│   │   ├── meals/
│   │   └── settings/
│   ├── shared/
│   │   ├── api/
│   │   ├── components/
│   │   ├── forms/
│   │   └── types/
│   └── theme/
└── tests/
    └── component/
```

**Structure Decision**: Se adopta una estructura de web app con backend y frontend separados para mantener frontera limpia entre API de negocio y SPA. El backend concentra acceso a Firestore, Wger y reglas de comparativas; el frontend consume exclusivamente la API autenticada.

## Phase 0: Research Output

- Gateway único .NET para negocio, seguridad y contratos.
- Firestore como persistencia principal y caché persistente de catálogo.
- Caché híbrida de Wger (memoria + Firestore).
- Servicio de dominio para comparativas con snapshots persistidos.
- Frontend SPA con React + TypeScript + Material UI.

## Phase 1: Design Output

### Domain Modules

- **Catalog**: búsqueda, filtrado y caché de ejercicios, grupos musculares, imágenes y alimentos de Wger.
- **Workouts**: registro de sesiones, ejercicios realizados y estado completo/incompleto.
- **Progress**: cálculo y lectura de comparativas por ejercicio, resúmenes semanales/mensuales y alertas de regresión.
- **Routines**: creación y reutilización de plantillas de entrenamiento por día/etiqueta y grupos musculares.
- **Diets/Meals**: planificación por días, registro de comidas y control calórico opcional.
- **Settings**: preferencias del usuario, especialmente seguimiento calórico y objetivo diario.

### API Design

- Endpoints REST bajo `/api` con JWT de Firebase en `Authorization: Bearer <token>`.
- Swagger/OpenAPI como contrato oficial.
- Errores uniformes con `application/problem+json`.
- Paginación en listados históricos y filtros por fecha/entidad.

### Security Design

- Verificación de tokens de Firebase Auth en middleware.
- Scoping por `userId` obligatorio para todos los datos personales.
- Catálogo cacheado accesible solo a través del backend para evitar filtración de secretos o inconsistencias.

### Reliability Design

- TTL de caché en memoria: 15 minutos para catálogos consultados frecuentemente.
- TTL de caché persistente: 24 horas, con `stale-while-revalidate` en backend.
- En fallo de Wger, devolver catálogo cacheado con indicador `catalogStatus=stale` si existe; si no existe, devolver error controlado y mensaje accionable.

### Observability Design

- Correlation ID por request.
- Logging estructurado por operación de API, lectura de proveedor externo y cálculo de progreso.
- Health checks separados para API, Firestore y conectividad básica con Wger.

## Testing Strategy

- **Backend unit tests**: comparativas, validación de calorías, reglas de sesiones incompletas, composición de rutinas/dietas.
- **Backend integration tests**: autenticación, autorización, contratos de endpoints, fallback de Wger, persistencia en Firestore emulada o aislada.
- **Frontend component tests**: formularios de entrenamiento, rutina, dieta, selector multiselección, estados loading/error/empty, panel de progreso.
- **Contract validation**: el backend debe generar Swagger alineado con `contracts/openapi.yaml` y pasar tests de respuesta mínima para endpoints críticos.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Ninguna | N/A | El diseño actual cumple la constitución sin excepciones |
