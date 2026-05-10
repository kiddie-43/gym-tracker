# Feature Specification: App de Seguimiento Fitness y Progreso

**Feature Branch**: `[001-track-fitness-progress]`  
**Created**: 2026-05-05  
**Status**: Draft  
**Input**: User description: "Crear una app de seguimiento fitness donde cada usuario pueda registrar entrenamientos y comidas, comparar su rendimiento actual contra entrenamientos recientes, crear rutinas de entrenamiento y dietas personalizadas por días, y opcionalmente llevar control de calorías."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar y comparar entrenamientos (Priority: P1)

Como usuario autenticado, quiero registrar cada entrenamiento con sus ejercicios y ver una comparación inmediata contra mis sesiones equivalentes recientes para saber si estoy mejorando, manteniéndome o empeorando.

**Why this priority**: Es el núcleo del producto y la propuesta de valor principal. Sin esta capacidad la aplicación solo sería un registro estático sin feedback de progreso.

**Independent Test**: Puede probarse registrando un ejercicio en dos sesiones distintas y verificando que el sistema muestra comparación contra la última sesión, promedio reciente y mejor marca con métricas coherentes.

**Acceptance Scenarios**:

1. **Given** que el usuario ya registró al menos una sesión previa del mismo ejercicio, **When** guarda una nueva sesión completa, **Then** el sistema muestra la comparación contra la última sesión equivalente, el promedio de las últimas 4 y la mejor marca reciente.
2. **Given** que el usuario registra una sesión de un ejercicio sin historial suficiente, **When** consulta el resultado, **Then** el sistema muestra el estado "sin referencia" sin inventar comparativas.
3. **Given** que una sesión queda marcada como incompleta, **When** el usuario consulta su progreso, **Then** el sistema distingue esa condición y evita comparaciones engañosas.

---

### User Story 2 - Crear y reutilizar rutinas (Priority: P2)

Como usuario, quiero crear rutinas por día o tipo de entrenamiento usando grupos musculares y ejercicios sugeridos para organizar mejor mis sesiones futuras y reutilizar estructuras repetidas.

**Why this priority**: Aumenta la utilidad diaria del producto y reduce el esfuerzo de registrar entrenamientos desde cero, pero depende menos del valor principal que la comparación de progreso.

**Independent Test**: Puede probarse creando una rutina con nombre, día, grupos musculares y ejercicios sugeridos, guardándola y reutilizándola como base de un entrenamiento real.

**Acceptance Scenarios**:

1. **Given** que el usuario selecciona grupos musculares en una rutina, **When** avanza al selector de ejercicios, **Then** el sistema muestra ejercicios relacionados con esos grupos desde el catálogo externo.
2. **Given** que el usuario tiene una rutina guardada, **When** decide iniciar un entrenamiento desde ella, **Then** el sistema precarga los ejercicios planificados para facilitar el registro.
3. **Given** que el usuario quiere modificar una rutina existente, **When** la edita y la guarda, **Then** los cambios quedan reflejados sin alterar entrenamientos históricos ya registrados.

---

### User Story 3 - Crear dietas diarias y control calórico opcional (Priority: P3)

Como usuario, quiero planificar mis comidas por días y franjas horarias, con la opción de llevar o no control de calorías, para adaptar la app a mi nivel de detalle deseado.

**Why this priority**: Complementa el seguimiento fitness con planificación nutricional, pero la app sigue siendo valiosa aunque esta parte llegue después del núcleo de entrenamiento.

**Independent Test**: Puede probarse creando una dieta semanal con alimentos por desayuno/comida/merienda/cena, activando y desactivando el control de calorías y verificando que la app cambia su comportamiento sin romper la planificación.

**Acceptance Scenarios**:

1. **Given** que el usuario crea una dieta para varios días, **When** añade alimentos a una franja concreta, **Then** el sistema guarda esa planificación por día y tipo de comida.
2. **Given** que el usuario activa el seguimiento calórico, **When** consulta un día con datos suficientes, **Then** el sistema muestra calorías objetivo y calorías consumidas diferenciando datos conocidos de no informados.
3. **Given** que el usuario desactiva el seguimiento calórico, **When** registra o consulta sus comidas, **Then** la app no exige calorías ni muestra métricas calóricas como obligatorias.

---

