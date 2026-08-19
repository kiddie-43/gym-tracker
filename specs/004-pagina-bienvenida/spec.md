# Feature Specification: Página de Bienvenida con Layout Principal

**Feature Branch**: `004-pagina-bienvenida`
**Created**: 2026-05-15
**Status**: Draft
**Input**: User description: "Crear la página de bienvenida (home) de la app con su layout principal (AppHeader + AppFooter), enrutamiento base y soporte multiidioma, respetando la estructura constitucional del proyecto."

## Contexto

La aplicación Gym Tracker carece actualmente de una pantalla de bienvenida funcional y de un layout base operativo. El `App.tsx` está en blanco (solo `ThemeProvider` + `CssBaseline`, sin router ni rutas), los componentes `AppHeader`, `AppFooter` y `AppLayout` existen en `shared/components/` (fuera de la ubicación constitucional), y `AppPreferencesContext` —dependencia directa del header— no existe. Esta feature establece la base navegable mínima de la app.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visualizar la página de bienvenida (Priority: P1)

Un usuario que abre la app ve una página de bienvenida estructurada con un header de navegación, el contenido principal de presentación de la app y un footer, sin necesidad de autenticarse.

**Why this priority**: Es el punto de entrada de toda la aplicación. Sin esta pantalla la app no tiene ruta `/` funcional ni layout navegable. Ningún otro módulo puede ser alcanzado por el usuario.

**Independent Test**: Puede probarse arrancando la app en desarrollo, navegando a `/` y verificando que se renderiza el header, el contenido de bienvenida y el footer, tanto en ES como en EN.

**Acceptance Scenarios**:

1. **Given** que la app arranca por primera vez, **When** el usuario navega a `/`, **Then** se muestra la página de bienvenida con header, contenido principal (hero + cards + flujo + CTA) y footer completamente renderizados.
2. **Given** que el usuario está en la página de bienvenida, **When** redimensiona la ventana a ancho móvil (<600 px), **Then** el layout se adapta: el header colapsa al menú hamburguesa y el contenido se apila verticalmente sin overflow horizontal.
3. **Given** que la app arranca, **When** se carga la página de bienvenida, **Then** todos los textos visibles provienen del sistema i18n y se muestran en el idioma configurado por defecto (español).

---

### User Story 2 - Cambiar el idioma desde el header (Priority: P1)

Un usuario puede cambiar el idioma de la aplicación entre español e inglés mediante el selector del header sin recargar la página.

**Why this priority**: El soporte multiidioma es un requisito explícito de la feature y afecta a todos los textos de la app. Debe validarse en la misma feature que lo introduce.

**Independent Test**: Puede probarse seleccionando "EN" en el selector del header y verificando que todos los textos del header, footer y página de bienvenida cambian al inglés. Al volver a "ES" deben restaurarse.

**Acceptance Scenarios**:

1. **Given** que el idioma activo es español, **When** el usuario selecciona "EN" en el selector del header, **Then** todos los textos de la página (header, home, footer) cambian al inglés sin recarga.
2. **Given** que el idioma activo es inglés, **When** el usuario selecciona "ES", **Then** todos los textos vuelven al español.
3. **Given** que el usuario cambia de idioma, **When** navega a otra ruta y vuelve, **Then** el idioma seleccionado se mantiene activo durante la sesión.

---

### User Story 3 - Navegar mediante el menú lateral (Priority: P2)

Un usuario puede abrir el menú lateral desde el header, ver los módulos disponibles de la app y navegar a cualquiera de ellos con un clic.

**Why this priority**: El menú lateral es el sistema de navegación principal de la app. Sin él, las rutas de los demás módulos son inaccesibles desde la UI.

**Independent Test**: Puede probarse haciendo clic en el icono de hamburguesa, verificando que el drawer se abre con todos los ítems de navegación, y haciendo clic en uno para comprobar que la ruta cambia.

**Acceptance Scenarios**:

1. **Given** que el usuario está en cualquier página, **When** hace clic en el botón de menú del header, **Then** se abre un drawer lateral con la lista completa de módulos navegables.
2. **Given** que el drawer está abierto, **When** el usuario hace clic en un ítem de navegación, **Then** el drawer se cierra y la URL cambia a la ruta correspondiente.
3. **Given** que el usuario está en una ruta activa, **When** abre el drawer, **Then** el ítem correspondiente a la ruta actual aparece visualmente marcado como seleccionado.

---

