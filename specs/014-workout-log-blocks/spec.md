# Feature Specification: Registrar Entrenamiento por Bloques (Plan Mensual)

**Feature Branch**: `014-workout-log-blocks`
**Created**: 2026-08-07
**Status**: Draft
**Input**: User description: "Rediseñar por completo la pantalla de Registro de un ejercicio dentro del Plan Mensual, sustituyendo el registro de valores sueltos por unidad (inputs numericos + lista de sets) por un modelo de Registrar entrenamiento basado en resumen rapido + estructura de bloques + notas, con historial de sesiones guardadas."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar una sesión de fuerza con resumen y bloques (Priority: P1)

Un usuario entra a un ejercicio de tipo fuerza (STRENGTH/BODYWEIGHT/PLYOMETRIC/REHABILITATION) planificado en una semana/día concretos, revisa el resumen rápido (Duración, Volumen total, Ejercicios/bloques), añade o edita bloques de su entrenamiento (Calentamiento, Aproximación, Trabajo, Descanso) y guarda la sesión con notas opcionales.

**Why this priority**: Es el flujo principal que sustituye por completo la funcionalidad actual de registro y aporta el valor central del feature.

**Independent Test**: Puede probarse entrando a un ejercicio de fuerza planificado, registrando resumen + al menos un bloque + notas, pulsando "Guardar entrenamiento" y verificando que la sesión aparece en el Historial.

**Acceptance Scenarios**:

1. **Given** un ejercicio de fuerza planificado sin sesiones previas, **When** el usuario abre el tab "Registrar", **Then** ve la tarjeta de resumen rápido con las 3 métricas de fuerza y la estructura de bloques vacía con opciones "Ver plantillas" y "+ Añadir bloque".
2. **Given** el formulario de añadir bloque abierto, **When** el usuario selecciona un tipo de bloque de fuerza, indica nombre y duración y confirma, **Then** el bloque aparece en la estructura del entrenamiento con su icono, nombre, duración y descripción.
3. **Given** resumen, bloques y notas completados, **When** el usuario pulsa "Guardar entrenamiento", **Then** la sesión se persiste asociada a la semana/día/ejercicio planificado y queda disponible en el Historial.

---

### User Story 2 - Registrar una sesión de cardio/nado con ritmo objetivo (Priority: P1)

Un usuario entra a un ejercicio de tipo cardio (CARDIO/MOBILITY/STRETCHING/SPORTS) planificado, revisa el resumen rápido (Duración, Calorías, Ritmo objetivo en escala RPE), añade bloques específicos (Nado, Series, Técnica, Descanso) indicando intensidad donde aplica, y guarda la sesión.

**Why this priority**: Es el segundo perfil de ejercicio explícitamente descrito en los mockups y garantiza que el modelo de bloques y resumen se adapta según `ExerciseType`.

**Independent Test**: Puede probarse entrando a un ejercicio de tipo cardio planificado, comprobando que las métricas y los tipos de bloque disponibles son distintos a los de fuerza, y guardando la sesión correctamente.

**Acceptance Scenarios**:

1. **Given** un ejercicio de cardio/nado planificado, **When** el usuario abre el tab "Registrar", **Then** ve como métricas de resumen Duración, Calorías y Ritmo objetivo (con etiqueta de intensidad tipo "Cómodo"/"Moderado").
2. **Given** el formulario de añadir bloque para un ejercicio de cardio, **When** el usuario selecciona un tipo de bloque que soporta intensidad, **Then** el campo "Intensidad (RPE)" se muestra; si selecciona un tipo que no la soporta, el campo no se muestra.

---

### User Story 3 - Editar el objetivo del resumen rápido (Priority: P2)

Un usuario quiere ajustar antes o durante el entrenamiento los 3 valores del resumen rápido (por ejemplo, la duración planeada o el volumen esperado) sin tener que editar los bloques.

**Why this priority**: Es una funcionalidad claramente solicitada en los mockups, pero secundaria respecto al flujo principal de registrar bloques y guardar.

**Independent Test**: Puede probarse pulsando "Editar objetivo", modificando los 3 valores en el panel, pulsando "Guardar cambios" y comprobando que la tarjeta de resumen rápido refleja los nuevos valores.

**Acceptance Scenarios**:

1. **Given** la tarjeta de resumen rápido visible, **When** el usuario pulsa "Editar objetivo", **Then** se abre un panel con los 3 campos editables junto con su unidad correspondiente.
2. **Given** el panel de edición de resumen abierto, **When** el usuario cambia valores y pulsa "Guardar cambios", **Then** los valores nuevos se reflejan inmediatamente en la tarjeta de resumen sin necesidad de guardar la sesión completa.

