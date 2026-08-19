---

description: "Task list template for feature implementation"
---

# Tasks: Registrar Entrenamiento por Bloques (Plan Mensual)

**Input**: Design documents from `/specs/014-workout-log-blocks/`
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/openapi.yaml](./contracts/openapi.yaml), [quickstart.md](./quickstart.md)

**Tests**: Se incluyen tareas de test unitario/integracion (backend) y componente/reducer (frontend) por story, segun el alcance definido en quickstart.md.

**Organization**: Tareas agrupadas por user story (US1 P1, US2 P1, US3 P2, US4 P2, US5 P2), ordenadas por capas reales: dominio → aplicacion → infraestructura → api → frontend interfaces/services → redux → componentes → integracion.

**Fuera de alcance (NO generar tareas)**: `TrainingLog` existente, modulo `routines/exercices`, comparativas de progreso, graficos (FR-022, plan.md).

## Format: `[ID] [P?] [Story] Description [skill: nombre-del-skill]`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: User story a la que pertenece (US1..US5); ausente en Setup/Foundational/Polish
- **[skill: ...]**: Skill(s) aplicable(s) segun la tabla de `spec-kit-workflow`

---

## Phase 1: Setup

**Purpose**: Confirmar estructura de carpetas antes de escribir codigo, sin introducir dependencias nuevas (no hay paquetes nuevos en esta feature).

- [ ] T001 Crear la estructura de carpetas vacia del backend prevista en plan.md: `src/backend/src/GymTracker.Domain/Entities/TrainingSession/`, `src/backend/src/GymTracker.Application/TrainingSession/`, `src/backend/src/GymTracker.Infrastructure/TrainingSession/`, `src/backend/src/GymTracker.Api/Controllers/TrainingSessions/`, `src/backend/tests/GymTracker.Application.UnitTests/TrainingSession/`, `src/backend/tests/GymTracker.Api.IntegrationTests/TrainingSessions/` [skill: backend-module-structure-crud]
- [ ] T002 [P] Crear la estructura de carpetas vacia del frontend prevista en plan.md: `src/frontend/src/interfaces/trainingSessions/`, `src/frontend/src/services/api/trainingSessions/`, `src/frontend/src/redux/{actions,reducers,states}/trainingSessions/`, y las subcarpetas de componentes `pages/routines/monthlyPlan/components/{QuickSummaryCard,SessionBlocksList,SessionBlockFormDialog,EditQuickSummaryDialog,SessionHistoryList}/` [skill: frontend-architecture]

**Checkpoint**: Carpetas listas, sin logica todavia.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Modulo backend `TrainingSessions` completo (dominio → aplicacion → infraestructura → api) mas contratos/servicio/redux base del frontend, compartido por todas las user stories. Ninguna story puede empezar hasta completar esta fase.

**⚠️ CRITICAL**: Incluye la auditoria de base de datos ANTES de la migracion (evitar el incidente de drift de `PlannedExercises` documentado en research.md Decision 10).

