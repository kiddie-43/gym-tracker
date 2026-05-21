# Feature Specification: Panel de Administración de Mediciones Configurable

**Feature Branch**: `010-create-feature-branch`  
**Created**: 2026-05-17  
**Status**: Draft  
**Input**: User description: "ahora mismo el panel de administracion de mediciones es muy ambiguo y nada claro, asi que necesito mejorarlo para que sea dinamico y se puedan configurar... estas mediciones seran para asignar a ejercicios, entonces es algo que se debera de poder gestionar, el administrador debera de poder subir un csv y crear las mediciones as como por un pop up, fijate en como esta el modulo de musculos y debe de parecerse a ese"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Crear y gestionar tipos de medición dinámicos (Priority: P1)

El administrador puede gestionar un catálogo de tipos de medición desde el panel de administración mediante un formulario emergente. Cada tipo de medición permite definir nombre, unidad, categoría, tipo de dato, descripción opcional y una clave de identificación única.

**Why this priority**: Es el flujo principal para eliminar la ambigüedad del panel actual y habilitar una configuración clara y mantenible.

**Independent Test**: Abrir el panel de mediciones, crear un tipo de medición desde el formulario emergente, editarlo y desactivarlo. El catálogo debe reflejar cada cambio sin requerir otras funcionalidades.

**Acceptance Scenarios**:

1. **Given** que el administrador accede al panel de mediciones, **When** abre el formulario emergente de alta, **Then** puede registrar un nuevo tipo de medición con todos los campos obligatorios.
2. **Given** que existe un tipo de medición activo, **When** el administrador lo edita desde el formulario emergente, **Then** los cambios quedan guardados y visibles en el listado.
3. **Given** que existe un tipo de medición activo, **When** el administrador lo desactiva, **Then** deja de aparecer en la vista activa del catálogo.
4. **Given** que el administrador usa el panel de mediciones, **When** navega por las acciones principales, **Then** encuentra una experiencia equivalente al módulo de músculos (acciones de añadir, importar CSV, refrescar, filtros y reactivación).

---

### User Story 2 - Importar tipos de medición por CSV (Priority: P1)

El administrador puede cargar un archivo CSV para crear múltiples tipos de medición en una sola operación, con un resultado detallado por fila.

**Why this priority**: Reduce esfuerzo manual, acelera la configuración inicial del catálogo y mejora la calidad de carga masiva de datos.

**Independent Test**: Subir un CSV con filas válidas e inválidas y verificar que se importan solo las válidas, mostrando detalle de errores de las rechazadas.

**Acceptance Scenarios**:

1. **Given** que el administrador selecciona un CSV con estructura válida, **When** confirma la importación, **Then** se crean los tipos de medición válidos y se muestra un resumen de resultados.
2. **Given** que el archivo contiene filas con datos inválidos, **When** finaliza la importación, **Then** cada fila inválida se reporta con su motivo de rechazo.
3. **Given** que el archivo contiene claves repetidas, **When** se procesa, **Then** las filas duplicadas no se crean y se informan como conflicto.

---

### User Story 3 - Usar mediciones en la asignación de ejercicios (Priority: P2)

El catálogo de tipos de medición se usa como fuente administrable para asignar mediciones a ejercicios, garantizando que solo se seleccionen tipos activos y válidos.

**Why this priority**: Asegura coherencia entre el catálogo administrado y la configuración de ejercicios, evitando asignaciones ambiguas.

**Independent Test**: Crear un nuevo tipo de medición y comprobar que queda disponible para asignación en ejercicios; desactivarlo y verificar que deja de estar disponible para nuevas asignaciones.

**Acceptance Scenarios**:

1. **Given** que existen tipos de medición activos, **When** el administrador configura un ejercicio, **Then** puede seleccionar esos tipos de medición para asignación.
2. **Given** que un tipo de medición se desactiva, **When** se intenta asignar en un nuevo ejercicio, **Then** ya no aparece como opción disponible.

---

### Edge Cases