---

### User Story 4 - Cargar una plantilla de bloques predefinida (Priority: P2)

Un usuario que no quiere construir la estructura de bloques manualmente pulsa "Ver plantillas" y carga una estructura predefinida adecuada al tipo de ejercicio, pudiendo luego ajustarla.

**Why this priority**: Acelera el registro y reduce fricción, pero el feature sigue siendo funcional sin plantillas (creación manual de bloques).

**Independent Test**: Puede probarse pulsando "Ver plantillas" en un ejercicio sin bloques y verificando que se carga una estructura de bloques por defecto acorde al tipo de ejercicio (fuerza o cardio).

**Acceptance Scenarios**:

1. **Given** un ejercicio de fuerza sin bloques, **When** el usuario pulsa "Ver plantillas" y elige una plantilla, **Then** la estructura del entrenamiento se rellena con los bloques típicos de fuerza (Calentamiento, Series de aproximación, Parte principal, Vuelta a la calma).
2. **Given** un ejercicio con bloques ya creados, **When** el usuario pulsa "Ver plantillas", **Then** el sistema pide confirmación antes de reemplazar los bloques existentes.

---

### User Story 5 - Consultar el historial de sesiones de un ejercicio planificado (Priority: P2)

Un usuario quiere revisar sesiones de entrenamiento guardadas previamente para ese mismo ejercicio planificado (misma semana/día del plan, a lo largo de distintos meses reales), viendo su resumen y sus bloques.

**Why this priority**: Necesario para dar continuidad y valor histórico al registro, pero depende de que existan sesiones guardadas (User Story 1/2).

**Independent Test**: Puede probarse guardando dos sesiones en momentos distintos para el mismo ejercicio planificado y comprobando que ambas aparecen en el tab "Historial", ordenadas por fecha, cada una con su resumen y bloques.

**Acceptance Scenarios**:

1. **Given** un ejercicio planificado sin sesiones guardadas, **When** el usuario abre el tab "Historial", **Then** ve un estado vacío indicando que no hay sesiones registradas.
2. **Given** varias sesiones guardadas para el mismo ejercicio planificado, **When** el usuario abre el tab "Historial", **Then** ve las sesiones ordenadas de más reciente a más antigua, cada una mostrando al menos sus 3 métricas de resumen y sus bloques.

---

### Edge Cases

