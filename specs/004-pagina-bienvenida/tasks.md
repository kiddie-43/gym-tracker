# Tasks: Página de Bienvenida con Layout Principal

**Input**: Design documents from `specs/004-pagina-bienvenida/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, quickstart.md ✓

**Skills aplicables**: `frontend-architecture`, `frontend-components`  
**Tests**: No solicitados en el spec. Validación manual mediante quickstart.md.

---

## Phase 1: Setup

**Purpose**: Tipos de dominio compartidos entre componentes y contexto.

- [X] T001 [P] Crear `src/frontend/src/interfaces/preferences/preferences.ts` con interfaces `UserProfile` y `AppPreferences` según data-model.md [skill: frontend-components, frontend-architecture]

---

## Phase 2: Foundational (Bloqueante para todas las User Stories)

**Purpose**: `AppPreferencesContext` es importado por `AppHeader` y consumido por `App.tsx`. Ningún componente de esta feature puede compilar sin él.

**⚠️ CRÍTICO**: Ninguna user story puede comenzar hasta que esta fase esté completa.

- [X] T002 Crear `src/frontend/src/context/AppPreferencesContext.tsx` — exporta `AppPreferencesContext`, `AppPreferencesProvider` y `useAppPreferences()`; provee `profile` con valores vacíos por defecto y `themeMode` derivado de `window.matchMedia('(prefers-color-scheme: dark)')` con listener reactivo en `useEffect` y cleanup en return [skill: frontend-components]

**Checkpoint**: `AppPreferencesContext` exportado y tipado — los componentes pueden importar `useAppPreferences()`.

---

## Phase 3: User Story 1 — Visualizar la página de bienvenida (Priority: P1) 🎯 MVP

**Goal**: El usuario ve la página de bienvenida completa (header + contenido + footer) en la ruta `/`.

**Independent Test**: Arrancar `npm run dev` en `src/frontend/`, navegar a `http://localhost:5173/` y verificar que se renderizan header, hero, 3 cards, flujo, CTA y footer en idioma español.

- [X] T003 [P] [US1] Crear `src/frontend/src/components/AppFooter/AppFooter.tsx` — `<Box component="footer">` con nombre de app y descripción con año vía `useTranslation()` (`app.name`, `footer.description`); estilo con gradiente verde constitucional [skill: frontend-components]
- [X] T004 [P] [US1] Crear `src/frontend/src/pages/home/HomePage.tsx` — cuatro secciones: hero (Chip + dos líneas de título + summary), cards de funcionalidades (3 cards con tag/título/descripción), flujo habitual (4 pasos), CTA final; todos los textos vía claves `home.*` de i18n; layout responsive con MUI `Container` y `Grid` [skill: frontend-components, frontend-architecture]
- [X] T005 [US1] Crear `src/frontend/src/components/AppHeader/AppHeader.tsx` — `<AppBar position="sticky">` con nombre de app, tagline, chip de sección activa (compara `location.pathname` con la lista de `NavigationLink`; si no hay coincidencia muestra `t('header.livePlan')`), avatar de usuario con menú de perfil (nombre/apellido del contexto, enlace a `/profile`, botón logout con handler vacío `// TODO: integrar con auth`); `aria-label` en botón de avatar; sin drawer ni selector de idioma aún [skill: frontend-components]
- [X] T006 [US1] Crear `src/frontend/src/components/AppLayout/AppLayout.tsx` — `<Box sx={{ height: '100dvh', display: 'flex', flexDirection: 'column', overflowX: 'hidden' }}>` con `<AppHeader />`, `<Container sx={{ overflowX: 'hidden' }}>` con `<Outlet />` y `<AppFooter />`; importa desde `components/AppHeader` y `components/AppFooter`; verificar que no hay desbordamiento horizontal entre 320 px y 1440 px (SC-003) [skill: frontend-components, frontend-architecture]
- [X] T007 [US1] Actualizar `src/frontend/src/App.tsx` — crear componente interno `AppShell` que consuma `useAppPreferences()`, calcule `useMemo(() => getAppTheme(themeMode), [themeMode])` y renderice `<ThemeProvider theme={theme}><CssBaseline /><BrowserRouter><Routes><Route path="/" element={<AppLayout />}><Route index element={<HomePage />} /></Route></Routes></BrowserRouter></ThemeProvider>`; el componente raíz `App` solo provee el contexto: `<AppPreferencesProvider><AppShell /></AppPreferencesProvider>`. **Nota**: `AppShell` debe ser un componente hijo independiente — no se puede usar `useAppPreferences()` en el mismo componente que renderiza `<AppPreferencesProvider>` (React anti-pattern) [skill: frontend-components, frontend-architecture]

**Checkpoint**: La ruta `/` muestra header + hero + cards + flujo + CTA + footer. `tsc --noEmit` sin errores.

---

## Phase 4: User Story 2 — Cambiar el idioma desde el header (Priority: P1)

**Goal**: El usuario cambia el idioma (ES ↔ EN) desde el header sin recargar la página y todos los textos se actualizan.

**Independent Test**: Seleccionar "EN" en el selector y verificar que todos los textos del header, home y footer cambian al inglés. Seleccionar "ES" y verificar que vuelven al español.

- [X] T008 [US2] Añadir selector de idioma a `src/frontend/src/components/AppHeader/AppHeader.tsx` — `<Select>` de MUI con valores `"es"` / `"en"`, controlado por `i18n.resolvedLanguage`, con handler `i18n.changeLanguage(value)`; `aria-label={t('header.languageLabel')}`; estilo coherente con la AppBar [skill: frontend-components]

