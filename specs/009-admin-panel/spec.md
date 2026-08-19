# Feature Specification: Panel de Administración — Catálogos (Músculos, Mediciones, Ejercicios)

**Feature Branch**: `009-admin-panel`
**Created**: 2026-05-17
**Status**: Draft
**Input**: User description: "necesito crear el panel de administracion de mi aplicacion"

## Overview

El panel de administración (`/administracion`) ya cuenta con la estructura de pestañas definida en spec 006. Esta feature implementa los tres paneles de catálogo funcionales: **Músculos** (actualiza spec 007), **Mediciones** (nuevo) y **Ejercicios** (nuevo). Los tres paneles permiten al administrador consultar, crear, editar y gestionar estado mediante soft delete (activo = no borrado, desactivado = borrado lógico). Además, el panel de Músculos permite importación masiva por CSV para evitar altas manuales una a una. Los ejercicios referencian músculos y tipos de medición, de modo que los tres catálogos forman una unidad coherente y deben implementarse en ese orden.

## Clarifications

### Session 2026-05-17

- Q: ¿Se permite restaurar registros con soft delete? → A: Sí, mediante un filtro (check) para ver borrados y una acción de reactivar en la tabla.
- Q: ¿Qué pasa al reactivar si el `code` ya existe activo? → A: Se bloquea la reactivación y se muestra error de unicidad.
- Q: ¿Cómo se procesa el CSV si contiene filas válidas e inválidas? → A: Importación parcial; se importan válidas y se rechazan inválidas.
- Q: ¿Qué control de permisos aplica al panel `/administracion`? → A: Se difiere para una feature posterior; fuera de alcance en esta iteración.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Gestionar el catálogo de Músculos (Priority: P1)

El administrador abre la pestaña "Músculos" y ve la lista con todos los músculos no borrados del catálogo. Puede crear nuevos músculos, editar la descripción de los existentes, desactivarlos con eliminación lógica e importar músculos masivamente mediante un archivo CSV.

**Why this priority**: Es el catálogo base; los ejercicios referencian músculos. Sin músculos no pueden crearse ejercicios correctamente.

**Independent Test**: Abrir la pestaña "Músculos", verificar que la lista carga, crear un músculo con Code "BICEPS" y Description "Bíceps braquial", desactivarlo mediante soft delete y validar que deja de aparecer en la lista activa. Luego cargar un CSV válido con varios músculos y confirmar que se importan en lote.

**Acceptance Scenarios**:

1. **Given** que la API devuelve músculos no borrados, **When** el administrador abre la pestaña "Músculos", **Then** se muestra una tabla con columnas Code, Description y Acciones; hay un indicador de carga transitorio.
2. **Given** que la API no tiene registros, **When** el administrador abre la pestaña, **Then** se muestra un mensaje de lista vacía.
3. **Given** que la API falla, **When** el administrador abre la pestaña, **Then** se muestra un mensaje de error con botón "Reintentar".
4. **Given** que el administrador rellena Code y guarda, **When** la operación tiene éxito, **Then** el nuevo músculo aparece en la tabla activa por defecto.
5. **Given** que el administrador edita un músculo, **When** el modal se abre, **Then** Code es de solo lectura; puede modificar Description.
6. **Given** que el administrador confirma la eliminación, **When** la operación tiene éxito, **Then** el músculo desaparece de la lista (soft delete, conservado en el sistema).
7. **Given** que el administrador adjunta un CSV válido de músculos, **When** confirma la importación, **Then** el sistema crea los registros válidos en lote y los muestra en la tabla.
8. **Given** que el CSV contiene filas inválidas o códigos duplicados, **When** el sistema procesa el archivo, **Then** se muestra un resumen por fila con éxito/error y motivo para cada rechazo.
9. **Given** que el administrador intenta reactivar un músculo borrado cuyo Code ya existe en otro registro activo, **When** confirma la reactivación, **Then** el sistema bloquea la operación y muestra un error de unicidad.

---

### User Story 2 — Gestionar el catálogo de Mediciones (Priority: P1)