### User Story 4 - Respetar el tema visual del sistema operativo (Priority: P2)

La interfaz de la app adopta automáticamente el tema claro u oscuro según la preferencia configurada en el sistema operativo del usuario, sin ningún control de cambio de tema dentro de la web.

**Why this priority**: El `AppPreferencesContext` debe crearse en esta feature y debe leer `prefers-color-scheme` del sistema. No hay switch de tema en la UI.

**Independent Test**: Puede probarse configurando el sistema operativo en modo oscuro y verificando que la app arranca con el tema oscuro. Cambiar el SO a modo claro con la app abierta y verificar que el tema cambia de forma inmediata sin recargar la página.

**Acceptance Scenarios**:

1. **Given** que el sistema operativo tiene modo oscuro activo, **When** el usuario abre la app, **Then** la interfaz se renderiza con el tema oscuro definido en `getAppTheme('dark')`.
2. **Given** que el sistema operativo tiene modo claro activo, **When** el usuario abre la app, **Then** la interfaz se renderiza con el tema claro definido en `getAppTheme('light')`.
3. **Given** que la app está abierta con tema claro, **When** el usuario cambia el SO a modo oscuro sin recargar la página, **Then** la interfaz cambia al tema oscuro de forma inmediata sin recargar.
4. **Given** que el SO tiene un tema definido, **When** el usuario recarga la app, **Then** la interfaz arranca directamente con el tema del SO.

---

### Edge Cases

- ¿Qué ocurre si el navegador no soporta el idioma configurado? → El sistema usa español como fallback definido en i18n.
- ¿Qué ocurre si el usuario intenta acceder a una ruta no definida? → Por ahora no hay ruta 404; el router solo define `/`. Las rutas no definidas no se especifican en esta feature.
- ¿Qué ocurre cuando el usuario hace clic en "Ver/editar perfil" en el menú de perfil? → La app navega a `/profile`. Dicha ruta no está implementada en esta feature; el comportamiento al navegar a una ruta sin componente asignado queda fuera del scope.
- ¿Qué ocurre si `window.matchMedia` no está disponible (entorno SSR o navegador muy antiguo)? → El contexto usa `'light'` como valor de `themeMode` por defecto y la app funciona correctamente.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La app DEBE tener una ruta `/` que renderice `AppLayout` con `HomePage` como contenido.
- **FR-002**: `AppLayout` DEBE componer `AppHeader`, el contenido de la ruta activa y `AppFooter` en una estructura de columna que ocupe el 100% del viewport.
- **FR-003**: `AppHeader` DEBE incluir: botón de menú hamburguesa, nombre y tagline de la app, chip con la sección activa, selector de idioma (ES/EN) y avatar de usuario con menú de perfil.
- **FR-004**: El menú de perfil del header DEBE incluir: nombre y apellido del usuario, enlace a perfil y botón de logout. El switch de tema claro/oscuro NO debe incluirse. El botón de logout DEBE renderizarse visualmente pero su acción queda sin implementar en esta feature (handler vacío marcado como TODO para integración con autenticación).
- **FR-005**: El drawer de navegación lateral DEBE listar todos los módulos de la app con su ícono y etiqueta traducida, marcando visualmente el módulo activo. Los módulos son: overview, workout, progress, routines, diets, meals, settings, administración.
- **FR-006**: `AppFooter` DEBE mostrar el nombre de la app y la descripción del año actual usando claves i18n.
- **FR-007**: `HomePage` DEBE renderizar cuatro secciones: hero (chip + título dos líneas + summary), cards de funcionalidades (3 cards), flujo habitual (4 pasos) y CTA final.
- **FR-008**: Todos los textos visibles en `AppHeader`, `AppFooter` y `HomePage` DEBEN obtenerse exclusivamente via `useTranslation()` — ningún string en español o inglés hardcodeado en el componente.
- **FR-009**: El selector de idioma en el header DEBE cambiar el idioma activo de i18next al seleccionar una opción, afectando toda la interfaz sin recarga de página.
- **FR-010**: `AppPreferencesContext` DEBE proveer: datos de perfil (`firstName`, `lastName`, `email`, `photoUrl`) con valores vacíos por defecto, y `themeMode` (`'light'` | `'dark'`) derivado de `window.matchMedia('(prefers-color-scheme: dark)')`. No se expone `toggleThemeMode()` ni ningún mecanismo de cambio manual de tema.
- **FR-011**: `App.tsx` DEBE envolver la app en `BrowserRouter`, proveer `AppPreferencesContext` e inicializar el tema desde `themeMode` del contexto.
- **FR-012**: Los componentes `AppHeader`, `AppFooter` y `AppLayout` DEBEN ubicarse en `components/AppHeader/`, `components/AppFooter/` y `components/AppLayout/` respectivamente, según la estructura constitucional. Los archivos previos en `shared/components/` DEBEN eliminarse en esta misma feature.
- **FR-013**: La página de bienvenida DEBE ubicarse en `pages/home/HomePage.tsx`.
- **FR-014**: El contexto DEBE ubicarse en `context/AppPreferencesContext.tsx`.
- **FR-015**: El layout DEBE ser responsive: a partir de 600 px de ancho el layout es de columna única apilada; por encima de ese umbral puede aprovechar el espacio horizontal.
- **FR-016**: Los controles interactivos clave del header DEBEN incluir atributo `aria-label` descriptivo: botón de menú hamburguesa, selector de idioma y botón de avatar/perfil.

