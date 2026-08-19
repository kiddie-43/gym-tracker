# Feature Specification: Página Placeholder para Módulos Pendientes

**Feature Branch**: `005-pagina-placeholder`
**Created**: 2026-05-15
**Status**: Draft
**Input**: User description: "necesito que se cree un componente place holder, en este componetnte se usara para de moemto que las rutas muestren algo y no una pantalla en blanco, estas rutas tendran que utilizar mi componente footer y header, revisa como se creo home, pero en vez de duplicar el componente home sera un place holder, luego esta decision se tendra que añadir a la skill para que cuando se cree una nueva pantalla o pagina se cree con esa estructura"

## Overview

Crear un componente `PlaceholderPage` reutilizable que sirva como marcador de posición para los módulos de la app que aún no tienen implementación. Actualmente, navegar a cualquier módulo del menú lateral (excepto la página de inicio) muestra una pantalla en blanco. `PlaceholderPage` soluciona esto mostrando el nombre del módulo y un indicador de que está en desarrollo, reutilizando el mismo layout (header + footer) que el resto de la app.

La actualización de la skill de arquitectura frontend queda fuera del alcance de esta feature — se abordará en una feature posterior cuando el patrón esté estabilizado.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navegar a un módulo sin implementar (Priority: P1)

Al hacer clic en cualquier ítem del menú lateral, el usuario ve una página coherente con la identidad visual de la app: el header de navegación, un contenido centrado que identifica el módulo y el footer. Nunca se muestra una pantalla en blanco.

**Why this priority**: Es el problema visible más inmediato — cualquier usuario que abra el menú lateral y navegue a cualquier módulo ve una pantalla en blanco. Esta US elimina ese problema.

**Independent Test**: Abrir la app, hacer clic en cualquier módulo del menú lateral (Entrenamiento, Progreso, Rutinas, Dietas, Comidas, Ajustes, Administración, Perfil) y verificar que se muestra una página con header, título del módulo y footer.

**Acceptance Scenarios**:

1. **Given** que la app está cargada, **When** el usuario navega a `/workouts`, **Then** ve una página con el header, el nombre "Entrenamiento" (o "Workout" en inglés) y el footer — sin pantalla en blanco.
2. **Given** que el usuario está en una ruta de módulo placeholder, **When** observa la página, **Then** el chip de sección activa del header refleja el módulo actual — esto ocurre automáticamente mediante detección de ruta (useLocation/NavLink) sin ningún cambio en AppHeader.
3. **Given** que el usuario navega a cualquiera de las 8 rutas registradas, **Then** la experiencia visual (header + contenido + footer) es idéntica en todas.

---

### User Story 2 - Cambiar el idioma en una página placeholder (Priority: P2)

El selector de idioma del header funciona correctamente en las páginas placeholder: al cambiar entre ES y EN, el nombre del módulo y los textos del placeholder se actualizan sin recargar la página.

**Why this priority**: La internacionalización es transversal. Si el selector de idioma no funciona en las páginas placeholder, el comportamiento de la app sería inconsistente.

**Independent Test**: Navegar a `/routines`, cambiar el idioma a EN, verificar que el título cambia a "Routines". Volver a ES y verificar "Rutinas".

**Acceptance Scenarios**:

1. **Given** que el usuario está en una página placeholder en español, **When** cambia el idioma a inglés, **Then** el nombre del módulo y los textos del placeholder se actualizan al inglés sin recarga.
2. **Given** que el usuario está en una página placeholder en inglés, **When** cambia el idioma a español, **Then** todo vuelve al español sin recarga.

---

### Edge Cases

