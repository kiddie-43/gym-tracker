---
description: "Task list for feature 005-pagina-placeholder"
---

# Tasks: Página Placeholder para Módulos Pendientes

**Input**: Design documents from `/specs/005-pagina-placeholder/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · quickstart.md ✅

**Note on tests**: No test tasks generated — spec does not request TDD; PlaceholderPage is a static presentational component. Compilation (`tsc --noEmit`) serves as the primary quality gate.

**Organization**: Phase 1 omitted (no project initialization needed — feature is purely additive). Phases 2→3 cover the two user stories.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1, US2)
- Paths are workspace-relative from `src/frontend/src/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core artifacts that MUST be complete before user story implementation. Both US1 and US2 depend on these.

**⚠️ CRITICAL**: T001 and T002 can run in parallel (different files). T003 depends on T002.

- [X] T001 [P] Add `placeholder.*` i18n keys (ES + EN) to `src/frontend/src/i18n/resources.ts`
- [X] T002 [P] Create `PlaceholderPage` component in `src/frontend/src/pages/placeholder/PlaceholderPage.tsx`

**Checkpoint**: `PlaceholderPage` renders and i18n keys resolve → ready for route registration

---

## Phase 3: User Story 1 — Navegar a un módulo sin implementar (Priority: P1) 🎯 MVP

**Goal**: Cualquier ruta del menú lateral muestra header + contenido (`PlaceholderPage`) + footer en lugar de pantalla en blanco.

**Independent Test**: Arrancar la app (`npm run dev`), navegar a `/workouts` → ver página con header, chip de sección activo "Entrenamiento", cuerpo con "[Módulo] está en desarrollo" y footer. Repetir para las 8 rutas.

- [X] T003 [US1] Import `PlaceholderPage` and register 8 module routes under `AppLayout` in `src/frontend/src/App.tsx`

**Checkpoint**: Las 8 rutas muestran header + `PlaceholderPage` + footer. SC-001 ✅

---

## Phase 4: User Story 2 — Cambiar idioma en página placeholder (Priority: P2)

**Goal**: El selector de idioma actualiza todos los textos del placeholder sin recargar la página.

**Independent Test**: Navegar a `/routines` → cambiar a EN → verificar "Routines is in development" → cambiar a ES → verificar "Rutinas está en desarrollo".

> **Nota**: US2 se entrega íntegramente con el trabajo fundacional (T001 + T002). El hook `useTranslation()` y la interpolación `{{module}}` en `placeholder.title` garantizan la reactividad automática al cambiar idioma. No se requieren tareas de implementación adicionales para esta historia.

**Checkpoint**: Cambio de idioma actualiza textos sin recarga. SC-002 ✅

---

## Final Phase: Polish & Cross-Cutting

**Purpose**: Validación transversal post-implementación

- [X] T004 Run `tsc --noEmit` from `src/frontend/` and verify zero type errors — SC-003

---

## Dependencies

```
T001 ─┐
      ├─→ T003 (routes need component + keys to be ready)
T002 ─┘

T004 depends on T001 + T002 + T003 (full compilation check)
```

**Parallel opportunities**:
- T001 ∥ T002 (independent files: `resources.ts` vs `PlaceholderPage.tsx`)
- T003 → T004 (sequential)

---

## Implementation Strategy

**MVP scope (T001 → T002 → T003)**: Completes US1 AND US2 in full. No incremental delivery needed — the feature is atomic.

**Execution order**:
1. T001 ∥ T002 (in parallel)
2. T003 (after T001 + T002 complete)
3. T004 (final validation)

---

## Summary

| Phase | Task | Story | Parallel | File |
|-------|------|-------|----------|------|
| 2 Foundational | T001 | — | ✅ | `i18n/resources.ts` |
| 2 Foundational | T002 | — | ✅ | `pages/placeholder/PlaceholderPage.tsx` |
| 3 US1 (P1) | T003 | US1 | — | `App.tsx` |
| Final | T004 | — | — | (compilation) |

**Total tasks**: 4  
**US1 tasks**: 1 (+ 2 foundational prereqs)  
**US2 tasks**: 0 additional (delivered by foundational)  
**Parallel opportunities**: T001 ∥ T002  
**Suggested MVP**: All 4 tasks (feature is complete and atomic)
