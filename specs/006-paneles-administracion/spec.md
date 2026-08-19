# Feature Specification: Paneles de Administración con Tabs

**Feature Branch**: `006-paneles-administracion`
**Created**: 2026-05-15
**Status**: Draft
**Input**: User description: "Ahora vamos a crear los paneles de administracion, en este caso la pagina de administracion sera como un panel con tabs, y se creara una tabb por panel de administracion"

## Overview

La ruta `/administracion` actualmente muestra una página placeholder. Esta feature la reemplaza con una página real que contiene la estructura de navegación por pestañas (MUI Tabs): una pestaña por cada dominio de administración.

El contenido de cada pestaña es un placeholder por ahora. Los paneles CRUD (Músculos, Tipos de Ejercicio, Tipos de Formulario, Ejercicios) se implementarán en ramas futuras dedicadas.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Navegar entre paneles de administración (Priority: P1)

El administrador accede a `/administracion` y ve una página con cuatro pestañas claramente etiquetadas. Puede cambiar de pestaña sin recargar la página; el contenido del área inferior cambia al panel correspondiente. La URL no se modifica al cambiar de tab. El contenido de cada pestaña muestra un placeholder indicando que está en construcción.

**Why this priority**: Es el único entregable de esta feature; establece la estructura que las ramas futuras rellenarán.

**Independent Test**: Abrir `/administracion`, verificar que hay 4 tabs visibles, hacer clic en cada una y confirmar que el contenido cambia al placeholder correspondiente y la tab queda visualmente activa. La URL permanece en `/administracion` sin parámetros adicionales.

**Acceptance Scenarios**:

1. **Given** que el usuario navega a `/administracion`, **When** la página carga, **Then** se muestran 4 pestañas con los nombres del catálogo (`administration.tabs.*`), la primera pestaña activa por defecto.
2. **Given** que el usuario hace clic en cualquier pestaña, **When** la tab cambia, **Then** el contenido del área inferior cambia al del panel correspondiente y la pestaña queda visualmente activa.

---

### Edge Cases

- Si el componente de tabs recibe un valor de tab inválido por estado interno, se muestra la primera pestaña por defecto.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La página `AdministracionPage` DEBE reemplazar el `PlaceholderPage` en la ruta `/administracion`.
- **FR-002**: `AdministracionPage` DEBE mostrar 4 pestañas navegables etiquetadas con las claves `administration.tabs.*` de `resources.ts`. La navegación entre tabs cambia únicamente el contenido renderizado; no modifica la URL.
- **FR-003**: El contenido de cada pestaña DEBE ser un componente placeholder que indique que el panel está en construcción. Los paneles CRUD se implementarán en ramas futuras.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El administrador puede cambiar entre las 4 pestañas y el contenido del área inferior cambia al panel correspondiente.
- **SC-002**: El proyecto frontend compila sin errores tras el cambio de ruta.

---

## Assumptions

- Los paneles CRUD (Músculos, Tipos de Ejercicio, Tipos de Formulario, Ejercicios) se implementan en ramas futuras; esta feature solo crea la estructura de tabs.
- El contenido de cada tab es un placeholder reutilizando el componente `PlaceholderPage` existente o similar.
- Los textos de las pestañas reutilizan las claves `administration.tabs.*` existentes en `resources.ts`; no se añaden claves nuevas.
- No se realizan cambios en el backend en esta feature.
- `PlaceholderPage` en `/administracion` se reemplaza; las demás rutas placeholder no se ven afectadas.
- Los estados de carga, error y vacío no aplican en esta iteración porque no hay data fetching; se añadirán en cada rama futura cuando los paneles integren llamadas a la API.

---

## Clarifications

### Session 2026-05-15

- Q: ¿La eliminación de registros es física (hard delete) o lógica (soft delete)? → A: Eliminación lógica (soft delete) — campo `IsDeleted`; registros ocultos en listados pero conservados en BD. *(Deferred — aplica a ramas futuras de paneles CRUD)*
- Q: ¿Cuáles son los valores posibles del campo `dificultad` en Ejercicios? → A: Enum de 3 niveles — Principiante · Intermedio · Avanzado. *(Deferred — aplica a la rama futura del panel Ejercicios)*
- Q: ¿El contenido de cada tab es solo estructura de tabs o incluye paneles CRUD completos? → A: Solo estructura de tabs con contenido placeholder; los paneles CRUD se implementan en ramas futuras.
- Q: ¿La tab activa debe persistir en la URL o solo cambiar el contenido renderizado? → A: Solo cambia el contenido renderizado; la URL no se modifica al cambiar de tab.