# Feature Specification: Modulo de Administracion de Ejercicios con Multimedia

**Feature Branch**: `[002-modulo-administracion-ejercicios-multimedia]`  
**Created**: 2026-05-11  
**Status**: Draft  
**Input**: User description: "Necesito construir un modulo de administracion completo para Gym Tracker, con frontend y backend, para gestionar datos maestros de ejercicios y su logica de captura. Este modulo debe integrarse con Firestore y Firebase Storage, coexistir con la logica actual de entrenamientos y dejar preparado el consumo desde el flujo de workouts."

## Clarifications

### Session 2026-05-11

- Q: Como se modela la relacion entre musculos y grupos musculares? → A: Un musculo puede pertenecer a multiples grupos musculares.
- Q: Como debe comportarse la eliminacion de entidades administrativas? → A: Debe ser borrado logico.
- Q: Que catalogo debe usar workouts para seleccionar ejercicios? → A: Workouts debe usar solo ejercicios administrados en el modulo admin.
- Q: Como debe preservar workouts el historico de ejercicios usados? → A: Debe guardar `exerciseId` y un snapshot minimo con nombre, portada y tipo de formulario al momento del registro.
- Q: Cuales son los limites de multimedia por ejercicio? → A: Maximo 8 archivos por ejercicio, con hasta 6 imagenes de 5 MB y 2 videos de 100 MB.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Administrar catalogo maestro fitness (Priority: P1)

Como administrador, quiero gestionar zonas musculares, musculos, tipos de ejercicio y tipos de formularios para mantener el catalogo funcional y consistente sin depender de cambios en codigo.

**Why this priority**: Es la base del panel admin y habilita todo el flujo de definicion de ejercicios y captura de entrenamientos.

**Independent Test**: Puede probarse creando, editando y eliminando cada entidad desde el panel admin, validando persistencia en backend y Firestore.

**Acceptance Scenarios**:

1. **Given** que el administrador accede al panel, **When** crea una zona muscular con codigo unico, **Then** la zona queda disponible para asociacion en musculos y ejercicios.
2. **Given** que existe un musculo, **When** el administrador actualiza sus zonas asociadas, **Then** el cambio se refleja en formularios y filtros relacionados.
3. **Given** que existe un tipo de formulario, **When** el administrador define campos dinamicos, **Then** la configuracion se guarda y queda reutilizable en ejercicios.

---

### User Story 2 - Gestionar ejercicios con multimedia educativa (Priority: P1)

Como administrador, quiero crear ejercicios con imagenes y videos para que los usuarios vean como se ejecuta cada movimiento.

**Why this priority**: El valor principal del nuevo modulo depende de que cada ejercicio sea administrable y tenga contenido visual util.

**Independent Test**: Puede probarse creando un ejercicio, subiendo multiples imagenes/videos, marcando media principal, reordenando y eliminando recursos.

**Acceptance Scenarios**:

1. **Given** que existe un ejercicio, **When** el administrador solicita una URL de subida y sube un video, **Then** puede registrar metadatos y ver preview en el panel.
2. **Given** multiples media en un ejercicio, **When** el administrador marca una como principal, **Then** esa media se usa como portada del ejercicio.
3. **Given** una media existente, **When** el administrador la elimina, **Then** se elimina metadato en Firestore y binario en Firebase Storage.

---

### User Story 3 - Consumir catalogo admin en workouts sin ruptura (Priority: P2)

Como usuario final, quiero que los ejercicios administrados aparezcan en el flujo de entrenamientos para registrar sesiones con datos actualizados.

**Why this priority**: Asegura que el modulo admin tenga impacto directo en la experiencia principal de entrenamiento.

**Independent Test**: Puede probarse creando un ejercicio en admin y validando su aparicion en seleccion de ejercicios de workout.

**Acceptance Scenarios**:

1. **Given** un ejercicio activo creado en admin, **When** un usuario abre el formulario de workout, **Then** puede buscar y seleccionar ese ejercicio desde el catalogo administrado.
2. **Given** que el admin actualiza nombre o portada del ejercicio, **When** el usuario consulta listados y detalle, **Then** ve datos actualizados segun contratos definidos.

---

### Edge Cases