El administrador abre la pestaña "Mediciones" y ve la lista de tipos de medición. Cada tipo define un nombre y un conjunto de campos dinámicos que describen qué datos se recogerán al registrar una sesión con ese tipo (ej.: "Peso + Reps" tiene campos Weight y Reps). Puede crear, editar y eliminar lógicamente tipos de medición.

**Why this priority**: Los ejercicios referencian un tipo de medición; sin este catálogo no puede completarse el alta de ejercicios.

**Independent Test**: Crear un tipo de medición "Peso + Reps" con campos [Weight, Reps], verificar que aparece en la lista, editar para añadir un campo "Notes" y eliminarlo lógicamente.

**Acceptance Scenarios**:

1. **Given** que la API devuelve mediciones no borradas, **When** el administrador abre la pestaña "Mediciones", **Then** se muestra una tabla con columnas Name, Nº de Campos y Acciones.
2. **Given** que el administrador crea una medición con Name "Tiempo" y campo "Seconds", **When** guarda, **Then** la nueva medición aparece en la tabla activa por defecto.
3. **Given** que el administrador edita una medición, **When** el modal se abre, **Then** puede modificar Name y la lista de Fields.
4. **Given** que el administrador intenta eliminar una medición referenciada por algún ejercicio, **When** hace clic en eliminar, **Then** se muestra un mensaje de advertencia de dependencias antes de proceder.

---

### User Story 3 — Gestionar el catálogo de Ejercicios (Priority: P2)

El administrador abre la pestaña "Ejercicios" y ve la lista de ejercicios del catálogo. Puede crear nuevos ejercicios seleccionando músculos primarios/secundarios y un tipo de medición de los catálogos ya creados, adjuntar imágenes y vídeos, editar todas sus propiedades y eliminar ejercicios lógicamente.

**Why this priority**: P2 porque depende de que los catálogos de Músculos y Mediciones (P1) estén disponibles como catálogo de referencia.

**Independent Test**: Con al menos un músculo y una medición activos en el sistema, crear un ejercicio "Press de Banca" con Code "BENCH_PRESS", Dificultad Media, seleccionar Pectoral como músculo primario y "Peso + Reps" como medición. Verificar que aparece en la lista y que las referencias se muestran correctamente.

**Acceptance Scenarios**:

1. **Given** que la API devuelve ejercicios, **When** el administrador abre la pestaña "Ejercicios", **Then** se muestra una tabla con columnas Code, Name, Dificultad, Categoría, Estado y Acciones.
2. **Given** que el administrador crea un ejercicio con todos los campos requeridos, **When** guarda, **Then** el ejercicio aparece en la tabla activa por defecto.
3. **Given** que el administrador adjunta imágenes o vídeos, **When** guarda, **Then** las referencias multimedia quedan almacenadas y son visibles en el detalle del ejercicio.
4. **Given** que el administrador confirma la eliminación de un ejercicio, **When** la operación tiene éxito, **Then** el ejercicio desaparece de la lista (soft delete).
5. **Given** que el administrador abre el selector de músculos primarios, **When** el modal está abierto, **Then** el selector muestra únicamente músculos no borrados.
6. **Given** que el administrador abre el selector de MeasurementType, **When** el modal está abierto, **Then** el selector muestra únicamente mediciones no borradas.
7. **Given** que el administrador intenta eliminar un músculo referenciado por algún ejercicio, **When** hace clic en eliminar, **Then** se muestra advertencia de dependencias antes de proceder.

---

### Edge Cases

- Intento de crear un ejercicio sin ningún músculo disponible (no borrado) → se muestra advertencia indicando que primero deben crearse músculos.
- Intento de crear un ejercicio sin ninguna medición disponible (no borrada) → se muestra advertencia indicando que primero deben crearse mediciones.
- Fallo de red al guardar un formulario → el modal permanece abierto con el mensaje de error; no se pierden los datos ingresados.
- Introducción de un Code duplicado en Músculo o Ejercicio → el modal permanece abierto con un mensaje de código ya en uso.
- Aplicar soft delete a un músculo o medición referenciado por ejercicios activos → los ejercicios existentes conservan la referencia histórica, pero el registro deja de aparecer en los selectores de nuevos ejercicios.
- Importación CSV de músculos con cabeceras faltantes o formato inválido → la importación se rechaza y se informa el motivo.
- Importación CSV con mezcla de filas válidas e inválidas → se importan solo las válidas y se devuelve un resumen detallado por fila.
- Reactivación de un registro borrado cuando existe conflicto con un valor único activo (ej.: `code`) → la reactivación se rechaza con mensaje claro de conflicto.

