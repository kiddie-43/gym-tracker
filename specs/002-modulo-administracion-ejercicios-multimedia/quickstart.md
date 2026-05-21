# Quickstart: Modulo de Administracion de Ejercicios con Multimedia

## Goal

Levantar backend y frontend para validar el panel `/admin`, el CRUD completo del catalogo maestro, la subida de multimedia con Firebase Storage y el consumo del catalogo admin desde workouts.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Proyecto Firebase con Firestore y Storage habilitados
- Bucket de Firebase Storage accesible por la cuenta de servicio del backend
- Credenciales de servicio Firebase para backend
- Claim admin disponible en tokens Firebase (`custom.admin=true`)

## Environment Variables

### Backend

Configurar en `backend/src/GymTracker.Api/appsettings.Development.json` o User Secrets:

- `Firebase:ProjectId`
- `Firebase:CredentialsPath` o `Firebase:ServiceAccountJson`
- `Firebase:ClientEmail`
- `Firebase:PrivateKey`
- `Firebase:StorageBucket`
- `ASPNETCORE_ENVIRONMENT=Development`

### Frontend

- `VITE_API_BASE_URL`
- `VITE_FIREBASE_API_KEY`
- `VITE_FIREBASE_AUTH_DOMAIN`
- `VITE_FIREBASE_PROJECT_ID`

## Run Backend

1. `cd backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`
5. Abrir Swagger y verificar `GET /health`.

## Run Frontend

1. `cd frontend`
2. `npm install`
3. `npm run dev`
4. Abrir la SPA en el puerto reportado por Vite.

## Manual Validation Flow

1. Iniciar sesion con un usuario administrador.
2. Acceder a `/admin` y verificar bloqueo para usuarios sin claim admin.
3. Crear un `MuscleGroup`, un `Muscle` asociado a multiples grupos, un `ExerciseType` y un `ExerciseFormType`.
4. Crear un `Exercise` activo usando esas entidades.
5. Solicitar `POST /api/admin/exercises/{exerciseId}/media/upload-url` para una imagen, subir el archivo al bucket y confirmar con `POST /api/admin/exercises/{exerciseId}/media`.
6. Repetir con mas imagenes y al menos un video, luego reordenar y marcar portada.
7. Intentar exceder limites de cantidad y tamano para verificar validaciones.
8. Ir al flujo de nuevo workout y confirmar que el selector consume solo ejercicios admin activos.
9. Registrar un workout y comprobar que el detalle guarda snapshot minimo del ejercicio.
10. Desactivar o borrar logicamente el ejercicio y verificar que desaparece del selector, pero sigue visible en historial y detalle del workout ya creado.

## Minimum Test Matrix

- Backend unit tests para validaciones de codigo unico, borrado logico, limites de media y snapshot mapping.
- Backend integration tests para todos los endpoints `/api/admin/*`, roles admin/no-admin y flujo de confirmacion/compensacion de media.
- Frontend component tests para guard de `/admin`, tablas CRUD, form builder, uploader multimedia y selector de workouts consumiendo catalogo admin.

## Expected Deliverables from Implementation

- Nuevos controladores admin en backend con contrato OpenAPI sincronizado.
- Servicios/repositorios Firestore para entidades admin e indices de unicidad.
- Servicio de Storage con signed URLs y borrado compensatorio.
- Nuevas paginas y componentes `frontend/src/features/admin/*`.
- Adaptacion del selector de workouts a un endpoint dedicado de catalogo admin.

## Validation Results

- Backend build: `dotnet build backend/GymTracker.sln --no-restore` en verde.
- Backend tests US2/US3: `ExercisesEndpointsTests`, `ExerciseMediaEndpointsTests`, `MediaCompensationTests`, `WorkoutCatalogEndpointsTests`, `WorkoutSnapshotTests`, `WorkoutHistoryCompatibilityTests` en verde.
- Frontend build: `npm run build` en verde.
- Frontend component tests: `AdminCrudTables.test.tsx`, `ExerciseMediaUploader.test.tsx`, `AdminExerciseSelector.test.tsx` en verde.
