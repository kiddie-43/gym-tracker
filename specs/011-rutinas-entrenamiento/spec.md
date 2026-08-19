# Feature Specification: Pagina de Rutinas de Entrenamiento

**Feature Branch**: `011-prepare-spec-branch`  
**Created**: 2026-05-18  
**Status**: Draft  
**Input**: User description: "ahora vamos a crear la pagina de rutinas, es decir, donde el usuario va a poder crear sus rutinas y en estas rutinas podra crearse una rutina,por ejemplo rutina perdida de calorias, en esta rutina podra ponerse sesiones por ejemplo pecho, que tendra asignada unos dias, por ejemplo lunes, y jueves, en estas sesiones se asignan ejercicios y los ejercicios contienen series es decir por ejemplo 1 serie de 12 repeticiones levantando 40 kg"

## Clarifications

### Session 2026-05-18

- Q: En sesiones de rutina, origen de ejercicios disponibles. -> A: Solo permitir seleccionar ejercicios desde el catalogo existente.
- Q: Estrategia de borrado de rutina. -> A: Borrado logico obligatorio (archivar y permitir reactivacion).
- Q: Regla de sesiones por dia en una misma rutina. -> A: Permitir solo 1 sesion por dia.
- Q: Manejo de ediciones concurrentes de una misma rutina. -> A: No se prioriza soporte multi-dispositivo simultaneo; se aplica last write wins en esta fase.
- Q: Vista de rutinas para usuario final. -> A: Mostrar rutinas en tarjetas con titulo, fecha y cantidad de ejercicios; al entrar se listan ejercicios configurados con accion para anadir.
- Q: Registro de entrenamiento en detalle de ejercicio. -> A: Abrir ejercicio en pantalla completa para registrar kg/repeticiones, notas por usuario y adjuntos de foto o video.
- Q: Eliminacion de ejercicios desde rutina. -> A: La tarjeta de ejercicio permite quitarlo de la rutina mediante panel de eliminacion sin borrarlo fisicamente.
- Q: Limite de adjuntos por registro de entrenamiento. -> A: Maximo 5 adjuntos (fotos/videos) por registro.
- Q: Flujo UI durante entrenamiento. -> A: Usar stepper a pantalla completa en la misma ruta con arbol rutina > sesion > ejercicio > datos del ejercicio.
- Q: Control de foco del usuario durante entrenamiento. -> A: Agregar iniciar entrenamiento para bloquear navegacion externa y cancelar entrenamiento para restaurar navegacion normal.
- Q: Persistencia de progreso con bloqueo activo. -> A: Si hay F5 o cierre/reapertura, restaurar el ultimo estado de rutina/sesion/ejercicio/datos en curso.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Crear rutina personal (Priority: P1)

La persona usuaria puede crear una rutina con nombre y objetivo (por ejemplo, perdida de calorias) para organizar su entrenamiento semanal.

**Why this priority**: Sin la creacion de rutina no existe una base sobre la cual agregar sesiones, ejercicios o series.

**Independent Test**: Crear una rutina nueva con nombre y objetivo, guardarla y verificar que aparece en el listado personal de rutinas.

**Acceptance Scenarios**:

1. **Given** que la persona usuaria esta en la pagina de rutinas, **When** crea una rutina con nombre y objetivo validos, **Then** la rutina queda guardada y visible en su listado.
2. **Given** que la persona usuaria intenta guardar una rutina sin nombre, **When** confirma la accion, **Then** el sistema muestra un mensaje claro y evita el guardado.
3. **Given** que la persona usuaria ya tiene rutinas creadas, **When** entra nuevamente a la pagina de rutinas, **Then** puede ver sus rutinas existentes con su nombre y objetivo.

---

### User Story 2 - Planificar sesiones por dias (Priority: P1)

Dentro de una rutina, la persona usuaria puede crear sesiones (por ejemplo, "Pecho") y asignarles uno o mas dias de la semana (por ejemplo, lunes y jueves).

**Why this priority**: La planificacion por dias convierte la rutina en un plan ejecutable y ordenado.

**Independent Test**: En una rutina existente, crear una sesion, asignarle dias, guardar y validar que el calendario de la rutina refleja esos dias.

