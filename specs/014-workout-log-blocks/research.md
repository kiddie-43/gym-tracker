# Research: Registrar Entrenamiento por Bloques (Plan Mensual)

**Feature**: 014-workout-log-blocks
**Date**: 2026-08-07
**Status**: Complete

## Decision 1: Modulo backend nuevo siguiendo el precedente `TrainingLog`, no el CRUD generico de catalogo

- Decision: `TrainingSessions` se implementa como modulo transaccional propiedad del usuario (entidad, contratos, servicio, repositorio, controlador thin) siguiendo exactamente el mismo patron ya usado por `TrainingLog`/`TrainingLogsController` (herencia `CurrentUserControllerBase`, filtrado por `UserId`, sin listado paginado generico tipo admin).
- Rationale: El skill `backend-modular` define un CRUD estandar pensado para catalogos administrables (`GET /modulo` paginado). `TrainingSession` no es un catalogo; es un log transaccional por usuario, exactamente como `TrainingLog`. Seguir el precedente real del repo es mas consistente que aplicar la tabla CRUD generica a un dominio que no encaja.
- Alternatives considered:
  - CRUD completo con `GET /trainingsessions` paginado tipo admin: descartado, no hay ningun requisito de listar todas las sesiones de todos los ejercicios; el unico listado pedido (FR-016/017) es "historial de un ejercicio planificado concreto", igual que `GET .../week/{w}/day/{d}/exercise/{code}/logs` de `TrainingLog`.

## Decision 2: Relacion con `PlannedExercise` via tripleta `WeekNumber`/`DayNumber`/`ExerciseId`, no via FK a `PlannedExercise.Id`

- Decision: `TrainingSession` almacena `WeekNumber` (1-4), `DayNumber` (1-7) y `ExerciseId` (Guid, FK a `Exercices`), replicando el patron ya validado en `TrainingLog` y `PlannedExercise`, en vez de referenciar `PlannedExercise.Id` directamente.
- Rationale: El mismo ejercicio planificado se repite en distintos meses reales (FR-015); el `PlannedExercise` es un registro de la configuracion del plan, no de cada repeticion mensual. Igual que `TrainingLog` ya lo resuelve, la tripleta identifica "que ejercicio, que semana/dia del plan", y el `Timestamp` distingue cada repeticion en el tiempo.
- Alternatives considered:
  - FK a `PlannedExercise.Id`: descartada porque si el usuario reconfigura el dia (desvincula/vincula otro ejercicio), el `PlannedExercise.Id` original podria dejar de existir o cambiar de semantica, rompiendo el historial.

## Decision 3: Resumen rapido como 3 columnas tipadas en `TrainingSession`, no una tabla generica de metricas

- Decision: El resumen rapido se modela como 3 campos fijos y explicitos en `TrainingSession` (`DurationMinutes`, `SecondaryMetricValue` + `SecondaryMetricUnitCode`, `TertiaryMetricValue`), en vez de una tabla `SessionMetric` generica de pares clave/valor.
- Rationale: El spec (FR-003) fija exactamente 3 metricas por perfil, con significado cerrado y conocido (Duracion siempre; Volumen(kg) o Calorias(kcal); num. bloques o Ritmo objetivo RPE). No hay ningun requisito de metricas configurables o extensibles. Crear una entidad generica para 3 slots fijos es sobre-ingenieria.
- Alternatives considered:
  - Tabla `SessionSummaryMetric` (N filas por sesion): descartada por complejidad injustificada frente a 3 columnas fijas; tambien complica la validacion de "exactamente 3, una por slot".
  - Calcular `TertiaryMetricValue` (num. bloques) siempre como `Blocks.Count`: descartada porque FR-004 permite editar los 3 valores del resumen de forma independiente a los bloques ("Editar objetivo"), es decir, es un valor objetivo/planificado, no necesariamente igual al recuento real de bloques.

## Decision 4: `BlockTemplate` es contenido estatico en Application, sin tabla ni migracion

