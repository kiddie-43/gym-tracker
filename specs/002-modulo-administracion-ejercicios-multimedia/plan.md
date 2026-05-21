# Implementation Plan: Modulo de Administracion de Ejercicios con Multimedia

**Branch**: `[002-modulo-administracion-ejercicios-multimedia]` | **Date**: 2026-05-11 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-modulo-administracion-ejercicios-multimedia/spec.md`

## Summary

Construir un modulo administrativo completo en backend .NET y frontend React para gestionar grupos musculares, musculos, tipos de ejercicio, tipos de formulario y ejercicios con multimedia, persistiendo metadatos en Firestore y binarios en Firebase Storage. El modulo debe proteger rutas admin con Firebase Auth, ofrecer signed uploads para media, y exponer un catalogo dedicado para que workouts consuma solo ejercicios admin activos sin romper historico gracias a snapshots minimos persistidos.

## Technical Context

**Language/Version**: C# 12 con .NET 8 LTS; TypeScript 5.x con React 19  
**Primary Dependencies**: ASP.NET Core Web API, Swashbuckle/OpenAPI, Google.Cloud.Firestore, Google.Cloud.Storage.V1, Firebase token validation, React 19, Material UI 7, Redux Toolkit, React Router 7  
**Storage**: Firestore para metadatos admin mediante `appDocuments` con claves logicas `admin/...`; Firebase Storage para binarios multimedia; Firestore existente para workouts/routines con snapshots  
**Testing**: xUnit + WebApplicationFactory en backend; Vitest + React Testing Library en frontend  
**Target Platform**: Web responsive para navegadores modernos en escritorio y movil  
**Project Type**: Web application con backend API + frontend SPA  
**Performance Goals**: p95 < 500 ms para listados admin paginados; emision de upload URL < 1 s; confirmacion de media < 2 s; selector de ejercicios de workout actualizado < 5 s tras persistencia  
**Constraints**: todas las rutas `/api/admin/*` requieren claim admin verificado; borrado logico obligatorio; maximo 8 archivos por ejercicio; uploads via URL firmada; historial existente de workouts no se migra destructivamente; UI visible en espanol  
**Scale/Scope**: 5 paneles admin, 1 galeria multimedia por ejercicio, 1 endpoint dedicado para selector de workouts, reglas de seguridad y pruebas backend/frontend para toda la superficie nueva

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate

- **OpenAPI-first**: PASS. El plan incluye contrato dedicado en `specs/002-modulo-administracion-ejercicios-multimedia/contracts/openapi.yaml` y sincronizacion posterior con Swagger del backend.
- **Security-first**: PASS. Se planifica validacion real de Firebase ID tokens, politica `AdminOnly` para `/api/admin/*` y guard de `/admin` en frontend.
- **Data boundary**: PASS. Firestore y Storage alojan el catalogo admin propio; los workouts siguen en Firestore del usuario y mantienen snapshots. Wger permanece como catalogo externo existente, pero el selector de workouts de esta feature migra a un endpoint admin dedicado aprobado por la spec.
- **Reliability**: PASS. El plan define signed uploads, compensacion ante fallos parciales Firestore/Storage y filtro de borrado logico para no romper historico.
- **Quality gates**: PASS. Se definen pruebas unitarias, integracion y componentes para CRUD admin, media y adaptacion de workouts.
- **Operations**: PASS. Se mantienen logging estructurado, correlacion por request, health checks y objetivos p95 para las nuevas superficies.

### Post-Design Re-Check

- **OpenAPI-first**: PASS. El contrato inicial cubre CRUD admin, media y catalogo dedicado de workouts.
- **Security-first**: PASS. El diseno aplica backend-only para mutaciones y signed URLs limitadas en el tiempo.
- **Data boundary**: PASS. Se evita acoplar la UI directamente a Firestore/Storage; el backend sigue siendo frontera operativa.
- **Reliability**: PASS. El agregado `Exercise` embebe `media[]` para invariantes atomicas y simplifica compensaciones.
- **Quality gates**: PASS. El alcance de pruebas queda trazado por historia y por endpoint.
- **Operations**: PASS. No se requieren excepciones constitucionales.

## Project Structure

### Documentation (this feature)

```text
specs/002-modulo-administracion-ejercicios-multimedia/
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
│   │   ├── Options/
│   │   ├── Contracts/
│   │   └── Program.cs
│   ├── GymTracker.Application/
│   │   ├── Admin/
│   │   │   ├── MuscleGroups/
│   │   │   ├── Muscles/
│   │   │   ├── ExerciseTypes/
│   │   │   ├── ExerciseFormTypes/
│   │   │   └── Exercises/
│   │   ├── Workouts/
│   │   └── Common/
│   ├── GymTracker.Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   └── GymTracker.Infrastructure/
│       ├── Firebase/
│       ├── Storage/
│       └── Observability/
└── tests/
    ├── GymTracker.Application.UnitTests/
    │   └── Admin/
    └── GymTracker.Api.IntegrationTests/
        ├── Admin/
        └── Workouts/

frontend/
├── src/
│   ├── app/
│   ├── features/
│   │   ├── admin/
│   │   │   ├── api/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   └── state/
│   │   └── workouts/
│   ├── shared/
│   └── types/
└── tests/
    └── component/
        ├── admin/
        └── workouts/

firebase/
└── firestore.rules
```

**Structure Decision**: Se mantiene la separacion backend/frontend existente y se agrega un modulo `Admin` transversal en Application, Controllers y frontend `features/admin`. `Workouts` se adapta solo en los contratos y selectores necesarios para consumir el catalogo admin sin redisenar el flujo completo.

## Phase 0: Research Output

- Persistencia admin sobre `FirestoreContext` actual con claves logicas `admin/...` e indices `admin-index/...`.
- `ExerciseMedia[]` embebido dentro de `Exercise` para garantizar limites, portada y orden de forma atomica.
- URLs firmadas V4 con `Google.Cloud.Storage.V1` para upload directo desde frontend y confirmacion posterior del metadato.
- Politica `AdminOnly` y validacion real de Firebase ID tokens para todas las mutaciones admin.
- Endpoint dedicado de catalogo de workouts respaldado solo por ejercicios admin activos.
- Snapshot minimo de ejercicio en workouts y rutinas para preservar historico y desacoplarlo de URLs efimeras.
- Borrado logico con `active`, `isDeleted` y `deletedAt` en todas las entidades admin.

## Phase 1: Design Output

### Domain Modules

- **Admin Catalog**: CRUD de `MuscleGroup`, `Muscle`, `ExerciseType`, `ExerciseFormType` y `Exercise` con validaciones de unicidad, relaciones y borrado logico.
- **Admin Media**: emision de upload URLs, confirmacion de archivo, reordenamiento, set primary y eliminacion con compensacion Storage/Firestore.
- **Workout Exercise Catalog**: endpoint de busqueda para workouts que devuelve solo ejercicios admin activos y shape compatible con el selector actual.
- **Workout Snapshot Adaptation**: evolucion de contratos y mapeos para guardar `exerciseId` y snapshot minimo sin romper historial existente.

### API Design

- CRUD REST bajo `/api/admin/*` con paginacion, busqueda, `includeInactive` y `problem+json` para errores.
- Endpoints multimedia bajo `/api/admin/exercises/{exerciseId}/media*`.
- Endpoint dedicado `/api/workouts/exercise-catalog` para el selector de workouts.
- Sin cambios destructivos inmediatos sobre `/api/catalog/exercises`; se depreca su uso para workouts en favor del endpoint dedicado.

### Security Design

- Verificacion de Firebase ID tokens en middleware/autenticacion.
- Politicas `AuthenticatedUser` y `AdminOnly` en backend.
- Guard de `/admin` en frontend usando claims de usuario autenticado.
- Signed URLs limitadas por expiracion, `contentType` y tamano.

### Reliability Design

- Confirmacion de media solo despues de verificar existencia del objeto en Storage.
- Compensacion por borrado de objeto si falla la persistencia del metadato.
- Borrado logico para no romper workouts y rutinas historicas.
- Resolucion de URLs de lectura en backend para evitar almacenar enlaces publicos permanentes.

### Observability Design

- Logging estructurado para CRUD admin, emision de signed URLs, confirmacion de media y compensaciones.
- Correlation ID heredado de la infraestructura actual.
- Health checks existentes se mantienen; se agrega chequeo de configuracion de `StorageBucket` cuando aplique.

## Testing Strategy

- **Backend unit tests**: validaciones de codigo unico, reglas de formulario dinamico, limites de media, invariantes de `isPrimary`, mapping de snapshots y filtros por borrado logico.
- **Backend integration tests**: `401/403` en `/api/admin/*`, CRUD completo por entidad, upload-url, confirm media, reorder, set-primary, delete y lectura de catalogo para workouts.
- **Frontend component tests**: guard de `/admin`, tablas CRUD, builder de campos, uploader multimedia, selector de workouts usando solo catalogo admin activo.
- **Contract validation**: sincronizacion posterior con `backend/src/GymTracker.Api/Contracts/openapi.yaml` y cobertura de respuestas minimas para endpoints criticos.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Ninguna | N/A | El diseno cumple la constitucion sin excepciones |
