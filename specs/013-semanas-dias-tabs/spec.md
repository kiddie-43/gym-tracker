# Feature Specification: Configuracion Semanas y Dias

**Feature Branch**: `013-replace-semanas-dias-tabs`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "ha habido un cambio de planes ahora hay un paso que es rutinas > sessiones > ejercicios > detalles de ejercicio, ahora la seccion de session y rutina van a desaparecer y pasara a ser semanas > dias el estilo sera una tab que pone semana 1 ... semana 4 y de dias sera dia 1 a 7 esto segun lo configure el usuario luego dentro de estas pestañas ya podra configurar los ejercicios, las 4 semanas seran fijas que es lo que dura un mes, y luego los dias son configurables, quiero dos tabs una para las semanas dentro de cada tab habra otra tab configurada por el usuario"

## Clarifications

### Session 2026-07-27

- Q: El numero de dias configurables (1..7) debe ser igual para todas las semanas o independiente por semana? → A: Un unico N de dias (1..7) aplicado igual a Semana 1..4.
- Q: Que pasa con ejercicios en dias fuera de rango al reducir N? → A: Se vacia la planificacion fuera de rango, pero se mantienen los registros historicos para estadisticas de entrenamiento.
- Q: Como tratar la compatibilidad con datos historicos de Rutina/Sesion al pasar a Semana/Dia? → A: Migracion dura en una sola entrega, convirtiendo historial a Semana/Dia y retirando campos anteriores.
- Q: Quien puede crear y editar la configuracion Semana > Dia? → A: Solo el propietario del plan.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plan mensual por semanas (Priority: P1)

Como usuario quiero estructurar mi plan de entrenamiento mensual en 4 semanas fijas para organizar mis ejercicios con una vista clara y predecible.

**Why this priority**: Define la base del nuevo flujo; sin este cambio no existe la nueva estructura de planificacion mensual.

**Independent Test**: Puede validarse creando o editando un plan y verificando que siempre aparecen 4 pestañas de semana disponibles para configuracion.

**Acceptance Scenarios**:

1. **Given** un usuario entra al flujo de planificacion, **When** abre el configurador principal, **Then** visualiza exactamente 4 pestañas de semana (Semana 1, Semana 2, Semana 3, Semana 4).
2. **Given** un usuario cambia entre semanas, **When** selecciona una pestaña de semana distinta, **Then** el sistema muestra el contenido de configuracion de esa semana sin perder cambios guardados previamente.

---

### User Story 2 - Configurar dias del mes (Priority: P1)

Como usuario quiero definir un unico numero de dias de entrenamiento para todo el mes para adaptar el plan a mi disponibilidad real.

**Why this priority**: Es la capacidad de personalizacion principal pedida en el cambio de enfoque.

**Independent Test**: Puede validarse configurando una cantidad de dias y comprobando que se crean y muestran solo las pestañas de dia esperadas dentro de cada semana.

**Acceptance Scenarios**:

1. **Given** un usuario define una cantidad de dias semanales entre 1 y 7, **When** guarda la configuracion, **Then** las 4 semanas muestran pestañas de dia desde Dia 1 hasta el mismo numero configurado.
2. **Given** un usuario reduce la cantidad de dias configurados, **When** confirma el cambio, **Then** el sistema conserva solo los dias dentro del nuevo limite y evita mostrar dias fuera de rango.

---

### User Story 3 - Configurar ejercicios dentro de cada dia (Priority: P2)

Como usuario quiero gestionar ejercicios dentro de cada dia de cada semana para completar mi rutina mensual sin navegar por secciones separadas de rutina y sesion.

**Why this priority**: Materializa el objetivo funcional final del cambio de estructura y elimina pasos intermedios del flujo anterior.

**Independent Test**: Puede validarse seleccionando una semana y un dia, agregando ejercicios y verificando que los ejercicios quedan asociados al contexto correcto.

**Acceptance Scenarios**:

1. **Given** un usuario esta en Semana 2 y Dia 3, **When** agrega o edita ejercicios, **Then** los cambios quedan asociados a Semana 2 > Dia 3.
2. **Given** un usuario navega entre distintos dias y semanas, **When** regresa a un dia ya configurado, **Then** visualiza los ejercicios previamente definidos en ese mismo contexto.

---

### Edge Cases

