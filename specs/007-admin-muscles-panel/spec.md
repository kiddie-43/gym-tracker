# Feature Specification: Panel de Administración — Músculos

**Feature Branch**: `007-admin-muscles-panel`
**Created**: 2026-05-15
**Status**: Draft
**Input**: User description: "ahora vamos a empezar con el primer panel, que sera el de musculos, hay que crear su redux, su pagina que se mostrara por las tabs del panel de administrador y sus endpoints, tendra que estar conectado con firebase y sus propiedades de momento seran id codigo y descripcion, estas propiedades deben de estar en ingles"

## Overview

La pestaña "Músculos" de `AdministrationPage` actualmente muestra un placeholder. Esta feature la reemplaza con un panel funcional de gestión CRUD: el administrador puede consultar, crear, editar y eliminar músculos del catálogo. Cada músculo tiene un identificador único, un código único y una descripción. Los datos se persisten en el sistema a través de la API del backend.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Consultar la lista de músculos (Priority: P1)

El administrador abre la pestaña "Músculos" y ve una tabla con todos los músculos del catálogo. Si hay datos, se muestra la lista; si no, se muestra un mensaje de lista vacía; si hay error de carga, se muestra un mensaje con opción de reintentar.

**Why this priority**: Sin la lista no hay punto de partida para ninguna otra operación.

**Independent Test**: Abrir `/administration`, ir a la pestaña "Músculos" y verificar que la tabla carga con datos, muestra un mensaje vacío si no hay registros, y muestra un botón de reintentar si la carga falla.

**Acceptance Scenarios**:

1. **Given** que la API devuelve músculos, **When** el usuario abre la pestaña "Músculos", **Then** se muestra una tabla con las columnas Code y Description y un indicador de carga transitorio.
2. **Given** que la API no tiene registros, **When** el usuario abre la pestaña, **Then** se muestra un mensaje de lista vacía.
3. **Given** que la API falla, **When** el usuario abre la pestaña, **Then** se muestra un mensaje de error con botón "Reintentar" que vuelve a solicitar los datos.

---

### User Story 2 — Crear un músculo (Priority: P1)

El administrador hace clic en "Añadir", rellena el formulario y guarda. El nuevo músculo aparece en la tabla.

**Why this priority**: P1 junto con US1 — sin datos propios el panel carece de valor.

**Independent Test**: Hacer clic en "Añadir", rellenar Code = "CHEST" y Description = "Pectoral mayor", guardar y verificar que aparece en la tabla.

**Acceptance Scenarios**:

1. **Given** que el usuario hace clic en "Añadir", **When** el modal se abre, **Then** el formulario muestra los campos Code (requerido) y Description (opcional) vacíos.
2. **Given** que el usuario rellena datos válidos y guarda, **When** la API responde con éxito, **Then** el modal se cierra y el nuevo músculo aparece en la tabla sin recargar la página.
3. **Given** que el usuario introduce un código ya existente, **When** intenta guardar, **Then** el modal permanece abierto y se muestra un error indicando que el código ya está en uso.
4. **Given** que el usuario deja el campo Code vacío, **When** intenta guardar, **Then** se muestra un error de validación en el campo y el formulario no se envía.

---

### User Story 3 — Editar un músculo (Priority: P1)

El administrador hace clic en el botón de editar de una fila, modifica la descripción y guarda. Los cambios quedan reflejados en la tabla.

**Why this priority**: La edición corrige errores en datos existentes; su ausencia hace el catálogo inmutable.

**Independent Test**: Hacer clic en editar sobre un músculo, cambiar la descripción, guardar y verificar que la tabla muestra el nuevo valor.

**Acceptance Scenarios**:

1. **Given** que el usuario hace clic en editar, **When** el modal se abre, **Then** los campos aparecen pre-rellenos con los valores actuales del músculo; el campo Code es de solo lectura.
2. **Given** que el usuario modifica la descripción y guarda, **When** la API responde con éxito, **Then** el modal se cierra y la tabla refleja los cambios.
3. **Given** que la API devuelve error al guardar, **When** se produce el fallo, **Then** el modal permanece abierto y se muestra el mensaje de error de la API.

---

### User Story 4 — Eliminar un músculo (Priority: P2)

El administrador hace clic en el botón de eliminar de una fila, confirma en el diálogo y el músculo desaparece de la lista.