- [ ] T003 Auditar el estado real de la base de datos de desarrollo (tablas/columnas existentes) ejecutando `dotnet ef migrations list --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api` y comparando contra `src/backend/src/GymTracker.Infrastructure/Migrations/AdminDbContextModelSnapshot.cs`; documentar que no hay drift pendiente ajeno a esta feature antes de tocar entidades o generar la migracion [skill: backend-modular]
- [x] T004 [P] Crear enum `SessionBlockType` en `src/backend/src/GymTracker.Domain/Enum/SessionBlockType.cs` con los 8 valores fijos (`WARMUP`, `APPROACH`, `WORK`, `REST_STRENGTH`, `SWIM`, `SERIES`, `TECHNIQUE`, `REST_CARDIO`) [skill: backend-modular]
- [x] T005 [P] Crear entidad `TrainingSession` en `src/backend/src/GymTracker.Domain/Entities/TrainingSession/TrainingSession.cs` heredando `AuditableEntity`, con campos y validaciones de data-model.md (`WeekNumber` 1-4, `DayNumber` 1-7, metricas >= 0, `Notes` <= 150) y metodo `Create(...)` [skill: backend-modular]
- [x] T006 Crear entidad `SessionBlock` en `src/backend/src/GymTracker.Domain/Entities/TrainingSession/SessionBlock.cs` con FK a `TrainingSession`, validaciones (`Name` no vacio, `DurationValue` > 0, `IntensityRpe` 1-10 o null, `Description` <= 100) y metodo `Create(...)` — depende de T004 (usa `SessionBlockType`) [skill: backend-modular]
- [x] T007 Crear `ExerciseTypeProfileMapper` en `src/backend/src/GymTracker.Application/TrainingSession/ExerciseTypeProfileMapper.cs` mapeando `ExerciseType` a perfil fuerza/cardio (data-model.md, seccion ExerciseProfile) [skill: backend-modular]
- [x] T008 [P] Crear `TrainingSessionContracts.cs` en `src/backend/src/GymTracker.Application/TrainingSession/TrainingSessionContracts.cs` (`CreateTrainingSessionRequest`, `CreateSessionBlockRequest`, `TrainingSessionResponse`, `SessionBlockResponse`) reflejando exactamente `contracts/openapi.yaml` [skill: backend-modular]
- [x] T009 [P] Crear interfaz `ITrainingSessionRepository` en `src/backend/src/GymTracker.Application/TrainingSession/ITrainingSessionRepository.cs` (`AddAsync`, `GetByIdAsync(id, userId)`, `ListHistoryAsync(userId, weekNumber, dayNumber, exerciseId)`) [skill: backend-modular]
- [x] T010 Crear `TrainingSessionService` en `src/backend/src/GymTracker.Application/TrainingSession/TrainingSessionService.cs` implementando `CreateAsync` (valida perfil fuerza/cardio del `ExerciseType` contra `BlockType` de cada bloque via T007, descarta `IntensityRpe` si el tipo no lo soporta — BR-005), `GetByIdAsync` (filtrado por `UserId`, FR-023) y `ListHistoryAsync` (orden desc por `Timestamp`) — depende de T005, T006, T007, T008, T009 [skill: backend-modular]
- [x] T011 Crear `SqlTrainingSessionRepository` en `src/backend/src/GymTracker.Infrastructure/TrainingSession/SqlTrainingSessionRepository.cs` implementando `ITrainingSessionRepository` contra `AdminDbContext`, incluyendo `Include(Blocks)` — depende de T009 [skill: backend-modular]
- [x] T012 Registrar `DbSet<TrainingSession>` y `DbSet<SessionBlock>` mas `Configure<TrainingSession>`/`Configure<SessionBlock>` (FKs con `OnDelete` segun data-model.md, indices, filtro soft-delete) en `src/backend/src/GymTracker.Infrastructure/Admin/AdminDbContext.cs` — MUST NOT modificar la configuracion de ninguna otra entidad existente (research.md Decision 10) [skill: backend-modular]
- [x] T013 Generar la migracion EF Core `AddTrainingSessionsModule` (`dotnet ef migrations add AddTrainingSessionsModule --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api`) y verificar contra la auditoria de T003 que el diff generado SOLO crea `TrainingSessions`/`SessionBlocks` (tablas, FKs, indices); si aparece cualquier cambio de esquema no relacionado, revertir y corregir el mapeo de T012 antes de continuar; actualizar `AdminDbContextModelSnapshot.cs` [skill: backend-modular]
- [x] T014 Crear `TrainingSessionsController` en `src/backend/src/GymTracker.Api/Controllers/TrainingSessions/TrainingSessionsController.cs` heredando `CurrentUserControllerBase`, con `POST /api/trainingsessions`, `GET /api/trainingsessions/{id}` y `GET /api/trainingsessions/week/{weekNumber}/day/{dayNumber}/exercise/{exerciseId}/history` como thin endpoints sobre `TrainingSessionService` — depende de T010 [skill: backend-modular]
- [x] T015 Integrar los 3 endpoints de `specs/014-workout-log-blocks/contracts/openapi.yaml` implementados en T014 dentro del contrato oficial `src/backend/src/GymTracker.Api/Contracts/openapi.yaml` [skill: backend-modular, skill: spec-kit-workflow]
- [ ] T015b [US4] Integrar el endpoint `GET /api/trainingsessions/block-templates/{exerciseType}` (implementado en T046) dentro del contrato oficial `src/backend/src/GymTracker.Api/Contracts/openapi.yaml`, cumpliendo el principio API-First antes de cerrar la user story — depende de T046 [skill: backend-modular, skill: spec-kit-workflow]
- [x] T016 Aplicar la migracion contra la base de datos real de desarrollo: `dotnet ef database update --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api` y confirmar que se aplica sin errores — depende de T013 [skill: backend-modular]
- [x] T017 [P] Crear `src/frontend/src/interfaces/trainingSessions/trainingSessions.ts` con `ITrainingSession`, `ISessionBlock`, `ICreateTrainingSessionRequest`, `ICreateSessionBlockRequest` reflejando los DTOs de T008 [skill: frontend-architecture]
- [x] T018 Crear `src/frontend/src/services/api/trainingSessions/trainingSessionsApi.ts` con `createTrainingSession`, `getTrainingSessionById`, `getTrainingSessionHistory` usando `apiFetch` — depende de T017 [skill: frontend-services]
- [x] T019 Crear el esqueleto Redux del modulo: `src/frontend/src/redux/states/trainingSessions/trainingSessionsState.ts` (contrato adaptado — `loading`/`error`/`popUpCode`/`form`/`history`, research.md Decision 8), `src/frontend/src/redux/actions/trainingSessions/trainingSessionsActions.ts` (`setTrainingSessionsForm`, `setTrainingSessionsPopUpCode`, `resetTrainingSessions`, thunks `fetchTrainingSessionHistory`/`submitTrainingSession`) y `src/frontend/src/redux/reducers/trainingSessions/trainingSessionsReducer.ts` — depende de T017, T018 [skill: frontend-redux]
- [x] T020 Registrar `trainingSessionsReducer` en `src/frontend/src/redux/store.ts` — depende de T019 [skill: frontend-redux]

