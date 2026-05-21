# Tasks: Paneles de Administración con Tabs

**Input**: Design documents from `specs/006-paneles-administracion/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Exact file paths included in all descriptions

---

## Phase 1: User Story 1 — Navegar entre paneles de administración (Priority: P1) 🎯 MVP

**Story goal**: Reemplazar `PlaceholderPage` en `/administracion` con una página real que tenga 4 pestañas MUI navegables; cada pestaña muestra un contenido placeholder.

**Independent test criteria**: Abrir `/administracion`, verificar que hay 4 tabs con los textos correctos, hacer clic en "Músculos" y confirmar que el contenido del panel cambia.

- [X] T001 [P] [US1] Crear `AdministrationPage.tsx` en `src/frontend/src/pages/administration/AdministrationPage.tsx` con `useState<number>(0)`, `<Tabs>` con 4 `<Tab>` usando las claves `administration.tabs.*` de i18n, y contenido placeholder por tab activa mediante helper interno `TabPlaceholder`
- [X] T002 [P] [US1] Crear `AdministrationPage.test.tsx` en `src/frontend/tests/component/AdministrationPage.test.tsx` cubriendo: (1) renderiza las 4 pestañas con textos correctos, (2) la primera pestaña está activa por defecto, (3) al hacer clic en la segunda pestaña el contenido cambia al panel correspondiente
- [X] T003 [US1] Actualizar `src/frontend/src/App.tsx`: reemplazar `<PlaceholderPage labelKey="nav.administracion" />` por `<AdministrationPage />` en la ruta `path="administracion"` e importar `AdministrationPage`
- [X] T004 [US1] Verificar que `cd src/frontend && npx tsc --noEmit` pasa sin errores y que los tests de `AdministrationPage.test.tsx` pasan con `npm test`

---

## Dependencies

```
T001 ──┐
       ├──▶ T003 ──▶ T004
T002 ──┘
```

T001 y T002 son independientes entre sí (ficheros distintos) → ejecutar en paralelo.  
T003 requiere que T001 exista (importación).  
T004 valida T001 + T002 + T003 juntos.

## Parallel Execution

```
Worker A: T001 → T003 → T004
Worker B: T002 ──────────────▶ (merge en T004)
```

## Implementation Strategy

**MVP = Esta feature completa** — hay un único user story, se entrega íntegro.

Orden recomendado para un único desarrollador:
1. T001 — componente primero
2. T002 — test inmediatamente después del componente
3. T003 — conectar la ruta
4. T004 — validación final (tsc + tests)
