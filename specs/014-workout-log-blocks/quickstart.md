# Quickstart: Registrar Entrenamiento por Bloques (014)

## Goal

Validar end-to-end el nuevo tab "Registrar" del Plan Mensual: resumen rapido por perfil de `ExerciseType`, estructura de bloques (crear/editar/eliminar), plantillas por defecto, notas, guardado como `TrainingSession`, e historial de sesiones por ejercicio planificado.

## Prerequisites

- .NET 8 SDK
- Node.js 22+
- SQL Server local/dev configurado (mismo que usa el resto del proyecto)
- Base de datos de desarrollo con al menos un `MonthlyPlan` + `PlannedExercise` existentes para el usuario de prueba (id fijo de `CurrentUserControllerBase`)

## Run Backend

1. `cd src/backend`
2. `dotnet restore`
3. `dotnet build GymTracker.sln`
4. Revisar el estado real de la base de datos antes de migrar (evitar arrastrar drift no relacionado, ver research.md Decision 10):
   - `dotnet ef migrations list --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api`
5. `dotnet ef database update --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api`
6. `dotnet run --project src/GymTracker.Api/GymTracker.Api.csproj`

## Run Frontend

1. `cd src/frontend`
2. `npm install`
3. `npm run dev`

## Manual Validation

1. **Perfil fuerza (US1)**:
   - Entrar a un ejercicio planificado de tipo `STRENGTH`/`BODYWEIGHT`/`PLYOMETRIC`/`REHABILITATION`.
   - Verificar tab renombrado a "Registrar" (no "Registro").
   - Verificar tarjeta "Resumen rapido" con Duracion (min), Volumen total (kg), Ejercicios/bloques.
   - Pulsar "+ Añadir bloque", elegir tipo fuerza (Calentamiento/Aproximacion/Trabajo/Descanso), completar nombre + duracion, confirmar.
   - Verificar que el campo "Intensidad (RPE)" NO aparece para tipos de bloque de perfil fuerza.
   - Guardar entrenamiento y verificar que aparece en "Historial".
2. **Perfil cardio (US2)**:
   - Repetir con un ejercicio `CARDIO`/`MOBILITY`/`STRETCHING`/`SPORTS`.
   - Verificar metricas de resumen: Duracion, Calorias, Ritmo objetivo (RPE con etiqueta).
   - Elegir un tipo de bloque cardio que soporte intensidad (p. ej. Series) y verificar que el campo RPE aparece; elegir Descanso y verificar que no aparece.
3. **Editar objetivo del resumen (US3)**:
   - Pulsar "Editar objetivo", cambiar los 3 valores, pulsar "Guardar cambios".
   - Verificar que la tarjeta de resumen refleja los valores nuevos sin llamada a guardar sesion completa (validar en Network que no hay request hasta pulsar "Guardar entrenamiento").
4. **Plantillas (US4)**:
   - En un ejercicio sin bloques, pulsar "Ver plantillas", elegir una plantilla de fuerza o cardio segun corresponda, verificar que se rellena la estructura.
   - Con bloques ya creados, pulsar "Ver plantillas" de nuevo y verificar que pide confirmacion antes de reemplazar.
5. **Historial (US5)**:
   - Guardar dos sesiones distintas para el mismo ejercicio planificado (dos guardados independientes).
   - Abrir tab "Historial" y verificar orden mas reciente primero, cada entrada con sus 3 metricas y sus bloques.
   - Verificar estado vacio en un ejercicio planificado sin sesiones guardadas.
6. **Validaciones y edge cases**:
   - Intentar confirmar un bloque sin nombre o sin duracion: debe bloquear la confirmacion.
   - Escribir una descripcion de bloque/nota que exceda 100/150 caracteres: el input debe truncar y el contador nunca ser negativo.
   - Guardar sin ningun bloque: debe permitir guardar (resumen + notas suficientes).
   - Cambiar el tipo de un bloque con RPE informado a un tipo que no lo soporta: el valor de RPE se descarta.
   - Simular fallo de red al guardar: los datos introducidos deben permanecer visibles para reintentar.
7. **Aislamiento por usuario**:
   - Verificar que `GET /api/trainingsessions/{id}` de una sesion de otro usuario responde `404`/`403` segun corresponda (FR-023).
8. **Limpieza de codigo obsoleto**:
   - Confirmar en `ExerciseDetail.tsx` que no quedan `UnitDraftValue`, `ExerciseSet`, `isTimeUnit`, `createEmptyDraft`, `handleDraftChange`, `handleAddSet`, `handleDeleteSet`, ni imports sin usar (`TextField`, `IconButton`, `DeleteIcon`, `AddIcon` si ya no se usan).
   - Ejecutar `npm run lint` y `npx tsc --noEmit` en `src/frontend` y verificar 0 errores/avisos de imports o variables sin usar.

## Suggested Test Scope

- Backend unit (`GymTracker.Application.UnitTests`):
  - Validaciones de dominio de `TrainingSession`/`SessionBlock` (rangos `WeekNumber`/`DayNumber`, duraciones >= 0/> 0, notas/descripcion <= limites).
  - `TrainingSessionService`: perfil fuerza/cardio determina tipos de bloque validos; descarte de `IntensityRpe` cuando el tipo no lo soporta; guardado sin bloques permitido.
  - `BlockTemplateCatalog`: cada perfil tiene al menos una plantilla; contenido determinista.
- Backend integration (`GymTracker.Api.IntegrationTests`):
  - `POST /api/trainingsessions` crea sesion + bloques y devuelve `201`.
  - `GET /api/trainingsessions/week/{w}/day/{d}/exercise/{id}/history` devuelve `[]` sin datos y lista ordenada por `Timestamp` desc con datos.
  - Acceso a sesion de otro usuario no autorizado.
  - `GET /api/trainingsessions/block-templates/{exerciseType}` devuelve plantilla del perfil correcto para cada uno de los 8 valores de `ExerciseType`.
- Frontend:
  - Reducer/actions del modulo `trainingSessions` (fetch history, submit draft, popUpCode de bloque/resumen/plantillas).
  - `ExerciseDetail.tsx`: render condicional de metricas por perfil, alta/edicion/borrado de bloque via `PopupDialog`, validacion de formulario de bloque, contador de caracteres de notas/descripcion.
  - Verificacion estatica (lint/tsc) de que no queda codigo muerto del modelo anterior de sets.