- El usuario intenta guardar un bloque sin nombre o sin duración: el sistema bloquea la confirmación y señala los campos requeridos.
- El usuario escribe una descripción de bloque o una nota que supera el máximo de caracteres (100 y 150 respectivamente): el sistema impide seguir escribiendo y el contador nunca muestra un valor negativo.
- El usuario pulsa "Guardar entrenamiento" sin haber añadido ningún bloque: el sistema permite guardar la sesión solo con resumen y notas (los bloques son opcionales).
- El usuario cambia el tipo de un bloque que tenía intensidad (RPE) informada a un tipo que no admite intensidad: el valor de intensidad se descarta para ese bloque.
- El usuario pulsa "Ver plantillas" teniendo ya bloques creados: se solicita confirmación antes de sustituir la estructura actual.
- El mismo ejercicio planificado (misma semana/día) se registra en distintos meses reales: cada guardado genera una entrada de historial independiente, no se sobrescribe la anterior.
- El usuario no tiene conexión o la petición de guardado falla: se muestra un error y los datos introducidos permanecen en el formulario sin perderse.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE sustituir, dentro del tab "Registrar" de un ejercicio planificado, los inputs numéricos por unidad y la lista de "sets" en memoria por un modelo de sesión de entrenamiento compuesto por resumen rápido, estructura de bloques y notas.
- **FR-002**: El tab anteriormente llamado "Registro" DEBE renombrarse a "Registrar", manteniendo el tab "Historial".
- **FR-003**: El sistema DEBE mostrar una tarjeta "Resumen rápido" con 3 métricas (icono, valor grande, etiqueta), cuyo contenido depende del `ExerciseType` del ejercicio: perfil "fuerza" (Duración en minutos, Volumen total en kg, número de Ejercicios/bloques) para STRENGTH, BODYWEIGHT, PLYOMETRIC y REHABILITATION; perfil "cardio" (Duración en minutos, Calorías aproximadas, Ritmo objetivo en escala RPE 1-10 con etiqueta) para CARDIO, MOBILITY, STRETCHING y SPORTS.
- **FR-004**: El sistema DEBE ofrecer una acción "Editar objetivo" que abra un panel para modificar los 3 valores del resumen rápido (con su unidad correspondiente) de forma independiente al guardado de la sesión completa, mediante un botón "Guardar cambios".
- **FR-005**: El sistema DEBE mostrar la "Estructura del entrenamiento" como una lista ordenada de bloques, cada uno con icono/color según su tipo, duración en minutos, nombre, descripción corta, y ser clicable para editarlo.
- **FR-006**: El sistema DEBE ofrecer un botón "Ver plantillas" que cargue una estructura de bloques predefinida según el perfil (fuerza o cardio) del `ExerciseType` del ejercicio, solicitando confirmación si ya existen bloques creados.
- **FR-007**: El sistema DEBE ofrecer un botón "+ Añadir bloque" que abra un formulario para crear un bloque nuevo.
- **FR-008**: El formulario de bloque DEBE incluir: selector de "Tipo de bloque" en formato grid de botones con icono (perfil fuerza: Calentamiento/Aproximación/Trabajo/Descanso; perfil cardio: Nado/Series/Técnica/Descanso), campo "Nombre del bloque" (texto, obligatorio), campo "Duración" (número + unidad de tiempo), campo "Descripción" opcional (texto libre, máximo 100 caracteres con contador), y campo "Intensidad (RPE)" (select Suave/Moderado/Alto...) visible únicamente para los tipos de bloque del perfil cardio donde aplica.
- **FR-009**: El sistema DEBE validar que tipo de bloque, nombre y duración (mayor que cero) estén informados antes de permitir confirmar "Añadir bloque".
- **FR-010**: El sistema DEBE permitir editar un bloque existente reutilizando el mismo formulario, precargado con sus valores actuales.
- **FR-011**: El sistema DEBE permitir eliminar un bloque de la estructura del entrenamiento.
- **FR-012**: El sistema DEBE mostrar, debajo del listado de bloques, una sección "Notas (opcional)" con un textarea libre de máximo 150 caracteres y contador de caracteres restantes.
- **FR-013**: El sistema DEBE mostrar, fijos en la parte inferior de la pantalla y siempre visibles sin desplazarse con el scroll, un botón principal "Guardar entrenamiento" y un botón secundario circular de acceso rápido al Historial.
- **FR-014**: Al pulsar "Guardar entrenamiento", el sistema DEBE persistir el resumen rápido, los bloques y las notas como una sesión de entrenamiento asociada a la semana/día/ejercicio planificado y al usuario actual, con marca de fecha/hora.
- **FR-015**: El sistema DEBE permitir que existan varias sesiones de entrenamiento guardadas para la misma combinación semana/día/ejercicio planificado a lo largo del tiempo (repeticiones del plan en distintos meses reales), sin sobrescribir sesiones anteriores.
- **FR-016**: El tab "Historial" DEBE listar las sesiones guardadas de ese ejercicio planificado ordenadas de más reciente a más antigua, mostrando para cada una al menos sus 3 métricas de resumen y sus bloques.
- **FR-017**: El tab "Historial" DEBE mostrar un estado vacío cuando el ejercicio planificado no tiene sesiones guardadas todavía.
- **FR-018**: Todo el texto nuevo de la interfaz DEBE estar disponible en español e inglés a través del sistema de i18n existente (es.ts/en.ts), sin cadenas de texto embebidas en el código.
- **FR-019**: El sistema DEBE eliminar por completo el código obsoleto de `ExerciseDetail.tsx` relativo al modelo anterior (inputs por unidad, botón "Añadir set", lista de sets en memoria y sus tipos/funciones auxiliares), sin dejar código muerto ni comentado.
- **FR-020**: El sistema DEBE incluir un bloque informativo estático "Autogestión eficiente" con una lista corta de consejos, con contenido puramente presentacional obtenido de i18n, sin lógica de negocio.
- **FR-021**: La cabecera del ejercicio (icono/color por tipo, nombre, badge "Plan mensual", botón volver) DEBE mantenerse sin cambios.
- **FR-022**: El sistema DEBE mantener sin cambios la entidad `TrainingLog` existente y el flujo de registro de métricas de entrenamiento del módulo de ejercicios (`routines/exercices`), por quedar fuera del alcance de este cambio.
- **FR-023**: El sistema DEBE limitar el acceso a cada sesión de entrenamiento guardada al usuario propietario de la misma.
- **FR-024**: Los campos numéricos de duración (en resumen y en bloques) DEBEN aceptar únicamente valores no negativos.
- **FR-025**: En pantallas anchas, el sistema DEBE permitir mostrar el panel "Editar resumen rápido" y el panel de añadir/editar bloque de forma independiente entre sí (uno u otro según la acción del usuario, reutilizando los patrones de panel/diálogo ya existentes en el proyecto).