**Checkpoint**: El selector cambia el idioma y todos los textos se actualizan reactivamente.

---

## Phase 5: User Story 3 — Navegar mediante el menú lateral (Priority: P2)

**Goal**: El usuario abre el drawer lateral, ve todos los módulos y navega haciendo clic.

**Independent Test**: Hacer clic en el botón hamburguesa, verificar que el drawer se abre con todos los ítems de navegación, hacer clic en uno y verificar que la URL cambia y el drawer se cierra.

- [X] T009 [US3] Añadir drawer de navegación a `src/frontend/src/components/AppHeader/AppHeader.tsx` — botón hamburguesa con `aria-label={t('header.menuLabel')}` que abre un `<Drawer anchor="left">`; lista de `NavigationLink` (overview, workout, progress, routines, diets, meals, settings, admin) con íconos MUI y etiquetas `t(link.labelKey)`; ítem activo resaltado comparando `location.pathname`; cierra al hacer clic en un ítem vía `<NavLink>` + `setIsMenuOpen(false)` [skill: frontend-components, frontend-architecture]

**Checkpoint**: Drawer abre y cierra, navegación funciona, ítem activo resaltado.

---

## Phase 6: User Story 4 — Respetar el tema visual del sistema operativo (Priority: P2)

**Goal**: La interfaz adopta automáticamente el tema del SO; sin toggle en la UI.

**Independent Test**: Con SO en modo oscuro, verificar que la app arranca con tema oscuro. Cambiar el SO a modo claro y recargar; verificar tema claro. Cambiar el SO durante la sesión y verificar que el tema se actualiza sin recargar.

- [X] T010 [US4] Crear test de componente en `src/frontend/tests/component/AppPreferencesContext.test.tsx` — (1) simular `window.matchMedia` con dark mode activo y verificar que `themeMode` devuelve `'dark'`; (2) simular evento `change` en el media query listener y verificar que `themeMode` se actualiza reactivamente sin recargar; (3) verificar que el listener se limpia al desmontar el provider (`removeEventListener` llamado) [skill: frontend-components]

**Checkpoint**: Tema claro/oscuro derivado del SO, actualización reactiva funciona, no hay localStorage involucrado.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Limpieza de archivos obsoletos y validación final de compilación y constitución.

- [X] T011 [P] Eliminar `src/frontend/src/shared/components/AppHeader.tsx` (reemplazado por `components/AppHeader/AppHeader.tsx`) [skill: frontend-architecture]
- [X] T012 [P] Eliminar `src/frontend/src/shared/components/AppFooter.tsx` (reemplazado por `components/AppFooter/AppFooter.tsx`) [skill: frontend-architecture]
- [X] T013 [P] Eliminar `src/frontend/src/shared/components/AppLayout.tsx` (reemplazado por `components/AppLayout/AppLayout.tsx`) [skill: frontend-architecture]
- [X] T014 Buscar imports restantes de `shared/components/AppHeader`, `AppFooter` o `AppLayout` en todo el proyecto y actualizarlos a las rutas constitucionales en `components/` [skill: frontend-architecture]
- [X] T015 Ejecutar `npx tsc --noEmit` en `src/frontend/` y resolver cualquier error de TypeScript [skill: spec-kit-workflow]
- [X] T016 Validar checklist completo de `specs/004-pagina-bienvenida/quickstart.md` (16 ítems en verde) [skill: spec-kit-workflow]

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Sin dependencias — puede iniciarse de inmediato
- **Phase 2 (Foundational)**: Depende de Phase 1 — **BLOQUEA** todas las user stories
- **Phase 3 (US1)**: Depende de Phase 2 — puede iniciarse en paralelo con T003 y T004
- **Phase 4 (US2)**: Depende de T005 (AppHeader base) en Phase 3
- **Phase 5 (US3)**: Depende de T008 (language selector) en Phase 4
- **Phase 6 (US4)**: Depende de T002 (contexto) y T007 (App.tsx) — verificación
- **Phase 7 (Polish)**: Depende de todas las fases anteriores completas

### User Story Dependencies

- **US1 (P1)**: Puede iniciarse tras Phase 2 — no depende de otras US
- **US2 (P1)**: Depende de T005 (AppHeader shell de US1)
- **US3 (P2)**: Depende de T008 (language selector de US2)
- **US4 (P2)**: Depende de T002 (Phase 2) + T007 (US1) — es verificación

### Within Each User Story

- T003 y T004 son independientes entre sí → ejecutables en paralelo [P]
- T005 depende de T002 (importa AppPreferencesContext)
- T006 depende de T005 (importa AppHeader)
- T007 depende de T005 + T006 (importa AppLayout + AppPreferencesContext)
- T011, T012, T013 son independientes entre sí → ejecutables en paralelo [P]

### Parallel Opportunities

| Grupo | Tareas paralelas |
|---|---|
| Phase 3 arranque | T003 y T004 (archivos distintos, sin dependencia mutua) |
| Phase 7 cleanup | T011, T012, T013 (archivos distintos) |

---

## Implementation Strategy

**MVP scope**: Phases 1 + 2 + 3 = base navegable completa (US1 entregable independiente)  
**Increment 2**: Phase 4 = multiidioma operativo (US2)  
**Increment 3**: Phase 5 = navegación completa (US3)  
**Increment 4**: Phase 6 + 7 = tema OS verificado + cleanup (US4 + polish)

**Total**: 16 tareas · 6 tasks [P] · 2 oportunidades de paralelismo
