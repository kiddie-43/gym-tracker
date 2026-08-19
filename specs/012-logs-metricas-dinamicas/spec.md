# Feature Specification: Replanteo de Logs Dinamicos de Entrenamiento

**Feature Branch**: `012-git-feature-hook`  
**Created**: 2026-05-28  
**Status**: Draft  
**Input**: User description: "necesito que se elimine lo que haya ahora mismo de obtener logs de los ejercicios, y lo vamos a replantear, la nueva manera consistira en una tabla que tendra los siguientes campos, id de usuario, id de log id de rutina id de session id de entrenamiento id de metrica fecha y valor de la metrica, de esta manera conseguiremos tener la capacidad de logs dinamicos, por si el dia de mañana cambian o se añaden nuevas metricas, necesito que se creen los endpoints para, obtener las metricas, crearlas, editarlas y borrarlas, aparte de los logs de cambios etc, en el front, se creara el estado de redux su reducer y sus acciones, aparte de la interfaz para tener las propiedades definidas, solo tendras que utilizar una interfaz para todo, luego hay que crear la pagina de detalle de logs llamando al redux tanto para abrir pop ups como el listado como esta en administracion de ejercicios, rutinas, y sessiones, tambien se tendra que crear el servicio que llamara a estos endpoints, aparte en el front, para el detalle del ejercicio, se tendra que validar dependiendo de que metricas contenga se tendra que mostrar una cosa o otrs, de momento me vas a dejar un div con el nombre de la metrica y en otra spec ya definiremos que componentes pondremos"

## Clarifications

### Session 2026-05-28

- Q: Como evitar ambiguedad al editar/borrar y como identificar el conjunto de metricas relacionadas? → A: No pueden existir dos grupos identicos; las operaciones de edicion y borrado deben resolverse por idGrupo para afectar todo lo relacionado.
- Q: Si existen multiples grupos en el mismo ejercicio/dia, que debe ocurrir al editar uno? → A: La edicion debe afectar solo el idGrupo objetivo (por ejemplo grupo 1) y no debe modificar ningun otro grupo.
- Q: Como se determina "lo actual" en operacion diaria? → A: Se toma el grupo de la fecha actual con la ultima fecha de creacion (CreatedAt) como vigente.
- Q: Que debe devolver la consulta diaria si no hay logs en la fecha actual? → A: Debe devolver un array vacio.
- Q: Como debe operar el borrado logico del conjunto? → A: El borrado logico se ejecuta solo por idGrupo y afecta todo el grupo relacionado.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gestionar logs metricos dinamicos por ejercicio (Priority: P1)

Como usuario autenticado quiero registrar, consultar, editar y eliminar logs metricos de mis entrenamientos para cada ejercicio, de manera que el sistema soporte metricas actuales y futuras sin rediseñar el flujo de captura.

**Why this priority**: Es el nucleo funcional del cambio; sin este flujo no existe el nuevo modelo dinamico de logs.

**Independent Test**: Puede validarse creando un log metrico, consultandolo, actualizando solo el valor de la metrica y eliminandolo, verificando que los datos restantes de contexto del log no cambian.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado con una rutina, sesion, entrenamiento y metrica validos, **When** crea un log metrico, **Then** el sistema guarda un registro con usuario, log, rutina, sesion, entrenamiento, metrica, fecha y valor.
2. **Given** un log metrico existente del usuario, **When** consulta sus logs, **Then** el sistema devuelve los registros con todos los campos del modelo dinamico.
3. **Given** un log metrico existente del usuario, **When** actualiza el valor de la metrica, **Then** el sistema cambia solo el valor de metrica y conserva sin cambios el resto de campos de contexto.
4. **Given** un log metrico existente del usuario, **When** lo elimina, **Then** el sistema deja de exponerlo en consultas activas y mantiene trazabilidad de la accion.

---

### User Story 2 - Visualizar detalle de logs con estado global y popups (Priority: P2)

Como usuario quiero una pagina de detalle de logs con listado y popups de gestion para operar sobre mis metricas de entrenamiento con una experiencia consistente con otros modulos administrativos, reutilizando los componentes compartidos de popup y menu de acciones.