- Importación de archivo sin columnas obligatorias del catálogo de mediciones.
- Importación con mezcla de filas válidas e inválidas.
- Intento de crear un tipo de medición con clave ya existente.
- Intento de usar categoría o tipo de dato fuera de los valores permitidos.
- Listado vacío de mediciones al iniciar el panel por primera vez.
- Navegación de paginación cuando el filtro reduce resultados y la página actual queda fuera de rango.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE mostrar un panel de mediciones con comportamiento claro y consistente con el patrón del panel de músculos.
- **FR-002**: El sistema DEBE permitir crear tipos de medición desde un formulario emergente.
- **FR-003**: El sistema DEBE permitir editar tipos de medición existentes desde el mismo flujo de formulario emergente.
- **FR-004**: El sistema DEBE permitir desactivar tipos de medición de forma reversible mediante estado lógico.
- **FR-005**: El sistema DEBE listar por defecto solo los tipos de medición activos.
- **FR-006**: El sistema DEBE permitir alternar entre vista de activos y vista con registros desactivados incluidos.
- **FR-007**: Cada tipo de medición DEBE incluir como mínimo: clave, nombre, unidad, tipo de dato y categoría.
- **FR-008**: El sistema DEBE admitir los tipos de dato permitidos: integer, decimal, time, boolean y text.
- **FR-009**: El sistema DEBE admitir las categorías permitidas: strength, cardio, mobility y general.
- **FR-010**: El sistema DEBE validar obligatoriedad y formato de campos antes de guardar un tipo de medición.
- **FR-011**: El sistema DEBE impedir el alta de claves duplicadas en el catálogo de mediciones.
- **FR-012**: El sistema DEBE permitir importar tipos de medición mediante archivo CSV.
- **FR-013**: La importación CSV DEBE procesar filas válidas e inválidas de manera parcial, sin cancelar toda la operación por errores puntuales.
- **FR-014**: El sistema DEBE informar un resultado por fila en la importación CSV, incluyendo motivo de rechazo cuando aplique.
- **FR-015**: Los tipos de medición activos DEBEN estar disponibles para asignación en ejercicios.
- **FR-016**: Los tipos de medición desactivados NO DEBEN aparecer para nuevas asignaciones en ejercicios.
- **FR-017**: El panel DEBE contemplar estados de carga, error con reintento y lista vacía.
- **FR-018**: El panel DEBE ofrecer acciones visibles y consistentes con el módulo de músculos: añadir por popup, importar CSV, refrescar listado, filtrar y gestionar reactivación.
- **FR-019**: El formulario emergente DEBE permitir crear y editar mediciones en un único flujo sin cambiar de pantalla.
- **FR-020**: El CSV DEBE aceptar al menos las columnas del catálogo de mediciones: `key`, `name`, `unit`, `dataType`, `category` y `description` (opcional).
- **FR-021**: El sistema DEBE validar que `dataType` pertenezca a {integer, decimal, time, boolean, text} y que `category` pertenezca a {strength, cardio, mobility, general} en alta manual y CSV.
- **FR-022**: El sistema DEBE impedir que registros desactivados se ofrezcan como opción de nuevas asignaciones en ejercicios, manteniendo trazabilidad de referencias históricas existentes.
- **FR-023**: El panel DEBE soportar paginación para listados grandes con tamaño de página configurable y comportamiento estable al aplicar filtros/búsquedas.

### Key Entities *(include if feature involves data)*

- **Tipo de Medición**: Define una medición reutilizable para ejercicios con atributos clave, nombre, unidad, tipo de dato, categoría, descripción opcional y estado.
- **Resultado de Importación**: Resume el procesamiento de un archivo CSV con conteo de filas creadas, rechazadas y detalle de cada rechazo.
- **Asignación de Medición en Ejercicio**: Relación funcional que vincula ejercicios con tipos de medición activos del catálogo.

### Modelo de Colección Esperado

- **Collection Path**: `measurementTypes/{measurementTypeId}`
- **Campos requeridos**: `key`, `name`, `unit`, `dataType`, `category`
- **Campos opcionales**: `description`
- **Semántica de uso**: cada documento representa una medición configurable reutilizable para asignación a ejercicios.

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: El catálogo de tipos de medición es dato administrado internamente por la aplicación.
- **INT-002**: El archivo CSV de importación se considera una entrada externa controlada por el administrador y debe validarse completamente antes de persistir cada fila.
- **INT-003**: La aplicación DEBE mantener trazabilidad de errores de importación para retroalimentar al administrador por fila.
- **INT-004**: Los tipos de medición administrados se publican como catálogo de referencia para la configuración de ejercicios.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de los administradores de prueba puede crear un tipo de medición válido en menos de 2 minutos usando el formulario emergente.
- **SC-002**: Al menos el 95% de las filas válidas de un CSV de 200 registros se crean correctamente en una sola operación.
- **SC-003**: El 100% de las filas inválidas en importación se reporta con motivo de rechazo entendible para el administrador.
- **SC-004**: El 100% de los tipos de medición activos creados quedan disponibles para asignación en ejercicios inmediatamente después de su alta.
- **SC-005**: Los tipos desactivados dejan de estar disponibles para nuevas asignaciones en ejercicios en el 100% de los casos verificados.
- **SC-006**: El 90% de los administradores de prueba califica el panel de mediciones como claro y no ambiguo en una validación funcional interna.
- **SC-007**: En un escenario de al menos 200 registros, el panel mantiene navegación paginada funcional sin errores de índice y con cambio de página consistente.

## Assumptions

- El actor que usa esta funcionalidad es un administrador con acceso al panel de administración.
- El flujo visual esperado para mediciones debe alinearse al patrón ya adoptado en el módulo de músculos.
- La carga por CSV usa una estructura de columnas alineada con los campos del tipo de medición del catálogo.
- La experiencia de administración de mediciones reutiliza el mismo patrón de interacción del panel de músculos (tabla, popup, acciones superiores y filtros).
- Terminología oficial de estados: en UI se usará "desactivado"; técnicamente corresponde a soft delete y se expone en API mediante `includeInactive` para incluir esos registros.
- Las mediciones mencionadas por negocio (peso, repeticiones, series, duración, distancia, velocidad, calorías, inclinación, frecuencia cardíaca, RPE) son ejemplos y no restringen nuevas configuraciones.
- La asignación de mediciones a ejercicios consume únicamente tipos de medición en estado activo.
- FR-016 y FR-022 se mantienen separados: FR-016 cubre visibilidad en nuevas asignaciones; FR-022 cubre preservación histórica de referencias existentes.