### Key Entities

- **AppPreferences**: Agrupa `themeMode` y `profile`. No es persistida en base de datos; vive en memoria de sesión (Context + estado local). Perfil vacío por defecto hasta que haya autenticación real.
- **NavigationLink**: Representa un ítem del menú lateral: ruta destino, clave de etiqueta i18n e ícono. Lista definida estáticamente en el componente.

### External Integrations & Data Boundaries

- **INT-001**: Esta feature no consume APIs externas ni Firebase en runtime. Todo el contenido es estático i18n + estado local de Context.
- **INT-002**: Los datos de perfil son valores por defecto vacíos; la integración con Firebase Auth queda fuera del alcance de esta feature.
- **INT-003**: No aplica fallback de proveedor externo en esta feature.
- **INT-004**: No hay entidades de catálogo externo en esta feature.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El usuario puede ver la página de bienvenida completa (header + contenido + footer) en menos de 2 segundos desde que abre la URL en desarrollo local.
- **SC-002**: El cambio de idioma (ES ↔ EN) se aplica a todos los textos visibles en menos de 200 ms, sin recarga de página.
- **SC-003**: El layout no presenta desbordamiento horizontal (overflow-x) en ningún ancho de ventana entre 320 px y 1440 px.
- **SC-004**: El 100% de los textos visibles en header, footer y home page provienen del sistema i18n (verificable buscando strings literales en los componentes).
- **SC-005**: El proyecto compila sin errores de TypeScript (`tsc --noEmit`) tras implementar esta feature.
- **SC-006**: Los componentes `AppHeader`, `AppFooter`, `AppLayout` y `HomePage` residen únicamente en las rutas constitucionales (`components/` y `pages/`), sin duplicados en `shared/components/`.

---

## Assumptions

- El perfil de usuario arranca vacío; no hay autenticación real en esta feature. La integración con Firebase Auth es una feature posterior.
- La lista de módulos de navegación es fija y definida en el propio componente `AppHeader`; no se obtiene de una API.
- Los archivos `shared/components/AppHeader.tsx`, `AppFooter.tsx` y `AppLayout.tsx` serán eliminados como parte de esta feature, una vez que los nuevos componentes en `components/` estén implementados.
- El tema oscuro/claro no es configurable por el usuario desde la web; se lee exclusivamente de `prefers-color-scheme` del sistema operativo. No se escribe ni lee `localStorage` para el tema.
- React Router v7 (`react-router-dom ^7`) ya está instalado en el proyecto.
- El sistema i18n (`i18next` + `react-i18next`) ya está configurado en `i18n/i18n.ts` con las claves necesarias en ES y EN.

---

## Clarifications

### Session 2026-05-15

- Q: ¿Debe esta feature eliminar los archivos de `shared/components/` (AppHeader, AppFooter, AppLayout) o dejarlos para la feature 003? → A: Esta feature los elimina.
- Q: ¿Qué hace el botón de logout en el menú de perfil dado que no hay autenticación real en esta feature? → A: Se renderiza visualmente con handler vacío marcado como TODO para auth.
- Q: ¿Qué nivel de accesibilidad se requiere en header y home page? → A: `aria-label` en controles interactivos clave (botón menú, selector idioma, avatar); sin auditoría WCAG completa en esta feature.
- Q: ¿El usuario puede cambiar el tema claro/oscuro desde la web? → A: No. El tema se lee de `prefers-color-scheme` del sistema operativo; no hay switch de tema en la UI.