**Why this priority**: Permite operar el nuevo modelo desde la interfaz principal y reduce friccion al reutilizar patrones de interaccion ya conocidos por el usuario.

**Independent Test**: Puede validarse navegando a la pagina de detalle de logs, abriendo popups de alta/edicion/baja, ejecutando acciones y confirmando refresco del listado sin recargar la aplicacion.

**Acceptance Scenarios**:

1. **Given** que existen logs metricos del usuario, **When** abre la pagina de detalle de logs, **Then** visualiza el listado con los campos del nuevo modelo y acciones de gestion.
2. **Given** que el usuario abre una accion de crear o editar, **When** confirma la operacion en popup, **Then** el estado global refleja el cambio y el listado se actualiza.
3. **Given** una operacion fallida por validacion, **When** la accion se procesa, **Then** el usuario recibe mensaje claro y el estado de la vista permanece consistente.
4. **Given** un item del listado de metricas, **When** el usuario abre el menu de acciones y elige editar o eliminar, **Then** el popup correspondiente se abre usando el estado global del modulo.

---

### User Story 3 - Renderizar metricas dinamicas en detalle de ejercicio (Priority: P3)

Como usuario quiero que el detalle del ejercicio renderice dinamicamente las metricas configuradas y, por ahora, muestre un bloque simple con el nombre de cada metrica para preparar una futura evolucion de componentes.

**Why this priority**: Permite confirmar que el sistema ya interpreta metricas dinamicas por ejercicio y deja listo el punto de extension de UI para la siguiente iteracion.

**Independent Test**: Puede validarse abriendo detalle de ejercicio con distintas combinaciones de metricas y comprobando que se renderiza un bloque por cada nombre de metrica.

**Acceptance Scenarios**:

1. **Given** un ejercicio con multiples metricas activas, **When** el usuario abre el detalle de ejercicio, **Then** se muestra un bloque por cada metrica con su nombre.
2. **Given** un ejercicio sin metricas configuradas, **When** el usuario abre el detalle, **Then** la vista informa estado vacio sin error.

---

### User Story 4 - Operar con referencia a fecha actual (Priority: P2)

Como usuario quiero que la gestion diaria de logs metricos se resuelva usando la fecha actual para crear, editar, borrar y consultar lo vigente, sin perder el registro completo de fecha y hora.

**Why this priority**: La operacion del entrenamiento diario depende de encontrar rapidamente el registro actual por fecha, mientras se conserva trazabilidad temporal completa.

**Independent Test**: Puede validarse ejecutando crear, editar y borrar en la fecha actual y verificando que el sistema encuentra el registro del dia por fecha, pero persiste fecha y hora completas en almacenamiento.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado en su dia de entrenamiento, **When** crea un log metrico, **Then** el registro queda asociado a la fecha actual y guarda marca temporal completa.
2. **Given** logs metricos historicos y del dia actual, **When** el usuario abre la gestion diaria, **Then** el sistema prioriza la fecha actual e identifica como vigente el grupo con ultima fecha de creacion.
3. **Given** un log del dia actual, **When** el usuario edita o elimina, **Then** la operacion se resuelve sobre el registro vigente del dia y conserva la trazabilidad temporal completa.
4. **Given** que no existen logs en la fecha actual, **When** el usuario consulta la gestion diaria, **Then** el sistema devuelve un array vacio.

---

### Edge Cases