**Acceptance Scenarios**:

1. **Given** que existe una rutina, **When** la persona usuaria crea una sesion y selecciona dias validos de la semana, **Then** la sesion queda asociada a esos dias.
2. **Given** que una sesion ya tiene dias asignados, **When** la persona usuaria actualiza los dias, **Then** la sesion muestra la nueva asignacion sin duplicados.
3. **Given** que la persona usuaria intenta guardar una sesion sin dias, **When** confirma la accion, **Then** el sistema solicita al menos un dia antes de guardar.
4. **Given** que una sesion activa ya ocupa un dia especifico de la rutina, **When** la persona usuaria intenta asignar ese mismo dia a otra sesion activa, **Then** el sistema bloquea la accion e informa el conflicto.

---

### User Story 3 - Definir ejercicios y series (Priority: P2)

Dentro de cada sesion, la persona usuaria puede agregar ejercicios y para cada ejercicio definir una o varias series con repeticiones y carga (por ejemplo, 1 serie de 12 repeticiones con 40 kg).

**Why this priority**: Completa la estructura operativa del entrenamiento y permite ejecutar la rutina de forma concreta.

**Independent Test**: En una sesion existente, agregar un ejercicio con varias series, guardar y verificar que se conserva la estructura completa (ejercicio + series).

**Acceptance Scenarios**:

1. **Given** que existe una sesion en una rutina, **When** la persona usuaria agrega un ejercicio con una serie valida, **Then** el ejercicio y su serie quedan guardados en la sesion.
2. **Given** que un ejercicio ya tiene series, **When** la persona usuaria edita repeticiones o carga, **Then** los valores actualizados quedan guardados correctamente.
3. **Given** que la persona usuaria intenta guardar una serie con repeticiones o carga no validas, **When** confirma la accion, **Then** el sistema muestra el error y no guarda esa serie.

---

### User Story 4 - Registrar entrenamiento por ejercicio (Priority: P1)

La persona usuaria visualiza sus rutinas en tarjetas y, al abrir un ejercicio, registra su ejecucion real del entrenamiento con series realizadas, notas personales y evidencia multimedia.

**Why this priority**: Es el flujo de uso diario para ejecutar y documentar entrenamientos reales, que aporta valor directo al usuario final.

**Independent Test**: Abrir una rutina desde tarjeta, entrar a un ejercicio en pantalla completa, registrar una serie (kg y repeticiones), guardar nota y adjuntar foto/video; luego recargar y verificar persistencia por usuario.

**Acceptance Scenarios**:

1. **Given** que la persona usuaria entra a la vista de rutinas, **When** se muestran sus rutinas, **Then** cada tarjeta presenta titulo, fecha y cantidad de ejercicios configurados.
2. **Given** que la persona usuaria pulsa una tarjeta de rutina, **When** se abre el detalle, **Then** se listan los ejercicios configurados y se muestra accion para anadir ejercicio.
3. **Given** que la persona usuaria pulsa una tarjeta de ejercicio, **When** se abre la vista de pantalla completa, **Then** puede registrar kg y repeticiones de su entrenamiento.
4. **Given** que la persona usuaria agrega notas y adjuntos multimedia en un ejercicio, **When** guarda el registro, **Then** los datos quedan asociados a ese ejercicio y al usuario propietario.
5. **Given** que la persona usuaria elimina un ejercicio desde su tarjeta en la rutina, **When** confirma la accion, **Then** el ejercicio se quita de la rutina sin borrado fisico del catalogo.
6. **Given** que la persona usuaria consulta su progreso en un ejercicio, **When** abre el detalle de entrenamiento, **Then** visualiza comparativas contra la ultima sesion, promedio de las ultimas 4 y mejor marca reciente.

---

### User Story 5 - Ejecutar entrenamiento guiado con bloqueo de navegacion (Priority: P1)

La persona usuaria ejecuta su entrenamiento en un stepper a pantalla completa dentro de la misma vista, avanzando por el arbol rutina > sesion > ejercicio > datos del ejercicio con foco total en el flujo.