- Decision: Las plantillas de bloques por perfil (fuerza/cardio) se implementan como un catalogo estatico en `GymTracker.Application.TrainingSession.BlockTemplateCatalog` (listas en memoria), expuesto solo por un endpoint de lectura. No hay entidad de dominio, `DbSet` ni migracion para `BlockTemplate`.
- Rationale: El spec es explicito (INT-004, Key Entities): "contenido estatico de referencia... no editable por el usuario en esta iteracion... sin identificadores externos". Crear una tabla administrable para esto seria anticipar un requisito (panel de admin de plantillas) que el spec descarta explicitamente para esta iteracion.
- Alternatives considered:
  - Tabla `BlockTemplates` editable vía admin: descartada explicitamente por el spec ("no es una tabla editable por admin en esta iteracion").
  - Hardcodear las plantillas solo en frontend: descartada porque duplicaria la fuente de verdad del perfil fuerza/cardio y dificultaria mantener sincronizados backend (validacion de tipos de bloque) y frontend (contenido de la plantilla).

## Decision 5: Sin endpoint de edicion de resumen antes de guardar la sesion

- Decision: La accion "Editar objetivo" (FR-004) es un cambio de estado local en el borrador Redux del frontend (`form.summary`); no dispara ninguna llamada API. Solo `POST /api/trainingsessions` persiste resumen + bloques + notas juntos, al pulsar "Guardar entrenamiento".
- Rationale: El spec describe la edicion del resumen como algo que ocurre "antes o durante" el registro, sobre datos que todavia no se han guardado como sesion. No existe ningun requisito de editar una sesion ya guardada (el historial es de solo lectura, US5). Añadir un endpoint de PATCH/PUT para un recurso que aun no existe en base de datos seria sobre-ingenieria.
- Alternatives considered:
  - `PUT /api/trainingsessions/{id}` generico reutilizado tambien para el borrador: descartado porque el borrador no tiene `id` hasta que se guarda; forzar un id temporal complicaria el contrato sin necesidad.

## Decision 6: Unidades de las metricas via `Units.Code` (string), sin FK estricta

- Decision: `SecondaryMetricUnitCode` (resumen) y `DurationUnitCode` (bloques) se almacenan como `string` (codigo de `Units`), igual que `TrainingLog.UnitCode`, sin foreign key hacia la tabla `Units`.
- Rationale: Sigue el precedente exacto ya usado por `TrainingLog` (que tampoco valida `UnitCode` contra `Units` a nivel de FK); el catalogo `Units` es administrado via importacion CSV por el admin y sus codigos no estan fijados en el codigo fuente, por lo que una FK estricta introduciria un acoplamiento fragil a datos de entorno.
- Alternatives considered:
  - FK real a `Units.Id`: descartada por inconsistencia con el precedente `TrainingLog` y por riesgo de romper el guardado si el entorno no tiene esos codigos de unidad cargados.

## Decision 7: Tipos de bloque como enum de dominio cerrado, no como catalogo administrable

- Decision: `SessionBlockType` es un `enum` en `GymTracker.Domain.Enum` con los 8 valores fijos del spec (`WARMUP`, `APPROACH`, `WORK`, `REST_STRENGTH`, `SWIM`, `SERIES`, `TECHNIQUE`, `REST_CARDIO`), validado en el `Create`/`Update` de `SessionBlock` segun el perfil del `ExerciseType` de la sesion.
- Rationale: El spec fija una lista cerrada y pequeña de tipos por perfil (FR-008); es analogo al enum `ExerciseType` ya existente. No hay ningun requisito de que el usuario o el admin puedan crear tipos de bloque nuevos.
- Alternatives considered:
  - Tabla `BlockTypes` administrable: descartada, mismo argumento que Decision 4 (no pedido, evitar sobre-ingenieria).

## Decision 8: Frontend — adaptacion del contrato de estado Redux estandar a un flujo de log, no de tabla CRUD

