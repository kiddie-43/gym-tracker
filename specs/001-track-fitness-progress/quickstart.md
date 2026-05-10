# Quickstart: App de Seguimiento Fitness y Progreso

## Goal

Levantar localmente el backend .NET y el frontend React para validar el flujo principal del feature con autenticación Firebase, catálogo Wger y persistencia en Firestore.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- Cuenta/proyecto Firebase con Auth y Firestore habilitados
- Credenciales de servicio para backend Firebase Admin
- Variables de entorno para acceso a Wger

## Environment Variables

### Backend

- `FIREBASE_PROJECT_ID`
- `FIREBASE_CLIENT_EMAIL`
- `FIREBASE_PRIVATE_KEY`
- `WGER_BASE_URL`
- `WGER_API_KEY` (si aplica)
- `ASPNETCORE_ENVIRONMENT=Development`

### Frontend

- `VITE_FIREBASE_API_KEY`
- `VITE_FIREBASE_AUTH_DOMAIN`
- `VITE_FIREBASE_PROJECT_ID`
- `VITE_API_BASE_URL`

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

1. Iniciar sesión como usuario válido.
2. Consultar grupos musculares y ejercicios sugeridos desde el catálogo cacheado/proxied.
3. Crear una rutina con nombre, día y varios grupos musculares.
4. Crear una dieta por día con desayuno/comida/merienda/cena.
5. Activar seguimiento calórico y definir objetivo diario.
6. Registrar dos sesiones del mismo ejercicio con resultados diferentes.
7. Verificar la comparación contra última sesión, promedio últimas 4 y mejor marca reciente.
8. Desactivar seguimiento calórico y confirmar que la planificación de comidas sigue funcionando sin exigir calorías.
9. Simular indisponibilidad de Wger y comprobar que el catálogo cacheado sigue visible con estado degradado.

## Minimum Test Matrix

- Backend unit tests para cálculo de comparativas.
- Backend integration tests para auth y fallback de catálogo.
- Frontend component tests para formularios de rutina/dieta y panel de progreso.

## End-to-End Validation Results (2026-05-05)

Estado: PASS

Backend validado:

- `dotnet build backend/GymTracker.sln`: OK
- `dotnet test backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "WorkoutEndpointsTests|ProgressEndpointsTests"`: OK
- `dotnet test backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "CatalogEndpointsTests|RoutineEndpointsTests"`: OK
- `dotnet test backend/tests/GymTracker.Api.IntegrationTests/GymTracker.Api.IntegrationTests.csproj --filter "DietEndpointsTests|MealLogEndpointsTests|UserPreferencesEndpointsTests"`: OK

Frontend validado:

- `npm run test -- --run`: OK
- `npm run lint`: OK
- `npm run build`: OK (warning no bloqueante por chunk > 500kB)

Flujos verificados:

1. Registro de workout y consulta de progreso.
2. CRUD basico de rutinas y sugerencias por catalogo.
3. CRUD basico de dietas, registro de comidas y preferencias caloricas.
4. Consulta de historial paginado y endpoint de estado degradado de catalogo.