### User Story 4 - Consultar historial y estado degradado controlado (Priority: P4)

Como usuario, quiero consultar mi historial de entrenamientos, comidas, rutinas y dietas incluso cuando el catálogo externo tenga incidencias, para no perder acceso a mi información ni bloquear el uso básico de la app.

**Why this priority**: Mejora confiabilidad y continuidad del uso, pero depende de que ya exista información relevante almacenada.

**Independent Test**: Puede probarse consultando historial con catálogos cargados y repitiendo la consulta durante una caída simulada del proveedor externo para verificar caché y mensajes de degradación.

**Acceptance Scenarios**:

1. **Given** que el proveedor externo no responde temporalmente, **When** el usuario abre rutinas, dietas o formularios con entidades ya cacheadas, **Then** el sistema sigue mostrando datos disponibles e informa que opera en modo degradado.
2. **Given** que el usuario consulta su historial por ejercicio o fechas, **When** existen registros guardados, **Then** la app devuelve los datos personales sin depender del proveedor externo para reconstruirlos.

### Edge Cases

- Usuario sin historial previo para un ejercicio o sin sesiones equivalentes suficientes.
- Sesiones incompletas o con datos parciales que no deben contaminar comparativas.
- Usuario sin rutinas creadas o sin dietas creadas.
- Ejercicios sin imagen disponible en el catálogo externo.
- Alimentos o ejercicios temporalmente no disponibles o renombrados por el proveedor externo.
- Duplicación accidental de rutinas, dietas, comidas o entrenamientos.
- Usuario con seguimiento calórico desactivado.
- Usuario con seguimiento calórico activado pero con datos incompletos o calorías no informadas.
- Días con comidas registradas pero sin información suficiente para un total calórico concluyente.
- Caída temporal del proveedor externo mientras el usuario intenta crear o editar rutinas y dietas.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir a cada usuario autenticado registrar entrenamientos con fecha, hora, ejercicios, series, repeticiones, peso, descanso y notas.
- **FR-002**: El sistema MUST permitir marcar un entrenamiento como completo o incompleto y reflejar ese estado en las comparativas posteriores.
- **FR-003**: El sistema MUST comparar cada ejercicio registrado contra la última sesión equivalente, el promedio de las últimas 4 sesiones equivalentes y la mejor marca reciente cuando exista historial suficiente.
- **FR-004**: El sistema MUST calcular y mostrar para cada comparación volumen total, carga total, repeticiones totales, variación porcentual y tendencia.
- **FR-005**: El sistema MUST evitar comparar ejercicios distintos entre sí y MUST mostrar el estado "sin referencia" cuando no existan datos suficientes.
- **FR-006**: Los usuarios MUST poder consultar resúmenes semanales y mensuales de progreso basados en sus registros históricos.
- **FR-007**: El sistema MUST permitir crear, editar, duplicar, eliminar y reutilizar rutinas de entrenamiento personalizadas por día o etiqueta.
- **FR-008**: El sistema MUST permitir seleccionar varios grupos musculares en una rutina y mostrar ejercicios relacionados desde el catálogo externo según esa selección.
- **FR-009**: El sistema MUST permitir usar una rutina guardada como base para registrar un entrenamiento real.
- **FR-010**: El sistema MUST permitir crear, editar, duplicar, eliminar y reutilizar dietas o planes de comidas personalizados por días.
- **FR-011**: El sistema MUST permitir planificar alimentos por desayuno, comida, merienda y cena para cada día de una dieta.
- **FR-012**: El sistema MUST permitir registrar comidas consumidas por fecha y asociarlas a alimentos del catálogo externo cuando existan.
- **FR-013**: El sistema MUST permitir que el usuario active o desactive el seguimiento calórico como preferencia personal.
- **FR-014**: Si el seguimiento calórico está activado, el sistema MUST permitir definir un objetivo diario de calorías y mostrar calorías consumidas solo cuando existan datos suficientes y explícitos.
- **FR-015**: Si el seguimiento calórico está desactivado, el sistema MUST permitir usar planificación y registro de comidas sin requerir calorías ni mostrar métricas calóricas como obligatorias.
- **FR-016**: El sistema MUST diferenciar entre calorías conocidas, calorías no informadas y seguimiento calórico desactivado para evitar interpretaciones incorrectas.
- **FR-017**: El sistema MUST mostrar imágenes de ejercicios cuando estén disponibles desde el catálogo externo.
- **FR-018**: El sistema MUST permitir consultar historial de entrenamientos y comidas por rango de fechas y por entidad relevante, como ejercicio o día.
- **FR-019**: El sistema MUST almacenar rutinas, dietas, entrenamientos, comidas, progreso y preferencias como datos personales del usuario.
- **FR-020**: El sistema MUST operar en modo degradado controlado cuando el proveedor externo falle, utilizando datos cacheados cuando existan y comunicando claramente la limitación al usuario.
- **FR-021**: El sistema MUST mantener trazabilidad al identificador externo de ejercicios y alimentos importados desde el catálogo.
- **FR-022**: El sistema MUST prevenir cálculos calóricos incorrectos cuando un alimento no tenga calorías explícitas y confiables.
- **FR-023**: El sistema MUST mostrar alertas simples cuando detecte regresión sostenida en el rendimiento del usuario.