**Why this priority**: P2 — la eliminación es necesaria pero no bloquea la operativa básica de creación y edición.

**Independent Test**: Hacer clic en eliminar sobre un músculo, confirmar en el diálogo y verificar que el músculo desaparece de la tabla.

**Acceptance Scenarios**:

1. **Given** que el usuario hace clic en eliminar, **When** el diálogo de confirmación se abre, **Then** se muestra el código del músculo y los botones Confirmar y Cancelar.
2. **Given** que el usuario confirma, **When** la API responde con éxito, **Then** el diálogo se cierra y el músculo desaparece de la tabla.
3. **Given** que el usuario cancela, **When** hace clic en Cancelar, **Then** el diálogo se cierra sin modificar la lista.

---

### Edge Cases

- Fallo de red al guardar en el modal → el modal permanece abierto con mensaje de error; no se pierde lo que el usuario había escrito.
- Fallo de red al eliminar → el diálogo permanece abierto con mensaje de error.
- Carga de lista mientras hay una operación en curso (creación/edición) → la operación en curso no se interrumpe.
- Código con espacios o caracteres especiales → se valida en el formulario antes de enviar.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La pestaña "Músculos" de `AdministrationPage` DEBE mostrar el panel de gestión de músculos en lugar del placeholder actual.
- **FR-002**: El panel DEBE mostrar una tabla con los músculos del catálogo y las columnas: Code, Description y columna de acciones (editar, eliminar).
- **FR-003**: El panel DEBE gestionar tres estados de forma explícita: carga, error (con botón Reintentar) y vacío.
- **FR-004**: El botón "Añadir" DEBE abrir un modal con campos Code (requerido, único) y Description (opcional).
- **FR-005**: El campo Code DEBE ser de solo lectura en el modal de edición.
- **FR-006**: La eliminación de un músculo DEBE ser lógica (soft delete): el registro se oculta de la lista pero se conserva en el sistema.
- **FR-007**: El sistema DEBE devolver un error claro cuando se intenta guardar un Code duplicado; el modal no se cierra.
- **FR-008**: El backend DEBE exponer endpoints REST para: listar, obtener por ID, crear, actualizar y eliminar (soft) músculos.

### Key Entities

- **Muscle**: id (auto, único) · code (único, requerido) · description (opcional)

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El administrador puede completar un ciclo CRUD completo en el panel de Músculos sin errores visibles.
- **SC-002**: Los tres estados (carga, error, vacío) son visibles bajo las condiciones correspondientes.
- **SC-003**: Introducir un código duplicado muestra un mensaje de error sin cerrar el modal.
- **SC-004**: Un músculo eliminado desaparece de la lista inmediatamente tras confirmar.
- **SC-005**: Frontend y backend compilan sin errores.

---

## Assumptions

- El campo `name` (nombre) del músculo no está incluido en esta iteración; las propiedades visibles son únicamente `code` y `description`. Si el backend requiere `name`, se enviará el mismo valor que `code` como valor temporal hasta que se añada el campo en una iteración futura.
- Muscle Group no está incluido en esta iteración; el campo `muscleGroupIds` se enviará vacío al backend.
- La paginación de la tabla es del lado del cliente en esta iteración.
- No se implementa búsqueda ni filtro en la tabla en esta iteración.
- Los textos de la UI reutilizan las claves `administration.muscles.*` existentes en `resources.ts`; se añaden solo las claves estrictamente necesarias.
- El campo Code no es modificable una vez creado el registro (solo lectura en edición).
- Los datos se persisten en Firebase Firestore a través de los endpoints del backend .NET.
- El estado global del panel se gestiona con Redux Toolkit en el frontend.
- Los estados de carga, error y vacío se añaden en este panel como primer panel funcional de la sección de administración.

---

## Clarifications

### Session 2026-05-15

- Q: ¿La eliminación es física o lógica? → A: Lógica (soft delete) — confirmado en feature 006.
- Q: ¿Las propiedades del músculo incluyen `name`? → A: La descripción de la feature especifica solo `id`, `code` y `description`. Se asume que `name` se omite en la UI; si el backend lo requiere, se enviará `code` como valor de `name` temporalmente.
- Q: ¿Se incluye `muscleGroup` en esta iteración? → A: No — se incluirá en una iteración futura.