**Checkpoint**: Backend y base de Redux/servicios listos y compilando. Las user stories pueden empezar.

---

## Phase 3: User Story 1 - Registrar una sesión de fuerza con resumen y bloques (Priority: P1) 🎯 MVP

**Goal**: Sustituir el tab "Registro" por "Registrar" con resumen rapido de perfil fuerza, alta/edicion/borrado de bloques y guardado como `TrainingSession`, eliminando por completo el codigo obsoleto de captura por set.

**Independent Test**: Entrar a un ejercicio de fuerza planificado, registrar resumen + al menos un bloque + notas, pulsar "Guardar entrenamiento" y verificar que la sesion aparece en el Historial.

### Limpieza de codigo obsoleto (mismo grupo que el nuevo tab, no aparte)

- [x] T021 [US1] Eliminar de `src/frontend/src/pages/routines/monthlyPlan/components/ExerciseDetail.tsx` las interfaces `UnitDraftValue`/`ExerciseSet`, la constante `TIME_UNIT_PATTERN`, las funciones `isTimeUnit`/`createEmptyDraft`/`handleDraftChange`/`handleAddSet`/`handleDeleteSet`, el estado `draft`/`sets`, y los imports que queden sin uso (`TextField`, `IconButton`, `DeleteIcon`, `AddIcon`) — sin dejar codigo muerto ni comentado (FR-019). Los botones fijos inferiores existentes ("Guardar cambios" outlined y "Completar ejercicio" contained ligado a `onStartTraining`) se ELIMINAN en este paso: quedan sustituidos por los botones fijos de FR-013 que se introducen en T028 ("Guardar entrenamiento" + acceso circular a Historial); `onStartTraining` deja de invocarse desde este componente — confirmar en `MonthlyPlanPage.tsx` si la prop `onStartTraining` de `ExerciseDetail` queda sin uso y eliminarla tambien si aplica [skill: frontend-components]

### Implementacion de User Story 1