### Key Entities *(include if feature involves data)*

- **Usuario**: Persona autenticada que posee registros, rutinas, dietas, preferencias de seguimiento calórico y progreso histórico.
- **Entrenamiento**: Sesión registrada con fecha, estado, notas y una colección de ejercicios realizados.
- **Ejercicio Realizado**: Registro individual dentro de un entrenamiento con referencia a ejercicio externo, series, repeticiones, peso, descanso y métricas derivadas.
- **Comparativa de Progreso**: Resultado calculado que resume diferencias entre un ejercicio actual y sus referencias históricas equivalentes.
- **Rutina**: Plantilla personal de entrenamiento con nombre, día o etiqueta, grupos musculares objetivo y ejercicios planificados.
- **Dieta**: Plan personal de comidas organizado por días y franjas como desayuno, comida, merienda y cena.
- **Comida Registrada**: Consumo real del usuario en una fecha concreta, compuesto por alimentos, cantidades y datos nutricionales disponibles.
- **Preferencia Calórica**: Configuración del usuario que define si lleva control de calorías y, en su caso, el objetivo diario.
- **Catálogo Externo**: Conjunto de ejercicios, imágenes y alimentos provenientes de Wger y referenciados desde entidades propias.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El sistema MUST tratar Wger como fuente externa de catálogo para ejercicios, imágenes de ejercicios, alimentos y metadatos asociados.
- **INT-002**: El sistema MUST almacenar en Firebase todos los registros transaccionales del usuario, incluyendo entrenamientos, comidas, rutinas, dietas, comparativas y preferencias.
- **INT-003**: El sistema MUST definir fallback ante fallos de Wger mediante caché, política de lectura de datos ya conocidos y mensaje de estado degradado al usuario.
- **INT-004**: El sistema MUST preservar trazabilidad hacia IDs externos de Wger para ejercicios y alimentos importados.
- **INT-005**: El sistema MUST permitir consultar historial personal aunque Wger no esté disponible, sin reconstruir datos del usuario desde el proveedor externo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Los usuarios pueden registrar una sesión completa de entrenamiento en menos de 2 minutos en condiciones normales de uso.
- **SC-002**: Los usuarios pueden crear una rutina o una dieta diaria en menos de 3 minutos sin ayuda externa.
- **SC-003**: Las comparativas por ejercicio se muestran en menos de 3 segundos para consultas habituales del historial personal.
- **SC-004**: Al menos el 95% de las comparativas generadas coinciden con los datos registrados sin errores de cálculo detectables por el usuario.
- **SC-005**: El usuario puede identificar su tendencia semanal de progreso sin realizar cálculos manuales adicionales.
- **SC-006**: La aplicación sigue permitiendo consultar datos personales previamente almacenados durante incidencias temporales del proveedor externo.

## Assumptions

- Los usuarios accederán autenticados antes de registrar o consultar información personal.
- La primera versión se enfocará en entrenamientos de fuerza y planificación básica de comidas.
- Las comparativas se realizarán entre sesiones del mismo ejercicio en un contexto equivalente de uso.
- Las rutinas y dietas serán privadas por usuario y no se compartirán en la primera versión.
- Cuando falte información calórica explícita y confiable, el sistema priorizará mostrar ausencia de dato en lugar de estimaciones automáticas.