---

## Requirements *(mandatory)*

### Functional Requirements

**Comunes a los tres paneles**

- **FR-001**: Cada panel DEBE gestionar tres estados explícitos: carga, error (con botón Reintentar) y lista vacía.
- **FR-002**: Las eliminaciones DEBEN aplicar soft delete; los registros no se borran físicamente del sistema.
- **FR-003**: La activación/desactivación DEBE modelarse exclusivamente mediante soft delete en todas las entidades (activo = no borrado, desactivado = borrado lógico).
- **FR-004**: Los registros con soft delete aplicado NO DEBEN aparecer en los selectores de referencia de otros paneles (ej.: selector de músculos en el panel de Ejercicios).
- **FR-005**: Cada panel DEBE incluir un filtro tipo check para alternar entre vista por defecto (solo no borrados) y vista que incluya registros con soft delete.
- **FR-006**: En la vista que incluya borrados, la tabla DEBE ofrecer una acción de reactivar para revertir el soft delete.

**Panel Músculos (actualización de spec 007)**

- **FR-007**: La tabla de Músculos DEBE mostrar únicamente registros no borrados por defecto.
- **FR-008**: El panel DEBE permitir aplicar soft delete a un músculo desde la acción de eliminar.
- **FR-009**: El modal de edición de Músculo NO DEBE incluir un control IsActive; el estado se gestiona por soft delete.
- **FR-010**: El panel de Músculos DEBE permitir cargar un archivo CSV para importar músculos en lote.
- **FR-011**: El CSV de importación DEBE requerir al menos la columna `code`; `description` es opcional.
- **FR-012**: El sistema DEBE validar el archivo CSV y reportar resultado por fila (importada/rechazada) con motivo en cada rechazo.
- **FR-013**: Las filas con `code` duplicado (contra datos existentes o dentro del propio archivo) NO DEBEN importarse.
- **FR-014**: La importación CSV DEBE ser parcial: filas válidas se persisten y filas inválidas se rechazan sin cancelar todo el archivo.

**Panel Mediciones (nuevo)**

- **FR-015**: El sistema DEBE permitir crear tipos de medición con un nombre único y una lista de campos dinámicos (`Fields[]`).
- **FR-016**: Cada campo (`Field`) de una medición DEBE tener al menos un nombre identificador.
- **FR-017**: La tabla de Mediciones DEBE mostrar el nombre del tipo y la cantidad de campos definidos.
- **FR-018**: El sistema DEBE advertir al administrador antes de eliminar una medición referenciada por algún ejercicio.

**Panel Ejercicios (nuevo)**

- **FR-019**: La entidad Ejercicio DEBE tener: Code (único, requerido), Name (requerido), Description (opcional), Difficulty (enum), Category (enum), PrimaryMuscles[] (FK, al menos 1 requerido), SecondaryMuscles[] (FK, opcional), MeasurementType (FK, requerido), Images[] (opcional), Videos[] (opcional).
- **FR-020**: El campo Code de un ejercicio DEBE ser de solo lectura en el modal de edición.
- **FR-021**: Los selectores de músculos primarios y secundarios DEBEN mostrar únicamente músculos no borrados.
- **FR-022**: El selector de MeasurementType DEBE mostrar únicamente mediciones no borradas.
- **FR-023**: El sistema DEBE advertir al administrador antes de eliminar un músculo referenciado por ejercicios.
- **FR-024**: El backend DEBE exponer endpoints REST completos (listar, obtener por ID, crear, actualizar, soft delete y reactivar) para los tres catálogos y un endpoint de importación CSV para músculos.
- **FR-025**: Si una reactivación viola una restricción de unicidad vigente (ej.: `code` en Músculos/Ejercicios), el sistema DEBE bloquear la reactivación y devolver un error de conflicto comprensible para el administrador.