- [x] T022 [US1] Renombrar el tab "Registro" a "Registrar" (mantener "Historial") en `ExerciseDetail.tsx` y anadir la clave i18n `monthlyPlan.record` = "Registrar" en `src/frontend/src/i18n/translates/es.ts` y `en.ts` (FR-002) — depende de T021 [skill: frontend-architecture, skill: frontend-components]
- [x] T023 [P] [US1] Crear utilidad `src/frontend/src/utils/exerciseProfile.ts` mapeando `ExerciseType` a perfil fuerza/cardio, equivalente al `ExerciseTypeProfileMapper` del backend (T007) [skill: frontend-architecture]
- [x] T024 [P] [US1] Crear componente `QuickSummaryCard` en `pages/routines/monthlyPlan/components/QuickSummaryCard/QuickSummaryCard.tsx` renderizando las 3 metricas del perfil fuerza (Duracion en min, Volumen total en kg, Ejercicios/bloques) via props tipadas — depende de T023 [skill: frontend-components]
- [x] T025 [P] [US1] Crear componente `SessionBlockFormDialog` en `.../SessionBlockFormDialog/SessionBlockFormDialog.tsx` usando `PopupDialog`, con grid de tipos de bloque de perfil fuerza (Calentamiento/Aproximacion/Trabajo/Descanso), campos Nombre, Duracion, Descripcion (contador 100 caracteres) y validacion FR-009 (bloquear confirmar sin tipo/nombre/duracion > 0) [skill: frontend-components]
- [x] T026 [US1] Crear componente `SessionBlocksList` en `.../SessionBlocksList/SessionBlocksList.tsx` renderizando la lista ordenada de bloques (icono/color por tipo, duracion, nombre, descripcion), clicable para editar (abre `SessionBlockFormDialog` precargado, FR-010) y con accion de eliminar (FR-011) — depende de T025 [skill: frontend-components]
- [x] T027 [US1] Crear version minima de `SessionHistoryList` en `.../SessionHistoryList/SessionHistoryList.tsx` que renderice las sesiones devueltas por `getTrainingSessionHistory`, suficiente para verificar que una sesion guardada aparece (el pulido de estado vacio/orden se completa en US5) [skill: frontend-components]
- [x] T028 [US1] Reescribir el contenido del tab "Registrar" en `ExerciseDetail.tsx`: montar `QuickSummaryCard`, boton "+ Añadir bloque" que abre `SessionBlockFormDialog` via `popUpCode`, `SessionBlocksList`, seccion "Notas (opcional)" (textarea con contador 150 caracteres, FR-012), la seccion estatica "Autogestion eficiente" (ver T028b) y los botones fijos inferiores "Guardar entrenamiento" + acceso rapido circular a Historial (FR-013), sustituyendo por completo a los botones "Guardar cambios"/"Completar ejercicio" eliminados en T021 — depende de T021, T022, T024, T025, T026, T027 [skill: frontend-components, skill: frontend-redux]
- [x] T028b [US1] Añadir la seccion informativa estatica "Autogestion eficiente" en `ExerciseDetail.tsx` (icono + lista corta de 3-4 consejos) con todo su contenido (titulo y consejos) obtenido de claves i18n nuevas en `es.ts`/`en.ts`, sin logica de negocio (FR-020) — depende de T021 [skill: frontend-components, skill: frontend-architecture]
- [x] T029 [US1] Conectar "Guardar entrenamiento" para despachar el thunk `submitTrainingSession` (persistiendo resumen + bloques + notas, FR-014/FR-015) y, al completarse, refrescar `fetchTrainingSessionHistory`; en caso de fallo de red mantener los datos introducidos visibles para reintentar (SC-005) — depende de T019, T028 [skill: frontend-redux]
- [x] T030 [P] [US1] Anadir claves i18n del perfil fuerza (etiquetas de metricas, tipos de bloque de fuerza, "Notas (opcional)", "Guardar entrenamiento") en `es.ts`/`en.ts` (FR-018) [skill: frontend-architecture]

### Tests de User Story 1