- Intento de crear codigos duplicados en zonas, musculos, tipos o ejercicios.
- Intento de eliminar logicamente una entidad administrativa que ya esta referenciada por ejercicios o workouts existentes.
- Archivos multimedia que exceden limite de tamano o formato no permitido.
- Fallo parcial: archivo subido en Storage pero metadato no registrado en Firestore.
- Eliminacion de media principal dejando ejercicio sin portada.
- Intento de acceso no admin a endpoints /api/admin/*.
- Caida temporal de Firebase Storage durante upload o delete.
- Campos dinamicos invalidos en tipos de formularios (min > max, opciones vacias en select, etc.).

## Requirements *(mandatory)*

### Functional Requirements

#### Seguridad y acceso

- **FR-001**: El sistema MUST restringir todas las rutas `/api/admin/*` a usuarios con rol administrador.
- **FR-002**: El frontend MUST proteger la ruta `/admin` y bloquear acceso a usuarios no administradores.

#### CRUD de datos maestros

- **FR-003**: El sistema MUST soportar CRUD completo para zonas musculares (`muscle-groups`).
- **FR-004**: El sistema MUST soportar CRUD completo para musculos (`muscles`) permitiendo asociar cada musculo a uno o multiples grupos musculares.
- **FR-005**: El sistema MUST soportar CRUD completo para tipos de ejercicio (`exercise-types`).
- **FR-006**: El sistema MUST soportar CRUD completo para tipos de formularios (`exercise-form-types`) con definicion dinamica de campos.
- **FR-007**: El sistema MUST soportar CRUD completo para ejercicios (`exercises`) con asignacion de tipo de ejercicio, tipo de formulario y musculos primarios/secundarios.
- **FR-007a**: El sistema MUST aplicar borrado logico para entidades administrativas y preservar referencias historicas existentes en workouts y catalogos relacionados.

#### Endpoints REST obligatorios

- **FR-008**: El backend MUST exponer para zonas musculares: `GET /api/admin/muscle-groups`, `GET /api/admin/muscle-groups/{id}`, `POST /api/admin/muscle-groups`, `PUT /api/admin/muscle-groups/{id}`, `DELETE /api/admin/muscle-groups/{id}`.
- **FR-009**: El backend MUST exponer para musculos: `GET /api/admin/muscles`, `GET /api/admin/muscles/{id}`, `POST /api/admin/muscles`, `PUT /api/admin/muscles/{id}`, `DELETE /api/admin/muscles/{id}`.
- **FR-010**: El backend MUST exponer para tipos de ejercicio: `GET /api/admin/exercise-types`, `GET /api/admin/exercise-types/{id}`, `POST /api/admin/exercise-types`, `PUT /api/admin/exercise-types/{id}`, `DELETE /api/admin/exercise-types/{id}`.
- **FR-011**: El backend MUST exponer para tipos de formularios: `GET /api/admin/exercise-form-types`, `GET /api/admin/exercise-form-types/{id}`, `POST /api/admin/exercise-form-types`, `PUT /api/admin/exercise-form-types/{id}`, `DELETE /api/admin/exercise-form-types/{id}`.
- **FR-012**: El backend MUST exponer para ejercicios: `GET /api/admin/exercises`, `GET /api/admin/exercises/{id}`, `POST /api/admin/exercises`, `PUT /api/admin/exercises/{id}`, `DELETE /api/admin/exercises/{id}`.

#### Multimedia de ejercicios

- **FR-013**: El sistema MUST permitir multiples imagenes y videos por ejercicio.
- **FR-014**: El backend MUST exponer `POST /api/admin/exercises/{exerciseId}/media/upload-url` para generar URL firmada y `storagePath`.
- **FR-015**: El backend MUST exponer `POST /api/admin/exercises/{exerciseId}/media` para registrar metadatos de media subida.
- **FR-016**: El backend MUST exponer `GET /api/admin/exercises/{exerciseId}/media` para listar la galeria de media.
- **FR-017**: El backend MUST exponer `PATCH /api/admin/exercises/{exerciseId}/media/{mediaId}` para actualizar metadatos (`title`, `active`, `sortOrder`, `thumbnailUrl`).
- **FR-018**: El backend MUST exponer `PATCH /api/admin/exercises/{exerciseId}/media/reorder` para reordenamiento masivo por `sortOrder`.
- **FR-019**: El backend MUST exponer `PATCH /api/admin/exercises/{exerciseId}/media/{mediaId}/set-primary` para marcar media principal.
- **FR-020**: El backend MUST exponer `DELETE /api/admin/exercises/{exerciseId}/media/{mediaId}` para eliminar metadato y archivo en Storage.
- **FR-021**: El sistema MUST validar formatos permitidos, con un maximo de 8 archivos por ejercicio, hasta 6 imagenes de 5 MB cada una y hasta 2 videos de 100 MB cada uno.

#### Persistencia e integraciones

- **FR-022**: El sistema MUST persistir datos maestros y metadatos en Firestore.
- **FR-023**: El sistema MUST almacenar binarios multimedia en Firebase Storage.
- **FR-024**: El backend MUST orquestar operaciones atomicas/logicamente consistentes entre Firestore y Storage (incluyendo compensacion en fallos parciales).
- **FR-025**: El sistema MUST definir reglas de seguridad para permitir lectura autenticada y escritura admin en colecciones/paths de administracion.

#### Frontend admin y consumo

- **FR-026**: El frontend MUST ofrecer tablas con busqueda, filtros y paginacion en los cinco paneles admin.
- **FR-027**: El panel de ejercicios MUST incluir uploader con preview, reorder, set primary y delete para media.
- **FR-028**: La UI MUST mantenerse en espanol para textos visibles de administracion.
- **FR-029**: Los ejercicios administrados activos MUST aparecer en el flujo de seleccion de workouts.
- **FR-029a**: El flujo de seleccion de workouts MUST consumir solo ejercicios administrados activos del modulo admin como fuente de catalogo seleccionable.
- **FR-030**: El consumo en workouts MUST mantenerse compatible con contratos actuales, sin romper historial existente.
- **FR-030a**: Cada workout registrado MUST persistir `exerciseId` y un snapshot minimo del ejercicio (`name`, `cover`, `formType`) para preservar el historico ante cambios o desactivaciones posteriores.

### Key Entities *(include if feature involves data)*

- **MuscleGroup**: Zona muscular con `id`, `name`, `code`, `description`, `active`, timestamps.
- **Muscle**: Musculo con `id`, `name`, `code`, `description`, `active`, `muscleGroupIds`, timestamps. Un musculo puede pertenecer a uno o multiples `MuscleGroup`.
- **ExerciseType**: Tipo de ejercicio con `id`, `name`, `code`, `description`, `primaryUnit`, `requiresUnits`, `active`, timestamps.
- **ExerciseFormType**: Definicion dinamica de campos de captura con `fields[]` y validaciones.
- **Exercise**: Ejercicio administrable con relaciones a tipo, formulario y musculos; incluye estado y metadata base.
- **ExerciseMedia**: Recurso multimedia por ejercicio con `mediaId`, `mediaType`, `url`, `storagePath`, `title`, `isPrimary`, `sortOrder`, `thumbnailUrl`, `active`, `createdAt`.
- **WorkoutExerciseSnapshot**: Snapshot minimo persistido dentro del workout con `exerciseId`, `name`, `cover`, `formType` y timestamp de captura para preservar consistencia historica.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema MUST tratar Firestore como persistencia de metadatos de administracion.
- **INT-002**: El sistema MUST tratar Firebase Storage como persistencia de binarios de media.
- **INT-003**: El sistema MUST preservar trazabilidad entre `Exercise` y `ExerciseMedia` mediante `exerciseId` y `storagePath`.
- **INT-004**: El sistema MUST definir comportamiento de degradacion cuando Firestore o Storage no esten disponibles.
- **INT-005**: El sistema MUST mantener interoperabilidad con el flujo actual de workouts y catalogo existente.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un administrador puede crear cualquier entidad maestra (zona, musculo, tipo, formulario, ejercicio) en menos de 2 minutos por entidad en condiciones normales.
- **SC-002**: Subir y registrar una media (imagen o video) por ejercicio completa en menos de 30 segundos para archivos dentro de limites configurados.
- **SC-003**: El 100% de rutas `/api/admin/*` rechazan usuarios sin rol admin.
- **SC-004**: El 100% de operaciones de borrado de media eliminan metadato y archivo asociado sin dejar huerfanos persistentes.
- **SC-005**: Los ejercicios administrados aparecen en seleccion de workout en menos de 5 segundos tras sincronizacion/persistencia.
- **SC-006**: No se introducen regresiones en los flujos actuales de registro e historial de entrenamientos.

## Assumptions

- Ya existe autenticacion y mecanismo de roles reutilizable para distinguir admins.
- Firestore y Firebase Storage ya estan configurados para el proyecto con credenciales de backend.
- La primera iteracion prioriza web responsive y no incluye app movil nativa.
- Los uploads se realizaran con URL firmada emitida por backend.
- El consumo en workouts puede evolucionar gradualmente para soportar portada multimedia sin migracion masiva inicial.