- Cuando se intenta crear un log con una metrica no vinculada al ejercicio, la operacion debe rechazarse con mensaje claro.
- Cuando un usuario intenta editar o eliminar un log que no le pertenece, la operacion debe rechazarse.
- Cuando se elimina o desactiva una metrica historicamente usada, los logs historicos deben seguir siendo consultables con su traza.
- Cuando se reciben valores metricos fuera de rango permitido, el sistema debe rechazar la operacion sin corromper datos.
- Cuando se dispara una doble confirmacion de guardado (click repetido), el resultado final debe ser consistente y sin duplicados involuntarios.
- Cuando existe mas de un registro del mismo contexto en la fecha actual, el sistema debe considerar vigente el grupo con ultima fecha de creacion.
- Cuando un registro fue eliminado logicamente, no debe aparecer en listados activos ni permitir edicion directa.
- Cuando se intenta crear o forzar manualmente un id de grupo ya existente, la operacion debe ser rechazada para preservar unicidad global.
- Cuando se intenta crear un grupo con el mismo contexto de otro grupo ya existente, la operacion debe rechazarse para evitar grupos identicos.
- Cuando se edita un grupo objetivo, ningun valor perteneciente a otros grupos del mismo ejercicio o fecha debe alterarse.
- Cuando no existan logs para la fecha actual, la respuesta debe ser un array vacio y no un error.
- Cuando se intente borrar solo una metrica individual del grupo, la operacion debe rechazarse porque el borrado logico es por idGrupo completo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST reemplazar la obtencion de logs de ejercicios actual por un modelo de log metrico dinamico basado en una sola estructura de registro usando como unica superficie soportada `/api/training-metric-logs/*` con contrato OpenAPI `2.0.0`; la migracion de consumidores MUST completarse antes del despliegue de esa version y no existe ventana de compatibilidad dentro del mismo despliegue.
- **FR-002**: El sistema MUST almacenar cada log metrico con los campos: idUsuario, idLog, idRutina, idSesion, idEntrenamiento, idMetrica, fecha, valorMetrica e idGrupo.
- **FR-003**: El sistema MUST permitir crear grupos de logs metricos (set de captura) para usuarios autenticados, resolviendo el alta sobre el conjunto completo de metricas del grupo.
- **FR-004**: El sistema MUST permitir consultar logs metricos del usuario autenticado con filtros por contexto de entrenamiento (rutina, sesion, entrenamiento y metrica) y con paginacion en listados potencialmente grandes.
- **FR-005**: El sistema MUST permitir editar logs metricos existentes del usuario autenticado resolviendo el conjunto objetivo exclusivamente por idGrupo.
- **FR-006**: El sistema MUST restringir la edicion para que solo se modifique valorMetrica; todos los demas campos del registro deben ser inmutables para clientes externos.
- **FR-007**: El sistema MUST aplicar eliminacion logica en logs metricos del usuario autenticado exclusivamente por idGrupo, eliminando logicamente todo el conjunto relacionado, ocultandolo de consultas activas y registrando la accion en la traza de cambios.
- **FR-008**: El sistema MUST registrar trazabilidad de cambios para crear, editar y eliminar logs metricos, incluyendo quien realizo la accion, cuando y que cambio.
- **FR-009**: El sistema MUST exponer capacidades de lectura de metricas disponibles para permitir captura dinamica en frontend.
- **FR-010**: El sistema MUST validar que idMetrica sea valido para el contexto del ejercicio antes de crear o editar un log.
- **FR-011**: El sistema MUST rechazar operaciones sobre logs o metricas que no pertenezcan al usuario autenticado o no esten autorizadas.
- **FR-012**: El sistema MUST ofrecer en frontend un estado global para listado, carga, error y resultado de operaciones CRUD de logs metricos.
- **FR-013**: El sistema MUST usar un unico contrato de datos de log metrico en frontend para mantener tipado coherente en estado, acciones y consumo de servicios.
- **FR-014**: El sistema MUST ofrecer una pagina de detalle de logs con listado y popups de gestion de crear, editar y eliminar.
- **FR-015**: El sistema MUST integrar un servicio frontend dedicado para consumir las capacidades CRUD del backend de logs metricos.
- **FR-016**: El sistema MUST actualizar automaticamente el listado de logs en la pagina de detalle tras operaciones exitosas de crear, editar o eliminar.
- **FR-017**: El sistema MUST renderizar dinamicamente en el detalle del ejercicio un bloque por cada metrica disponible mostrando, como minimo, el nombre de la metrica.
- **FR-018**: El sistema MUST mostrar estados vacio, carga y error en la pagina de detalle de logs y en la vista dinamica de metricas de ejercicio, utilizando el componente `FeedbackMessage` como mecanismo estandar de presentacion.
- **FR-019**: El sistema MUST conectar las acciones de crear, editar y eliminar al estado global del modulo en frontend usando el componente de popup existente del proyecto.
- **FR-020**: El sistema MUST usar el componente de menu de acciones existente del proyecto para disparar editar y eliminar metrica desde el listado.
- **FR-021**: El sistema MUST priorizar la fecha actual para obtener y resolver el log metrico vigente cuando el usuario gestione crear, editar o eliminar, considerando vigente el grupo con ultima fecha de creacion (CreatedAt).
- **FR-022**: El sistema MUST almacenar fecha y hora completas en `createdAt` y `updatedAt` para los eventos de log, aunque la fecha operativa de captura se exponga y resuelva por fecha (`date`).
- **FR-023**: El sistema MUST asignar a cada set de captura un idGrupo unico global que nunca se repita con otro grupo.
- **FR-024**: El sistema MUST generar y gestionar idGrupo exclusivamente en backend y MUST rechazar su envio desde el cliente en operaciones de alta o edicion.
- **FR-025**: El sistema MUST tratar idGrupo como identificador inmutable de ciclo de vida completo y MUST rechazar cualquier intento de mutacion una vez creado el grupo.
- **FR-026**: El sistema MUST impedir la creacion de grupos identicos para el mismo usuario y contexto de captura dentro de la misma fecha operativa.
- **FR-027**: El sistema MUST aplicar alcance de operacion a nivel de grupo completo en acciones mutables de grupo (por ejemplo, `deleteGroup` y cualquier mutacion bulk del set) y MUST rechazar direccionamiento parcial por `idLog` individual cuando la operacion declarada sea de grupo; esta regla no sustituye operaciones puntuales por metrica explicitamente modeladas en el contrato (por ejemplo `patchValue` por `groupId` + `metricId`).
- **FR-028**: El sistema MUST garantizar aislamiento de grupo en operaciones de edicion: una actualizacion sobre un idGrupo solo puede afectar registros pertenecientes a ese mismo idGrupo.
- **FR-029**: El sistema MUST devolver un array vacio cuando una consulta diaria por fecha actual no encuentre logs del usuario para el contexto solicitado.
- **FR-030**: El sistema MUST exponer el rechazo contractual de borrado parcial por metrica dentro de un grupo (ruta no soportada o respuesta de error de cliente), manteniendo como unica operacion valida de borrado logico la eliminacion por `idGrupo` completo.
- **FR-031**: El sistema MUST exponer health checks de dependencias criticas del modulo (persistencia SQL y fuente de definiciones de metricas) para verificacion operativa.
- **FR-032**: El sistema MUST permitir un modo temporal de autenticacion simulada solo en `Development` cuando no exista proveedor de identidad configurado, registrando su activacion y bloqueando su uso en `Staging` y `Production`.