- [ ] T031 [P] [US1] Tests de dominio backend: validaciones de `TrainingSession`/`SessionBlock` (rangos `WeekNumber`/`DayNumber`, duraciones >= 0/> 0, limites de `Notes`/`Description`) en `src/backend/tests/GymTracker.Application.UnitTests/TrainingSession/` [skill: backend-modular]
- [ ] T032 [P] [US1] Test de integracion backend: `POST /api/trainingsessions` (perfil fuerza) devuelve `201` y `GET /api/trainingsessions/{id}` devuelve `200` al propietario / `404` a otro usuario (FR-023) en `src/backend/tests/GymTracker.Api.IntegrationTests/TrainingSessions/` [skill: backend-modular]
- [ ] T033 [P] [US1] Tests de componente/reducer frontend: alta/edicion/borrado de bloque con validacion FR-009, guardado exitoso y aparicion en `SessionHistoryList`, reducer de `submitTrainingSession`/`fetchTrainingSessionHistory` [skill: frontend-components, skill: frontend-redux]

**Checkpoint**: User Story 1 funcional y comprobable de forma independiente (MVP).

---

## Phase 4: User Story 2 - Registrar una sesión de cardio/nado con ritmo objetivo (Priority: P1)

**Goal**: Adaptar resumen y estructura de bloques al perfil cardio, incluyendo el campo condicional de Intensidad (RPE).

**Independent Test**: Entrar a un ejercicio cardio planificado, comprobar que metricas y tipos de bloque son distintos a los de fuerza, y guardar la sesion correctamente.

- [x] T034 [P] [US2] Extender `QuickSummaryCard` para renderizar las metricas del perfil cardio (Duracion, Calorias aproximadas, Ritmo objetivo RPE 1-10 con etiqueta de intensidad) segun el perfil resuelto por `exerciseProfile.ts` — depende de T024 [skill: frontend-components]
- [x] T035 [US2] Extender `SessionBlockFormDialog` con los tipos de bloque de perfil cardio (Nado/Series/Tecnica/Descanso) y mostrar el campo "Intensidad (RPE)" solo para los tipos que lo soportan (FR-008), descartando el valor si el usuario cambia a un tipo sin soporte (edge case del spec) — depende de T025 [skill: frontend-components]
- [x] T036 [P] [US2] Anadir claves i18n del perfil cardio (Calorias, Ritmo objetivo, etiquetas RPE Muy suave/Comodo/Moderado/Alto/Maximo, tipos de bloque cardio) en `es.ts`/`en.ts` [skill: frontend-architecture]
- [ ] T037 [P] [US2] **BLOQUEADO (preexistente, fuera de alcance)**: Tests unitarios backend: `TrainingSessionService` rechaza `BlockType` de perfil incorrecto (cardio en sesion de fuerza y viceversa) y descarta `IntensityRpe` cuando el tipo no lo soporta (BR-005) [skill: backend-modular] — la logica YA esta implementada en `TrainingSessionService`/`SessionBlock.NormalizeIntensityRpe`; el test no puede verificarse con `dotnet test` porque `GymTracker.Application.UnitTests` no compila (~60 errores preexistentes no relacionados, ver memoria de repo)
- [ ] T038 [P] [US2] **BLOQUEADO (preexistente, fuera de alcance)**: Test de integracion backend: `POST /api/trainingsessions` (perfil cardio) con bloques que incluyen `intensityRpe` devuelve `201` y persiste el valor solo en los tipos que lo soportan [skill: backend-modular] — no verificable porque `GymTracker.Api.IntegrationTests` no compila (~44 errores preexistentes no relacionados)
- [ ] T039 [P] [US2] Test de componente: render de metricas de perfil cardio, visibilidad condicional del campo RPE y descarte de RPE al cambiar de tipo de bloque en `SessionBlockFormDialog` [skill: frontend-components]

**Checkpoint**: User Stories 1 y 2 funcionales de forma independiente.

---

## Phase 5: User Story 3 - Editar el objetivo del resumen rápido (Priority: P2)

**Goal**: Permitir editar los 3 valores del resumen rapido de forma local, sin llamada a guardado de sesion completa.

**Independent Test**: Pulsar "Editar objetivo", modificar los 3 valores, pulsar "Guardar cambios" y comprobar que `QuickSummaryCard` refleja los nuevos valores sin request de red.