**Why this priority**: Evita distracciones, reduce abandonos del flujo y mejora la continuidad de entrenamiento incluso tras recargar o reabrir la aplicacion.

**Independent Test**: Iniciar entrenamiento desde una rutina, avanzar y retroceder entre pasos, recargar la pagina y reingresar; verificar restauracion del ultimo paso y bloqueo de navegacion hasta cancelar entrenamiento.

**Acceptance Scenarios**:

1. **Given** que la persona usuaria inicia entrenamiento, **When** entra al flujo guiado, **Then** el sistema muestra un stepper a pantalla completa sin cambiar a otra ruta.
2. **Given** que el entrenamiento esta activo, **When** la persona usuaria intenta salir del arbol de entrenamiento, **Then** el sistema mantiene el bloqueo y solo permite navegar dentro de rutina > sesion > ejercicio > datos del ejercicio.
3. **Given** que el entrenamiento esta activo, **When** la persona usuaria avanza o retrocede, **Then** el movimiento queda restringido a pasos validos del arbol.
4. **Given** que el entrenamiento esta activo, **When** la persona usuaria recarga la pagina o cierra y vuelve a entrar, **Then** el sistema restaura el ultimo estado del flujo en curso.
5. **Given** que la persona usuaria pulsa cancelar entrenamiento, **When** confirma la accion, **Then** el sistema desactiva el bloqueo y restablece la navegacion normal de la aplicacion.

---

### Edge Cases

