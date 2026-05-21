# Implementation Plan: Página de Bienvenida con Layout Principal

**Branch**: `004-pagina-bienvenida` | **Date**: 2026-05-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/004-pagina-bienvenida/spec.md`

## Summary

Establecer la base navegable mínima del frontend: contexto de preferencias (`AppPreferencesContext` con tema desde OS via `prefers-color-scheme`), layout principal (`AppHeader` + `AppFooter` + `AppLayout` en rutas constitucionales), enrutamiento base (`BrowserRouter`, ruta `/`), y página de bienvenida (`HomePage`) con hero, 3 cards de funcionalidades, flujo típico y CTA. Todos los textos via i18n (ES/EN). Los componentes previos en `shared/components/` se eliminan en esta misma feature.

## Technical Context

**Language/Version**: TypeScript 5.8 (strict), React 19  
**Primary Dependencies**: MUI v7, react-router-dom v7.6, i18next v26 + react-i18next v17, Vite v6  
**Storage**: N/A — sin persistencia en esta feature (themeMode en memoria; no localStorage)  
**Testing**: Vitest v3 + @testing-library/react v16  
**Target Platform**: Web browser (SPA, client-side rendering)  
**Project Type**: Web application (frontend)  
**Performance Goals**: Renderizado inicial < 2 s; cambio de idioma < 200 ms  
**Constraints**: Sin nuevas librerías; solo MUI; TypeScript strict (sin `any` no justificado)  
**Scale/Scope**: 1 página, 3 componentes reutilizables, 1 contexto, 1 archivo de interfaces

## Constitution Check

*GATE: Evaluado antes de Phase 0 y confirmado tras Phase 1.*

| Principio | Aplica | Estado |
|---|---|---|
| I. API-First / OpenAPI | No — feature puramente frontend, sin nuevos endpoints | SKIP |
| II. Seguridad Firebase Auth | No — sin auth en scope | SKIP |
| III. Calidad (TS strict, MUI, accesibilidad, estados) | Sí | PASS — FR-003, FR-016; componentes con aria-labels; TypeScript strict |
| IV. Integridad Wger/Firebase | No — sin datos externos | SKIP |
| V. Observabilidad / Progreso medible | No — sin backend | SKIP |

Sin violaciones. Sin entradas en Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/004-pagina-bienvenida/
├── plan.md         ← este archivo
├── research.md     ← Phase 0 (generado)
├── data-model.md   ← Phase 1 (generado)
├── quickstart.md   ← Phase 1 (generado)
└── tasks.md        ← Phase 2 (generado por /speckit.tasks)
```

### Source Code

```text
src/frontend/src/
├── context/
│   └── AppPreferencesContext.tsx        ← NUEVO
├── components/
│   ├── AppHeader/
│   │   └── AppHeader.tsx                ← NUEVO
│   ├── AppFooter/
│   │   └── AppFooter.tsx                ← NUEVO
│   └── AppLayout/
│       └── AppLayout.tsx                ← NUEVO
├── pages/
│   └── home/
│       └── HomePage.tsx                 ← NUEVO
├── interfaces/
│   └── preferences/
│       └── preferences.ts               ← NUEVO
├── shared/
│   └── components/
│       ├── AppHeader.tsx                ← ELIMINAR
│       ├── AppFooter.tsx                ← ELIMINAR
│       └── AppLayout.tsx                ← ELIMINAR
└── App.tsx                              ← MODIFICAR
```

**Structure Decision**: Proyecto web SPA (solo frontend). Sin cambios en backend. Estructura de carpetas sigue la constitución: `components/` para reutilizables, `pages/` para vistas, `context/` para contextos globales, `interfaces/` para tipos de dominio.