- [x] T040 [US3] ~~Anadir la accion local `setTrainingSessionsSummaryDraft`~~ **Decision de implementacion**: `setTrainingSessionsForm` (T019) ya es una accion sincrona local (no-thunk), por lo que reutilizarla para el patch de resumen cumple el requisito (sin llamada de red) sin duplicar logica de estado — depende de T019 [skill: frontend-redux]
- [x] T041 [US3] Crear componente `EditQuickSummaryDialog` en `.../EditQuickSummaryDialog/EditQuickSummaryDialog.tsx` (basado en `PopupDialog`) con los 3 campos editables + su unidad, y boton "Guardar cambios" que solo despacha `setTrainingSessionsSummaryDraft` — depende de T040 [skill: frontend-components]
- [x] T042 [US3] **Decision de implementacion**: en vez de `popUpCode` compartido, `EditQuickSummaryDialog` y `SessionBlockFormDialog` usan cada uno su propio estado local booleano (`summaryDialogOpen`/`blockDialogOpen`) en `ExerciseDetail.tsx`, lo que garantiza independencia total entre ambos (FR-025) sin acoplarlos a un unico enum de popup compartido. Conectado el boton "Editar objetivo" para abrir el dialogo y reflejar los valores inmediatamente en la tarjeta via `onFormChange` (FR-004) — depende de T028, T041 [skill: frontend-components, skill: frontend-redux]
- [x] T043 [P] [US3] Anadir claves i18n para "Editar objetivo"/"Guardar cambios" en `es.ts`/`en.ts` [skill: frontend-architecture]
- [ ] T044 [P] [US3] Test de reducer/componente: editar el resumen actualiza el estado local y la tarjeta sin disparar ningun thunk/llamada de red [skill: frontend-redux, skill: frontend-components]

**Checkpoint**: User Stories 1-3 funcionales de forma independiente.

---

## Phase 6: User Story 4 - Cargar una plantilla de bloques predefinida (Priority: P2)

**Goal**: Permitir precargar una estructura de bloques por defecto segun el perfil, con confirmacion si ya existen bloques.

**Independent Test**: En un ejercicio sin bloques, pulsar "Ver plantillas" y verificar que se carga la estructura por defecto del perfil correspondiente; con bloques existentes, verificar que se pide confirmacion.

- [x] T045 [US4] Crear `BlockTemplateCatalog` en `src/backend/src/GymTracker.Application/TrainingSession/BlockTemplateCatalog.cs` con contenido estatico en memoria, al menos una plantilla por perfil (fuerza/cardio), sin tabla ni migracion (research.md Decision 4, INT-004) [skill: backend-modular]
- [x] T046 [US4] Anadir la accion `GET /api/trainingsessions/block-templates/{exerciseType}` a `TrainingSessionsController` devolviendo la plantilla del perfil correspondiente desde `BlockTemplateCatalog` — depende de T014, T045 [skill: backend-modular]
- [x] T047 [US4] **Decision de implementacion**: se anadio `getBlockTemplates` a `trainingSessionsApi.ts` y el thunk `fetchBlockTemplates`, pero SIN accion `applyBlockTemplate` separada: `ExerciseDetail.tsx` recibe un prop `onLoadBlockTemplate` (implementado por `MonthlyPlanPage.tsx` via `dispatch(fetchBlockTemplates(...)).unwrap()`) y aplica el resultado mapeado al formulario mediante el `onFormChange` ya existente, evitando duplicar logica de estado — depende de T018, T019, T046 [skill: frontend-services, skill: frontend-redux]
- [x] T048 [US4] Conectar el boton "Ver plantillas" en `ExerciseDetail.tsx` para cargar y aplicar la plantilla, mostrando un dialogo de confirmacion (reutilizando `PopupDialog`) cuando ya existen bloques antes de reemplazarlos (FR-006 edge case) — depende de T028, T047 [skill: frontend-components, skill: frontend-redux]
- [x] T049 [P] [US4] Anadir claves i18n para "Ver plantillas" y su dialogo de confirmacion en `es.ts`/`en.ts` [skill: frontend-architecture]
- [ ] T050 [P] [US4] **BLOQUEADO (preexistente, fuera de alcance)**: Test unitario backend: `BlockTemplateCatalog` devuelve una plantilla no vacia y deterministica para los 8 valores de `ExerciseType` [skill: backend-modular] — no verificable con `dotnet test` (ver nota en T037/T038)
- [ ] T051 [P] [US4] **BLOQUEADO (preexistente, fuera de alcance)**: Test de integracion backend: `GET /api/trainingsessions/block-templates/{exerciseType}` devuelve la plantilla del perfil correcto para cada `ExerciseType` [skill: backend-modular]
- [ ] T052 [P] [US4] Test de componente: cargar plantilla en ejercicio sin bloques rellena la estructura; con bloques existentes pide confirmacion antes de reemplazar [skill: frontend-components]