- ¿Qué ocurre si se pasa una clave i18n que no existe en los recursos? → i18next devuelve la clave en bruto como fallback; la app no lanza error.
- ¿Qué ocurre si se registra una ruta fuera del árbol `AppLayout`? → El header y el footer no se renderizan. Las rutas de módulo siempre deben estar bajo `<Route path="/" element={<AppLayout />}>`.
- ¿Qué ocurre si el usuario recarga la página en una ruta de módulo placeholder? → La ruta se mantiene y se muestra el placeholder correctamente; no hay redirección a `/`.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Debe existir un componente `PlaceholderPage` ubicado en `pages/placeholder/PlaceholderPage.tsx`.
- **FR-002**: `PlaceholderPage` DEBE aceptar una prop `labelKey: string` (clave i18n del módulo, p. ej. `'nav.workout'`) que se usa para mostrar el nombre del módulo como título principal.
- **FR-003**: `PlaceholderPage` DEBE mostrar: (a) un texto principal con el formato "[nombre del módulo] está en desarrollo" (el nombre proviene de `t(labelKey)`, el resto es texto estático traducible), y (b) un chip/badge que muestra la etiqueta `placeholder.status` (`'En desarrollo'` / `'In development'`) como indicador visual del estado.
- **FR-004**: Todos los textos de `PlaceholderPage` DEBEN obtenerse exclusivamente via `useTranslation()`. Los textos estáticos del componente (mensaje de desarrollo, etiqueta de estado) usan claves `placeholder.*` que se añaden a `i18n/resources.ts` en ES y EN.
- **FR-005**: Las rutas `/workouts`, `/progress`, `/routines`, `/diets`, `/meals`, `/settings`, `/administracion` y `/profile` DEBEN registrarse en `App.tsx` bajo el mismo `<Route path="/" element={<AppLayout />}>` que ya tiene `HomePage`, de modo que todas hereden automáticamente el header y el footer.
- **FR-006**: Los cambios en `App.tsx` DEBEN limitarse a añadir las nuevas rutas. No se modifican `AppHeader`, `AppFooter`, `AppLayout` ni ningún otro componente existente. El chip de sección activa del header detecta la ruta en curso automáticamente (via `useLocation`/`NavLink`) sin intervención manual en AppHeader.
### Key Entities

- **PlaceholderPage**: Componente de vista sin estado ni datos externos. Recibe `labelKey` como única prop. Es temporal — se reemplaza por la implementación real del módulo en su feature correspondiente.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Navegar a cualquiera de las 8 rutas nuevas muestra una página con header, contenido identificable (nombre del módulo) y footer — sin pantalla en blanco en ninguna de ellas.
- **SC-002**: Cambiar el idioma mientras se está en una página placeholder actualiza todos los textos (nombre del módulo + textos estáticos) sin recargar la página.
- **SC-003**: El proyecto compila sin errores tras los cambios.


---

## Assumptions

- `PlaceholderPage` es deliberadamente simple: sin estado, sin llamadas a API, sin formulario. Su único propósito es evitar pantallas en blanco.
- Las 8 rutas nuevas son exactamente: `/workouts`, `/progress`, `/routines`, `/diets`, `/meals`, `/settings`, `/administracion`, `/profile`.
- Los textos del placeholder en ES: `placeholder.status: 'En desarrollo'`, `placeholder.message: 'Este módulo estará disponible próximamente.'`; EN: `placeholder.status: 'In development'`, `placeholder.message: 'This module will be available soon.'`
- La actualización de la skill `frontend-architecture` queda diferida a una feature posterior.
- `PlaceholderPage` se reemplazará en features futuras; no es permanente.

---

## Clarifications

### Session 2026-05-15

- Q: ¿El chip de sección activa del AppHeader actualiza su estado automáticamente al registrar rutas nuevas, o requiere cambios en AppHeader? → A: Detecta la ruta automáticamente (useLocation/NavLink); FR-006 permanece intacto, no se modifica AppHeader.
- Q: ¿Qué formato debe tener el indicador visual del estado en PlaceholderPage? → A: Chip/badge con texto "En desarrollo" + mensaje principal con formato "[Nombre módulo] está en desarrollo".
- Q: ¿Debe actualizarse la skill `frontend-architecture` como parte de esta feature? → A: No; la actualización de la skill queda diferida. Esta feature solo cubre el componente `PlaceholderPage` y el registro de las 8 rutas existentes.