- Cuando el usuario intenta configurar 0 dias o mas de 7 dias, el sistema debe rechazar el valor y mantener una configuracion valida.
- Cuando el usuario cambia la cantidad de dias y existen ejercicios en dias que quedan fuera de rango, el sistema debe solicitar confirmacion, vaciar esos dias del plan activo y conservar su historico para estadisticas.
- Cuando no existen ejercicios configurados en un dia, la vista del dia debe mostrarse vacia y lista para configuracion sin errores.
- Cuando el usuario cambia rapidamente entre pestañas de semanas y dias, la navegacion debe mantener consistencia del contexto activo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST reemplazar la estructura de navegacion basada en Rutinas y Sesiones por una estructura basada en Semanas y Dias para la planificacion mensual.
- **FR-002**: El sistema MUST mostrar exactamente 4 semanas fijas en el flujo de planificacion, etiquetadas de Semana 1 a Semana 4.
- **FR-003**: El sistema MUST mostrar dentro de cada semana una segunda capa de pestañas de dias.
- **FR-004**: El sistema MUST permitir al usuario configurar un unico numero de dias activos (1 a 7) para todo el mes.
- **FR-005**: El sistema MUST generar automaticamente las pestañas de Dia 1 hasta Dia N segun la cantidad de dias configurada por el usuario.
- **FR-006**: El sistema MUST aplicar exactamente el mismo numero de dias configurado en Semana 1, Semana 2, Semana 3 y Semana 4.
- **FR-007**: El sistema MUST permitir configurar ejercicios dentro de cada combinacion Semana > Dia.
- **FR-008**: El sistema MUST mantener la asociacion de ejercicios al contexto exacto de semana y dia donde fueron configurados.
- **FR-009**: El sistema MUST preservar los cambios guardados al navegar entre semanas y dias.
- **FR-010**: El sistema MUST validar que no se puedan establecer cantidades de dias fuera del rango permitido.
- **FR-011**: El sistema MUST solicitar confirmacion explicita cuando un cambio de cantidad de dias pueda excluir dias que ya contienen ejercicios configurados.
- **FR-015**: El sistema MUST vaciar del plan activo los ejercicios en dias fuera de rango al reducir N, sin eliminarlos de los registros historicos usados para estadisticas.
- **FR-012**: El sistema MUST retirar de la interfaz del flujo mensual (vista principal de planificacion) las secciones visibles de Rutina y Sesion, sustituyendolas por la navegacion Semana > Dia.
- **FR-013**: El sistema MUST ofrecer estado vacio claro para dias sin ejercicios configurados.
- **FR-014**: El sistema MUST mantener trazabilidad funcional de la configuracion mensual resultante para que cada ejercicio pueda identificarse por semana y dia.
- **FR-016**: El sistema MUST ejecutar una migracion unica que convierta los datos historicos de Rutina/Sesion a Semana/Dia antes de habilitar el nuevo flujo en produccion.
- **FR-017**: El sistema MUST retirar en la misma entrega las dependencias funcionales activas de Rutina/Sesion dentro del flujo de planificacion mensual, sin requerir la eliminacion global de esos modulos fuera de dicho flujo.
- **FR-018**: El sistema MUST permitir crear y editar la configuracion Semana > Dia exclusivamente al propietario del plan.
- **FR-019**: El sistema MUST rechazar con error de autorizacion cualquier intento de acceso o modificacion de configuraciones de otro usuario.

### Key Entities *(include if feature involves data)*

- **PlanMensual**: Configuracion de entrenamiento de un mes compuesta por 4 semanas fijas.
- **SemanaPlan**: Unidad semanal del plan mensual identificada por posicion (1 a 4).
- **DiaPlan**: Unidad diaria dentro de una semana, identificada por posicion (1 a N, donde N es configurable entre 1 y 7).
- **ConfiguracionDiasSemanales**: Regla global definida por el usuario que determina un unico numero de dias habilitados para las 4 semanas.
- **BloqueEjerciciosDia**: Conjunto de ejercicios asignados a una combinacion concreta de semana y dia.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema MUST distinguir entre configuracion estructural del plan (semanas y dias) y contenido transaccional de ejercicios dentro de cada dia.
- **INT-002**: El sistema MUST persistir la configuracion mensual y las asignaciones de ejercicios en la base de datos principal del proyecto.
- **INT-003**: El sistema MUST definir comportamiento de degradacion cuando falle la carga o guardado de la configuracion, preservando el ultimo estado confirmado por el usuario.
- **INT-004**: El sistema MUST mantener identificadores trazables por semana y dia para permitir consulta y edicion posteriores sin ambiguedad.
- **INT-005**: El sistema MUST completar la migracion de datos sin ventana de compatibilidad entre esquemas dentro de la misma entrega.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de usuarios en pruebas funcionales visualiza 4 pestañas de semana fijas al entrar en el flujo de planificacion mensual.
- **SC-002**: El 100% de configuraciones validas de dias (1 a 7) se refleja correctamente en las pestañas de dia dentro de cada semana.
- **SC-003**: En pruebas de usabilidad controladas (n>=20), al menos el 95% de participantes completa la configuracion de un mes (semanas, dias y ejercicios) en <=5 minutos en su primer intento, medido desde la apertura del flujo mensual hasta el guardado exitoso.
- **SC-004**: El 100% de ejercicios guardados puede recuperarse en el mismo contexto Semana > Dia donde fue configurado.
- **SC-005**: El 100% de intentos con valores fuera de rango para dias semanales es rechazado con mensaje claro y sin corrupcion de la configuracion existente.
- **SC-006**: El 100% de cambios de dias que impliquen exclusion de contenido existente solicita confirmacion explicita antes de aplicar el cambio.
- **SC-007**: El 100% de reducciones de N vacia correctamente los dias fuera de rango en el plan activo y mantiene intactos los datos historicos de entrenamiento para estadisticas.
- **SC-008**: El 100% de registros historicos incluidos en el alcance de migracion queda convertido al esquema Semana > Dia y sin referencias activas a Rutina/Sesion en el flujo objetivo.
- **SC-009**: El 100% de intentos de crear o editar un plan por un usuario no propietario es rechazado correctamente en pruebas de autorizacion.

## Assumptions

- El numero de dias configurado se define una sola vez y se aplica de manera uniforme a todas las semanas del mes.
- El alcance de este cambio se centra en estructura de navegacion y configuracion; no redefine reglas internas de cada ejercicio.
- El flujo mantiene el objetivo de configuracion mensual y no contempla periodos de mas de 4 semanas en esta iteracion.
- Las operaciones de guardado y consulta de ejercicios reutilizan los mecanismos de persistencia ya existentes en el proyecto.
- La copia visible para el usuario final del flujo se presenta en espanol, alineada con el producto actual.
- La migracion de Rutina/Sesion a Semana/Dia se realiza en una sola entrega y no requiere coexistencia funcional entre ambos esquemas en este flujo.
- El sistema de autenticacion actual provee identificacion confiable del propietario del plan para aplicar las reglas de autorizacion definidas.