**Checkpoint**: User Stories 1-4 funcionales de forma independiente.

---

## Phase 7: User Story 5 - Consultar el historial de sesiones de un ejercicio planificado (Priority: P2)

**Goal**: Completar el tab "Historial" con estado vacio y orden correcto para todas las sesiones guardadas del ejercicio planificado.

**Independent Test**: Guardar dos sesiones en momentos distintos para el mismo ejercicio planificado y comprobar que ambas aparecen en "Historial", ordenadas por fecha, cada una con resumen y bloques.

- [x] T053 [US5] Finalizar `SessionHistoryList` (creado en T027): estado vacio (FR-017), orden mas reciente primero, cada entrada mostrando sus 3 metricas de resumen y sus bloques (FR-016) — depende de T027 [skill: frontend-components]
- [x] T054 [US5] Conectar el acceso rapido circular a Historial (FR-013, hecho en T028: navega a la tab 0) y el tab "Historial" en `ExerciseDetail.tsx`; `fetchTrainingSessionHistory` ya se dispara al seleccionar el ejercicio y tras cada guardado exitoso (`submitTrainingSession.fulfilled` → refresca historial), cubriendo el requisito sin refetch redundante en cada cambio de tab — depende de T028, T053 [skill: frontend-redux, skill: frontend-components]
- [x] T055 [P] [US5] Anadir claves i18n para el estado vacio del Historial en `es.ts`/`en.ts` [skill: frontend-architecture]
- [ ] T056 [P] [US5] **BLOQUEADO (preexistente, fuera de alcance)**: Test de integracion backend: `GET .../history` devuelve `[]` sin sesiones, y una lista ordenada desc por `Timestamp` con varias sesiones del mismo ejercicio planificado guardadas en momentos distintos (FR-015) [skill: backend-modular]
- [ ] T057 [P] [US5] Test de componente: `SessionHistoryList` renderiza estado vacio y multiples sesiones en el orden correcto [skill: frontend-components]

**Checkpoint**: Las 5 user stories funcionales de forma independiente.

---

## Phase 8: Polish & Verificación Final

**Purpose**: Verificacion cruzada de calidad antes de PR — sin funcionalidad nueva.

- [x] T058 Ejecutar `dotnet build GymTracker.sln` en `src/backend` y confirmar 0 errores y 0 warnings nuevos relacionados con el modulo `TrainingSession` [skill: backend-modular]
- [x] T059 Ejecutar `npx tsc --noEmit` en `src/frontend` y confirmar 0 errores en los archivos tocados (17 errores preexistentes no relacionados se mantienen sin cambios) [skill: frontend-components]
- [x] T060 Ejecutar `npm run lint` en `src/frontend` y confirmar 0 avisos de imports/variables sin usar en los archivos tocados por esta feature (se encontro y corrigio 1 variable `exerciseLoading` sin uso en `MonthlyPlanPage.tsx`) [skill: frontend-components]
- [x] T061 Volver a ejecutar `dotnet ef database update --project src/GymTracker.Infrastructure --startup-project src/GymTracker.Api` contra la base de datos real de desarrollo tras completar todas las stories: confirmado "No migrations were applied. The database is already up to date." (sin cambios de modelo pendientes) [skill: backend-modular]
- [ ] T062 Ejecutar la validacion manual completa de [quickstart.md](./quickstart.md) (US1-US5, edge cases, aislamiento por usuario) y registrar el resultado [skill: spec-kit-workflow]
- [ ] T063 Revisar el checklist de PR — Cumplimiento constitucional de `spec-kit-workflow` (OpenAPI actualizado, tests en verde, i18n completo, sin codigo muerto) antes de solicitar revision [skill: spec-kit-workflow]

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias — puede empezar de inmediato.
- **Foundational (Phase 2)**: depende de Setup — BLOQUEA todas las user stories.
- **User Story 1 (Phase 3)**: depende de Foundational. Es el MVP.
- **User Story 2 (Phase 4)**: depende de Foundational; extiende componentes creados en US1 (`QuickSummaryCard`, `SessionBlockFormDialog`) pero es verificable de forma independiente con su propio perfil de ejercicio.
- **User Story 3 (Phase 5)**: depende de Foundational y de `ExerciseDetail.tsx`/`QuickSummaryCard` (T028) de US1.
- **User Story 4 (Phase 6)**: depende de Foundational y de `ExerciseDetail.tsx` (T028) de US1.
- **User Story 5 (Phase 7)**: depende de Foundational y de `SessionHistoryList` (T027)/`ExerciseDetail.tsx` (T028) de US1.
- **Polish (Phase 8)**: depende de que todas las stories deseadas esten completas.