- Decision: El modulo Redux `trainingSessions` sigue el contrato obligatorio de `frontend-redux` (`loading`/`error`/`popUpCode`) pero sustituye `table`/`filters` (pensados para listados paginados con orden/filtro) por `history` (lista de sesiones del ejercicio planificado activo, sin paginacion — no pedida por el spec) y `form` (borrador de la sesion en curso: resumen, bloques, notas). Se documenta esta adaptacion explicitamente porque el dominio no es un CRUD de catalogo sino un formulario de registro + historial de solo lectura.
- Rationale: El propio skill reconoce que el contrato es "para modulo CRUD"; `TrainingSession` desde la perspectiva de esta pantalla es un flujo de creacion + consulta de historial (sin editar/borrar sesiones guardadas), por lo que `table`/`filters` no aportan valor y se sustituyen por los campos que el flujo realmente necesita, manteniendo `loading`/`error`/`popUpCode` intactos.
- Alternatives considered:
  - Forzar el contrato `table`/`filters` aunque no se use ordenacion/paginacion/filtrado: descartado por añadir campos muertos sin proposito.

## Decision 9: `PopupDialog` para bloque y para editar resumen; sin panel lateral fijo nuevo

- Decision: "Añadir/editar bloque" y "Editar resumen rapido" (FR-004, FR-025) se implementan ambos como instancias de `PopupDialog` (uno u otro abierto segun `popUpCode`, nunca ambos a la vez). No se crea un panel lateral fijo nuevo.
- Rationale: El contrato de `PopupDialog` ya soporta abrir un dialogo a la vez segun `popUpCode`, que es exactamente el comportamiento pedido por FR-025 ("uno u otro segun la accion del usuario"). El mockup de dos columnas no exige un panel *fijo* simultaneo; "pantallas anchas" solo implica que el dialogo puede mostrarse mas ancho (`maxWidth` de `PopupDialog`), no que dos paneles deban coexistir en pantalla. Introducir un panel lateral nuevo violaria la regla de reutilizacion de `frontend-components` sin justificacion suficiente.
- Alternatives considered:
  - Panel lateral fijo (`Drawer` persistente) para el resumen y otro para bloques, visibles simultaneamente: descartado; el spec no exige edicion simultanea de ambos, y anadiria un sistema de paneles nuevo no solicitado.

## Decision 10: Migracion EF Core aditiva y aislada

- Decision: La migracion `AddTrainingSessionsModule` solo crea las tablas `TrainingSessions` y `SessionBlocks` (mas sus indices/FKs). No modifica ninguna columna de `PlannedExercises`, `TrainingLogs` ni ninguna otra tabla existente. Antes de generar la migracion se revisa el estado real de la base de datos de desarrollo (via `dotnet ef migrations list` / comparacion con `AdminDbContextModelSnapshot.cs`) para confirmar que no hay drift pendiente que el scaffolding pudiera arrastrar.
- Rationale: Incidente reciente documentado — una migracion previa arrastro cambios de esquema no relacionados (`PlannedExercises.Notes`/`UserId`) que tuvieron que parchearse manualmente en `DatabaseMigrationExtensions.cs`. Esta feature no debe repetir ese patron.
- Alternatives considered:
  - Generar la migracion directamente con `dotnet ef migrations add` sin revisar el snapshot primero: descartado, es la causa raiz del incidente anterior.

## Validation Notes

- El spec ya cierra: mapeo `ExerciseType` -> perfil (fuerza/cardio), reglas de validacion de bloque (FR-009), limites de caracteres (100/150), edicion/eliminacion de bloques (FR-010/FR-011), persistencia multi-sesion sin sobrescritura (FR-015), acceso por propietario (FR-023), y no negatividad de duraciones (FR-024).
- No quedan `NEEDS CLARIFICATION` pendientes: todas las decisiones tecnicas de esta seccion derivan directamente de requisitos ya cerrados en el spec o de patrones ya existentes en el codebase.