### Canonical Terminology

- `idGrupo` es el termino de dominio canonico en reglas funcionales y modelo de datos.
- `groupId` se admite solo como alias de contrato/DTO para interoperabilidad API y tipos frontend.
- En caso de ambiguedad documental, prevalece `idGrupo` como semantica de negocio.

### Key Entities *(include if feature involves data)*

- **LogMetricoEntrenamiento**: Registro transaccional de una metrica capturada en un contexto de entrenamiento; contiene idUsuario, idLog, idRutina, idSesion, idEntrenamiento, idMetrica, fecha, valorMetrica, idGrupo, y estado de eliminacion logica.
- **GrupoCapturaMetrica**: Identificador inmutable y privado que agrupa todas las metricas correspondientes a un mismo set de captura y sirve como referencia principal para editar o eliminar el conjunto relacionado.
- **DefinicionMetricaEjercicio**: Metrica habilitada para un ejercicio, utilizada para validar capturas y para renderizado dinamico de detalle.
- **EventoCambioLogMetrico**: Traza auditable de operaciones sobre logs metricos (alta, edicion, baja) con actor, marca temporal y detalle de cambios.
- **EstadoDetalleLogs**: Estado global de la experiencia de detalle de logs, incluyendo coleccion actual, estado de carga, errores y dialogo activo.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema MUST distinguir entre datos de catalogo (definiciones de metricas/ejercicios) y datos transaccionales de usuario (logs metricos y eventos de cambio).
- **INT-002**: El sistema MUST almacenar los registros transaccionales de logs metricos en la base de datos principal del proyecto.
- **INT-003**: El sistema MUST definir degradacion controlada cuando no se puedan obtener definiciones de metricas, devolviendo `200 OK` con cabecera `X-Metric-Definitions-Status=degraded`, lista vacia de definiciones y preservando la integridad de logs existentes.
- **INT-004**: El sistema MUST preservar trazabilidad de identificadores de catalogo usados por cada log metrico para mantener consistencia historica.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de operaciones de crear, consultar, editar y eliminar logs metricos de usuarios autenticados devuelve codigos HTTP esperados (2xx en exito, 4xx en validacion/autorizacion), esquema de respuesta valido segun OpenAPI y sin errores no controlados en pruebas de aceptacion del modulo.
- **SC-002**: Al menos el 95% de consultas de listado de logs metricos y `current-day` devuelve resultados en menos de 2 segundos bajo un perfil de carga de referencia de 20 usuarios concurrentes, 10 minutos de ejecucion y dataset semilla de 10.000 registros.
- **SC-003**: El 100% de ediciones de logs metricos mantiene inmutables los campos de contexto (usuario, rutina, sesion, entrenamiento, metrica y fecha) y solo cambia valorMetrica.
- **SC-004**: El 100% de operaciones mutables (alta, edicion, baja) genera un evento de trazabilidad verificable.
- **SC-005**: El 100% de ejercicios con metricas configuradas renderiza un bloque visible por metrica en la vista de detalle del ejercicio.
- **SC-006**: El 100% de acciones diarias de crear, editar y eliminar resuelve el registro vigente usando fecha actual y ultima fecha de creacion en pruebas de aceptacion.
- **SC-007**: El 100% de eliminaciones de logs metricos se comporta como eliminacion logica y los registros eliminados no aparecen en consultas activas.
- **SC-008**: El 100% de ids de grupo generados en el modulo mantiene unicidad global sin colisiones en pruebas de integridad de datos.
- **SC-009**: El 100% de intentos de editar campos distintos a valorMetrica es rechazado en pruebas de validacion del modulo.
- **SC-010**: El 100% de operaciones de borrado logico por idGrupo afecta consistentemente todos los registros metricos relacionados definidos por el conjunto objetivo.
- **SC-011**: El 100% de ediciones sobre un idGrupo mantiene sin cambios los registros pertenecientes a otros idGrupo del mismo contexto de ejercicio/fecha.
- **SC-012**: El 100% de consultas diarias sin datos retorna array vacio, sin errores de negocio.
- **SC-013**: El 100% de health checks de dependencias criticas del modulo reporta estado consistente con la disponibilidad real en pruebas de integracion.
- **SC-014**: El 100% de pruebas de seguridad del modo de autenticacion simulada confirma habilitacion exclusiva en `Development` y bloqueo efectivo en `Staging` y `Production`.

## Assumptions

- La autenticacion y autorizacion de usuario ya existen y se reutilizan para proteger las operaciones del nuevo modulo.
- El inventario de metricas por ejercicio existe en el dominio administrativo y puede consultarse para validar capturas dinamicas.
- La sustitucion del modelo de logs actual se entrega sin migracion de historico previo, pero con plan de migracion de consumidores y retiro programado de endpoints legacy por version.
- La primera version de UI dinamica de metricas en detalle de ejercicio se limita a bloques simples con nombre de metrica; componentes avanzados quedan fuera de alcance.
- La gestion de adjuntos multimedia en logs no forma parte de este replanteamiento y se tratara en una iteracion futura si aplica.
- El criterio de agrupacion adoptado en esta iteracion es un idGrupo por set de captura; la evolucion de componentes visuales para valores compuestos queda para una especificacion posterior.