### User Story Dependencies

- US1 (P1): no depende de otras stories — es la base sobre la que US2-US5 extienden componentes ya creados.
- US2 (P1): extiende `QuickSummaryCard`/`SessionBlockFormDialog` de US1, pero es independientemente testeable con un ejercicio cardio.
- US3 (P2): extiende `QuickSummaryCard`/`ExerciseDetail.tsx` de US1 con un dialogo nuevo (`EditQuickSummaryDialog`).
- US4 (P2): extiende `ExerciseDetail.tsx` de US1 con un endpoint y flujo nuevos (plantillas).
- US5 (P2): finaliza `SessionHistoryList` creado en US1.

### Parallel Opportunities

- T001/T002 (Setup) en paralelo.
- T004, T005, T008, T009, T017 (Foundational, archivos distintos sin dependencias entre si) en paralelo.
- Dentro de US1: T023, T024, T025 en paralelo entre si (archivos distintos); T030-T033 (i18n y tests) en paralelo entre si.
- Dentro de US2: T034, T036, T037, T038, T039 en paralelo entre si.
- Dentro de US3/US4/US5: las tareas de i18n y tests marcadas [P] pueden ejecutarse en paralelo entre si.

---

## Parallel Example: User Story 1

```bash
# Componentes independientes de US1:
Task: "Crear utilidad exerciseProfile.ts en utils/"
Task: "Crear QuickSummaryCard (perfil fuerza)"
Task: "Crear SessionBlockFormDialog (perfil fuerza)"

# Tests de US1 tras completar implementacion:
Task: "Tests de dominio backend TrainingSession/SessionBlock"
Task: "Test de integracion POST /api/trainingsessions (fuerza)"
Task: "Tests de componente/reducer frontend flujo fuerza"
```

---

## Implementation Strategy

### MVP First (User Story 1 unicamente)

1. Completar Phase 1: Setup
2. Completar Phase 2: Foundational (CRITICO — bloquea todas las stories)
3. Completar Phase 3: User Story 1
4. **STOP y VALIDAR**: probar User Story 1 de forma independiente (ejercicio de fuerza, guardar, ver en Historial)
5. Entregar/demo si esta listo

### Incremental Delivery

1. Setup + Foundational → base lista
2. + US1 → validar de forma independiente → Demo (MVP)
3. + US2 → validar de forma independiente → Demo
4. + US3 → validar de forma independiente → Demo
5. + US4 → validar de forma independiente → Demo
6. + US5 → validar de forma independiente → Demo
7. Polish (Phase 8) → verificacion final antes de PR

---

## Notes

- [P] = archivos distintos, sin dependencias pendientes.
- [Story] mapea la tarea a su user story para trazabilidad.
- La limpieza de codigo obsoleto de `ExerciseDetail.tsx` (T021) MUST ejecutarse dentro de US1, junto con la introduccion del nuevo tab "Registrar" — no al final ni como tarea opcional.
- La auditoria de base de datos (T003) MUST completarse antes de generar la migracion (T013), para no repetir el incidente de drift de `PlannedExercises`.
- Verificar que cada test falla antes de implementar, cuando aplique TDD.
- Detenerse en cada checkpoint para validar la story de forma independiente.
- Evitar: tareas vagas, conflictos de archivo simultaneo, dependencias cruzadas entre stories que rompan la independencia.