### Key Entities *(include if feature involves data)*

- **TrainingSession**: Representa una sesión de entrenamiento guardada por el usuario para un ejercicio planificado concreto (semana, día, ejercicio) en un momento dado. Incluye el resumen rápido (3 métricas con su valor y unidad), la lista ordenada de bloques y las notas opcionales. Sustituye, para esta pantalla, al registro disperso por unidad que ofrecía `TrainingLog`.
- **SessionBlock**: Bloque individual de la estructura de una `TrainingSession`. Incluye tipo de bloque, nombre, duración (valor + unidad de tiempo), descripción opcional, intensidad (RPE) opcional y orden dentro de la sesión.
- **BlockTemplate**: Plantilla de referencia estática con una estructura de bloques predefinida por perfil de `ExerciseType` (fuerza o cardio), usada para precargar bloques al pulsar "Ver plantillas". No es editable por el usuario en esta iteración.
- **PlannedExercise** *(existente, reutilizado)*: Ejercicio planificado en una semana/día concretos del Plan Mensual; determina a qué `TrainingSession` pertenece cada registro.
- **Exercise / ExerciseType / Unit** *(existentes, reutilizados)*: Catálogo de ejercicios y su tipo, que determina qué perfil de resumen y qué tipos de bloque están disponibles.
- **TrainingLog** *(existente, sin cambios)*: Se mantiene tal cual para el flujo de registro de métricas por unidad del módulo de ejercicios (`routines/exercices`); no se usa en el nuevo modelo de "Registrar entrenamiento" del Plan Mensual.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema DEBE tratar el catálogo de Exercise, ExerciseType y Unit como datos de referencia ya existentes (gestionados por los módulos de administración/catálogo); este feature no introduce nuevos proveedores externos.
- **INT-002**: Las sesiones de entrenamiento (`TrainingSession`, `SessionBlock`) DEBEN almacenarse como datos transaccionales propiedad del usuario en la base de datos principal del proyecto, de igual forma que hoy se almacena `TrainingLog`.
- **INT-003**: El sistema no introduce nuevas dependencias de proveedores externos; si el catálogo de ejercicios/unidades no está disponible temporalmente, la pantalla de Registrar DEBE mostrar el mismo estado vacío/error ya utilizado en el resto del Plan Mensual.
- **INT-004**: Las `BlockTemplate` DEBEN ser contenido estático interno sin identificadores externos, indexado únicamente por el perfil de `ExerciseType`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un usuario puede registrar una sesión completa (resumen + hasta 5 bloques + notas) en menos de 2 minutos.
- **SC-002**: El 100% de los tipos de ejercicio soportados tienen un perfil de resumen y una plantilla de bloques por defecto disponibles.
- **SC-003**: Un usuario puede localizar y revisar cualquier sesión previamente guardada de un ejercicio planificado (resumen + bloques) desde el Historial sin pasos adicionales fuera del tab.
- **SC-004**: El 90% de los usuarios de prueba consiguen añadir un bloque y guardar una sesión de entrenamiento sin ayuda externa en su primer intento.
- **SC-005**: Ningún dato introducido en el formulario de resumen, bloques o notas se pierde si el guardado falla por un error de red (los datos permanecen visibles para reintentar).

## Assumptions

- El registro detallado por set/unidad (reps, peso, tiempo por serie) queda sustituido por un registro más agregado (resumen + estructura de bloques); esta pantalla ya no ofrece captura fina por serie individual.
- Mapeo de `ExerciseType` a perfil de resumen/bloques: STRENGTH, BODYWEIGHT, PLYOMETRIC y REHABILITATION usan el perfil "fuerza"; CARDIO, MOBILITY, STRETCHING y SPORTS usan el perfil "cardio".
- Las plantillas de bloques (`BlockTemplate`) son contenido estático de referencia por perfil, no gestionable desde un panel de administración en esta iteración.
- La escala de Intensidad (RPE) usa 10 niveles agrupados en etiquetas estándar (p. ej. Muy suave, Cómodo, Moderado, Alto, Máximo).
- Una sesión de entrenamiento puede guardarse sin ningún bloque (los bloques son opcionales; resumen y notas bastan).
- Cada pulsación de "Guardar entrenamiento" crea una nueva entrada de historial; no se sobrescribe silenciosamente una sesión previa de otro momento.
- La autenticación/sesión de usuario ya existente en el proyecto determina el propietario de cada `TrainingSession`.
- El módulo `routines/exercices` (Training Metric Logs sobre `TrainingLog`) permanece intacto y fuera de alcance de este cambio.