### Key Entities *(include if feature involves data)*

- **Muscle**: id (único), code (único, requerido), description (opcional), deletedAt/deleted (marcador de borrado lógico)
- **MeasurementType**: id (único), name (único, requerido), fields[] (lista de MeasurementField), deletedAt/deleted (marcador de borrado lógico)
- **MeasurementField**: name (requerido, identificador del campo a recoger, ej.: "Weight", "Reps", "Seconds")
- **Exercise**: id (único), code (único, requerido), name (requerido), description (opcional), difficulty (enum: Principiante / Intermedio / Avanzado), category (enum), primaryMuscles[] (FK → Muscle, mínimo 1), secondaryMuscles[] (FK → Muscle, opcional), measurementType (FK → MeasurementType), images[] (referencias externas), videos[] (referencias externas), deletedAt/deleted (marcador de borrado lógico)

### External Integrations & Data Boundaries *(mandatory)*

- **INT-001**: Los tres catálogos son datos propios del sistema; no se importan de fuentes externas. Se almacenan en la base de datos principal del proyecto.
- **INT-002**: Las imágenes y vídeos de ejercicios se almacenan en un servicio de almacenamiento externo; la entidad Ejercicio guarda únicamente las referencias (URLs o IDs).
- **INT-003**: Si el servicio de almacenamiento externo no está disponible, el ejercicio puede crearse o editarse sin adjuntar multimedia; los campos Images y Videos son opcionales.
- **INT-004**: No se consumen catálogos externos (como Wger) en esta feature; todos los datos son gestionados por el propio administrador.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El administrador puede completar un ciclo CRUD completo (crear, listar, editar y aplicar soft delete) en cada uno de los tres paneles sin errores visibles.
- **SC-002**: Los tres estados (carga, error, vacío) son visibles en cada panel bajo las condiciones correspondientes.
- **SC-003**: Un ejercicio creado referenciando músculos y una medición muestra correctamente esas referencias en la tabla y en el modal de edición.
- **SC-004**: Los registros con soft delete aplicado desaparecen de los selectores de referencia inmediatamente tras su desactivación lógica.
- **SC-005**: Los tres catálogos están disponibles y funcionales partiendo de un sistema sin datos previos (flujo de alta completo desde cero).
- **SC-006**: Frontend y backend compilan sin errores tras la implementación de los tres paneles.
- **SC-007**: El administrador puede importar al menos 100 músculos con CSV en una sola operación y obtiene un resumen claro de filas exitosas y rechazadas.
- **SC-008**: El administrador puede filtrar para ver borrados y reactivar un registro en cada panel en menos de 3 acciones.

---

## Assumptions

- Los valores del enum **Difficulty** son: `Principiante`, `Intermedio`, `Avanzado`.
- Los valores del enum **Category** (categoría del ejercicio) se definirán antes de la implementación; como punto de partida se asumen: `Fuerza`, `Cardio`, `Movilidad`, `Estiramiento`.
- Los `MeasurementField` son definiciones de los campos que se recogerán (ej.: "Weight", "Reps"), no valores concretos de una sesión; los valores reales se registran en el módulo de seguimiento (fuera del alcance de esta feature).
- La paginación de las tablas es del lado del cliente en esta iteración.
- No se implementan búsqueda ni filtros en las tablas en esta iteración.
- Los textos de la UI usan las claves `administration.*` de `resources.ts`; se añaden únicamente las claves estrictamente necesarias.
- Los datos se persisten en Firebase Firestore a través de los endpoints del backend .NET.
- El estado global de los tres paneles se gestiona con Redux Toolkit.
- Esta feature actualiza/extiende el panel de Músculos de spec 007; la activación/desactivación se basa en soft delete y no en campo `IsActive`.
- El orden de implementación recomendado es: Músculos → Mediciones → Ejercicios, dado que Ejercicios depende de los dos anteriores.
- El formato CSV de Músculos usa cabecera en inglés: `code,description`.
- El modelo de permisos/autorización para `/administracion` se definirá en una feature posterior; esta especificación no impone reglas de acceso adicionales.