- Intento de crear dos sesiones con el mismo nombre dentro de la misma rutina.
- Intento de asignar el mismo dia mas de una vez a una misma sesion.
- Intento de asignar un dia que ya esta ocupado por otra sesion activa de la misma rutina.
- Eliminacion de una sesion que ya contiene ejercicios y series.
- Ejercicios sin series definidas al momento de guardar la sesion.
- Rutina sin ejercicios configurados en el detalle (estado vacio con llamada a accion de anadir).
- Fallo al subir foto o video durante el guardado del entrenamiento del ejercicio.
- Intento de adjuntar mas de 5 archivos multimedia en un mismo registro de entrenamiento.
- Intento de acceder a notas o adjuntos de entrenamiento de otro usuario.
- Intento de salir a una seccion externa de la app mientras el bloqueo de entrenamiento esta activo.
- Recarga de pagina (F5) durante un entrenamiento activo con progreso parcial.
- Cierre y reapertura de la app durante entrenamiento activo sin perder el ultimo estado del arbol.
- Rutinas sin sesiones aun creadas (estado vacio inicial).
- Guardado interrumpido por error temporal de red durante la edicion de rutina.
- Catalogo externo de ejercicios no disponible durante el armado de sesion (modo degradado con retroalimentacion visible).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE permitir a una persona usuaria crear una rutina con nombre y objetivo.
- **FR-002**: El sistema DEBE validar que el nombre de la rutina sea obligatorio antes de guardar.
- **FR-003**: El sistema DEBE mostrar a la persona usuaria un listado de sus rutinas existentes.
- **FR-004**: El sistema DEBE permitir editar el nombre y objetivo de una rutina existente.
- **FR-005**: El sistema DEBE permitir crear sesiones dentro de una rutina.
- **FR-006**: Cada sesion DEBE permitir asignar uno o mas dias de la semana.
- **FR-007**: El sistema DEBE impedir dias duplicados dentro de la misma sesion.
- **FR-008**: El sistema DEBE exigir al menos un dia asignado para guardar una sesion.
- **FR-009**: El sistema DEBE permitir agregar ejercicios a una sesion seleccionandolos exclusivamente desde el catalogo de ejercicios existente.
- **FR-010**: Cada ejercicio DEBE permitir definir una o mas series.
- **FR-011**: Cada serie DEBE registrar al menos repeticiones y carga utilizada.
- **FR-012**: El sistema DEBE validar que repeticiones y carga sean valores positivos.
- **FR-013**: El sistema DEBE permitir editar y eliminar series dentro de un ejercicio.
- **FR-014**: El sistema DEBE conservar la relacion jerarquica rutina -> sesiones -> ejercicios -> series en cada guardado.
- **FR-015**: El sistema DEBE mostrar estados de carga, error y vacio en la pagina de rutinas.
- **FR-016**: El sistema DEBE garantizar que cada persona usuaria solo vea y gestione sus propias rutinas.
- **FR-017**: El sistema DEBE guardar cambios parciales de una rutina sin perder informacion ya registrada en otros niveles de la jerarquia.
- **FR-018**: El sistema DEBE aplicar borrado logico al eliminar una rutina completa, con confirmacion explicita para evitar borrados accidentales.
- **FR-019**: El sistema NO DEBE permitir crear ejercicios personalizados de texto libre dentro de una sesion de rutina.
- **FR-020**: El sistema DEBE permitir reactivar rutinas archivadas previamente por la persona usuaria propietaria.
- **FR-021**: El sistema NO DEBE realizar borrado fisico de rutinas desde la interfaz de usuario ni desde los flujos funcionales de esta feature.
- **FR-022**: El sistema DEBE impedir que dos sesiones activas de una misma rutina queden asignadas al mismo dia de la semana.
- **FR-023**: Si ocurre edicion concurrente de una misma rutina, el sistema DEBE aplicar estrategia last write wins sin flujo de resolucion de conflictos en esta fase.
- **FR-024**: La vista principal de rutinas del usuario DEBE mostrar las rutinas en formato tarjeta con titulo, fecha y cantidad de ejercicios.
- **FR-025**: Al seleccionar una tarjeta de rutina, el sistema DEBE mostrar la lista de ejercicios configurados e incluir una accion visible para anadir ejercicios.
- **FR-026**: Cada tarjeta de ejercicio dentro de una rutina DEBE incluir un panel de eliminacion para quitar el ejercicio de la rutina.
- **FR-027**: Al seleccionar un ejercicio desde la rutina, el sistema DEBE abrir una vista de detalle en pantalla completa del ejercicio.
- **FR-028**: En el detalle de ejercicio, el usuario DEBE poder registrar entradas de entrenamiento con al menos carga (kg) y repeticiones.
- **FR-029**: En el detalle de ejercicio, el usuario DEBE poder registrar notas de entrenamiento.
- **FR-030**: Las notas de entrenamiento DEBEN persistirse asociadas al ejercicio y al usuario propietario.
- **FR-031**: En el detalle de ejercicio, el usuario DEBE poder adjuntar fotos o videos relacionados con su entrenamiento.
- **FR-032**: Los adjuntos multimedia DEBEN quedar asociados al ejercicio y al usuario propietario.
- **FR-033**: Quitar un ejercicio desde la tarjeta de rutina DEBE aplicarse como desvinculacion logica dentro de la rutina y NO como borrado fisico del ejercicio base.
- **FR-034**: Cada registro de entrenamiento DEBE permitir un maximo de 5 adjuntos multimedia entre fotos y videos.
- **FR-035**: Si el usuario intenta superar el limite de adjuntos, el sistema DEBE bloquear la accion y mostrar un mensaje de validacion claro.
- **FR-036**: El flujo de ejecucion de entrenamiento DEBE presentarse como stepper a pantalla completa dentro de la misma ruta de la vista de rutinas.
- **FR-037**: El stepper DEBE seguir el orden de arbol rutina -> sesion -> ejercicio -> datos del ejercicio.
- **FR-038**: El flujo DEBE incluir un boton de iniciar entrenamiento que active un modo de bloqueo de navegacion externa.
- **FR-039**: Con el bloqueo activo, el usuario DEBE poder avanzar y retroceder solo dentro de los nodos del arbol de entrenamiento.
- **FR-040**: El flujo DEBE incluir un boton de cancelar entrenamiento que desactive el bloqueo y permita navegacion normal.
- **FR-041**: Si el bloqueo esta activo y la persona usuaria recarga la pagina o reingresa a la aplicacion, el sistema DEBE restaurar el ultimo estado del entrenamiento en curso.
- **FR-042**: El sistema DEBE calcular comparativas de progreso por ejercicio contra al menos: ultima sesion, promedio de las ultimas 4 sesiones y mejor marca reciente.
- **FR-043**: Las comparativas DEBEN incluir como minimo volumen total, carga total, repeticiones totales, variacion porcentual y tendencia (mejora/estable/empeora).
- **FR-044**: El detalle de ejercicio DEBE mostrar las comparativas de progreso de forma visible para la persona usuaria.
- **FR-045**: Si la fuente externa de catalogo no esta disponible, el sistema DEBE mantener operativas las rutinas guardadas y mostrar estado degradado al usuario.

