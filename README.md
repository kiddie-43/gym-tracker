# Gym Tracker

Aplicacion full-stack para seguimiento de entrenamientos, progreso, rutinas y nutricion.

## Stack

- Backend: ASP.NET Core 8, arquitectura por capas (Api/Application/Domain/Infrastructure)
- Frontend: React 19 + TypeScript + Vite + MUI
- Persistencia local de desarrollo: repositorios in-memory con forma de Firestore
- Integraciones: Firebase Auth y almacenamiento de datos de usuario; Wger API para catalogo externo

## Estructura Principal

- `backend/`: solucion .NET y tests
- `frontend/`: SPA React
- `specs/001-track-fitness-progress/`: especificacion, tareas y contratos
- `firebase/firestore.rules`: reglas de seguridad propuestas para datos de usuario en Firestore

## Ejecucion Rapida

### Frontend + Backend (un solo comando)

1. `npm install`
2. `npm run install:all`
3. `npm run dev`

Esto levanta en paralelo:

- Backend con hot reload (`dotnet watch`) en `http://localhost:5092`
- Frontend con Vite en `http://localhost:5173`

### Backend

1. `cd backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`

### Frontend

1. `cd frontend`
2. `npm install`
3. `npm run dev`

## Estado de Implementacion

- US1 completada: registro de entrenamientos y comparativas de progreso.
- US2 completada: rutinas, sugerencias por grupos musculares y UI de builder.
- US3 completada: dietas, logs de comidas, preferencias caloricas y UI asociada.
- US4 completada: historial paginado y estado degradado de catalogo.

## Notas de Rendimiento

- Ver recomendaciones de indices en `backend/src/GymTracker.Infrastructure/Firebase/FirestoreIndexes.md`.
- El build frontend puede advertir chunk > 500kB; no es bloqueante en desarrollo.

## Separacion De Responsabilidades

- Firebase se usa para autenticar usuarios y almacenar sus datos propios.
- Wger es una integracion externa independiente para catalogo, ejercicios y alimentos.
- Las claves de `Firebase:*` y `Wger:*` se mantienen separadas en configuracion y User Secrets.