### Key Entities *(include if feature involves data)*

- **Rutina**: Plan de entrenamiento personal que agrupa sesiones y define un objetivo general.
- **Sesion**: Bloque de trabajo dentro de una rutina, asociado a uno o varios dias de la semana.
- **Ejercicio de Sesion**: Actividad concreta asignada a una sesion de la rutina.
- **Serie de Ejercicio**: Unidad de ejecucion del ejercicio que incluye repeticiones y carga.
- **Registro de Entrenamiento de Ejercicio**: Registro por usuario de la ejecucion real del ejercicio, con datos de series realizadas, notas y adjuntos multimedia.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema DEBE tratar las rutinas, sesiones, ejercicios de sesion y series como datos propios de cada persona usuaria.
- **INT-002**: El sistema DEBE usar el catalogo de ejercicios existente como unica fuente para seleccionar ejercicios en sesiones de rutina.
- **INT-003**: El sistema DEBE definir un comportamiento de degradacion visible para la persona usuaria cuando una fuente externa de ejercicios no este disponible.
- **INT-004**: El sistema DEBE mantener trazabilidad entre ejercicios referenciados desde catalogos externos y su uso dentro de sesiones de rutina.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al menos el 95% de las personas usuarias de prueba puede crear una rutina con objetivo en menos de 2 minutos.
- **SC-002**: Al menos el 90% de las personas usuarias de prueba puede crear una sesion con dias asignados sin ayuda externa en su primer intento.
- **SC-003**: El 95% de los guardados validos de ejercicios y series se completa sin perdida de datos en la estructura rutina -> sesion -> ejercicio -> serie.
- **SC-004**: El 100% de los intentos con datos invalidos de series muestra mensajes de validacion comprensibles para la persona usuaria.
- **SC-005**: Al menos el 90% de las personas usuarias de prueba considera clara la organizacion de la pagina de rutinas en una evaluacion funcional interna.
- **SC-006**: El 95% de las rutinas mostradas en la vista principal renderiza correctamente titulo, fecha y cantidad de ejercicios en formato tarjeta.
- **SC-007**: Al menos el 90% de las personas usuarias de prueba completa un registro de entrenamiento (kg, repeticiones, nota y al menos un adjunto opcional) en menos de 3 minutos.
- **SC-008**: El 100% de las notas y adjuntos guardados en pruebas funcionales se recupera correctamente solo para el usuario propietario.
- **SC-009**: Al menos el 90% de las personas usuarias de prueba completa el flujo guiado del stepper sin abandonar el entrenamiento por navegacion accidental.
- **SC-010**: El 100% de los casos de prueba con recarga o reapertura durante entrenamiento activo restaura correctamente el ultimo estado del arbol rutina -> sesion -> ejercicio -> datos.
- **SC-011**: El 100% de los casos de prueba de comparativas de progreso devuelve y muestra correctamente volumen total, carga total, repeticiones totales, variacion porcentual y tendencia.

## Assumptions

- La funcionalidad esta dirigida a personas usuarias autenticadas dentro de la aplicacion.
- La primera version contempla dias semanales estandares (lunes a domingo).
- La carga de cada serie se registra en kilogramos como unidad base para esta version.
- Las rutinas son de propiedad individual y no se comparten entre personas usuarias en esta fase.
- La pagina de rutinas debe funcionar correctamente tanto para cuentas nuevas (sin datos) como para cuentas con multiples rutinas existentes.
- La definicion de ejercicios disponibles para sesiones depende del catalogo de ejercicios ya administrado por la plataforma.
- La politica constitucional del proyecto exige borrado logico para cualquier operacion de eliminacion en este alcance funcional.
- Se asume baja probabilidad de edicion simultanea de una misma rutina desde multiples dispositivos para una misma persona usuaria.
