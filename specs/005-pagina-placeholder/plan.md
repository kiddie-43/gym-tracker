# Implementation Plan: Página Placeholder para Módulos Pendientes

**Branch**: `005-pagina-placeholder` | **Date**: 2026-05-15 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/005-pagina-placeholder/spec.md`

## Summary

Crear el componente `PlaceholderPage` (frontend, sin estado ni API) y registrar 8 rutas bajo `AppLayout` en `App.tsx`, de modo que todos los módulos del menú lateral muestren una página coherente (header + contenido + footer) en lugar de una pantalla en blanco. Añadir claves i18n `placeholder.*` a `resources.ts`. Sin dependencias nuevas, sin cambios en backend.

## Technical Context

**Language/Version**: TypeScript 5.8 · React 19  
**Primary Dependencies**: MUI v7 · react-router-dom v7.6 · i18next v26 + react-i18next v17 · Vite 6  
**Storage**: N/A  
**Testing**: Vitest v3 · @testing-library/react v16  
**Target Platform**: Web SPA (browser — Chromium, Firefox, Safari)  
**Project Type**: Web application (frontend SPA)  
**Performance Goals**: N/A — componente estático, sin operaciones asíncronas  
**Constraints**: TypeScript strict (sin `any`); MUI exclusivo para UI; i18next para todos los textos; páginas controlan su propio Container  
**Scale/Scope**: 1 componente nuevo · 8 entradas de ruta · 2 adiciones al namespace i18n · 0 dependencias nuevas

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Estado | Justificación |
|-----------|--------|---------------|
| I. API-First (OpenAPI) | **N/A** | Feature 100% frontend. Sin endpoints nuevos ni cambios en contratos. |
| II. Security (Firebase Auth) | **N/A** | Sin cambios en autenticación ni en rutas protegidas. PlaceholderPage no expone datos sensibles. |
| III. Quality — TypeScript strict + MUI | ✅ **PASS** | Componente tipado con interface de props; MUI para todos los elementos visuales. |
| III. Quality — Estados carga/error/vacío | **N/A** | PlaceholderPage es estático, sin llamadas a API. Estos estados no aplican. |
| IV. Data Integrity (Wger/Firebase) | **N/A** | Sin operaciones de datos. |
| V. Observability | **N/A** | Feature frontend estática; sin métricas ni logging requeridos. |

**GATE resultado**: ✅ Sin violaciones. No se requiere justificación de complejidad.

**Re-evaluación post-diseño (Fase 1)**: Sin cambios. El diseño es consistente con el Technical Context y el Constitution Check inicial.

## Project Structure

### Documentation (this feature)

```text
specs/005-pagina-placeholder/
├── plan.md              ← este archivo
├── research.md          ← Fase 0 (generado)
├── data-model.md        ← Fase 1 (generado)
├── quickstart.md        ← Fase 1 (generado)
└── tasks.md             ← generado ✅
```

### Source Code

```text
src/frontend/src/
├── pages/
│   └── placeholder/
│       └── PlaceholderPage.tsx     ← CREAR
├── i18n/
│   └── resources.ts               ← MODIFICAR: añadir placeholder.* (ES + EN)
└── App.tsx                        ← MODIFICAR: registrar 8 rutas + import
```

## Complexity Tracking

Sin violaciones constitucionales. No se requiere justificación de complejidad.

## Implementation Design

### Componente: `PlaceholderPage`

**Ubicación**: `src/frontend/src/pages/placeholder/PlaceholderPage.tsx`

**Props interface**:
```ts
interface PlaceholderPageProps {
  labelKey: string; // clave i18n del módulo, p. ej. 'nav.workout'
}
```

**Render tree**:
```
Container (maxWidth="sm", textAlign="center", py={xs:6, md:10})
  └── Stack (spacing=3, alignItems="center")
        ├── Typography variant="h4" fontWeight=700
        │     t('placeholder.title', { module: t(labelKey) })
        ├── Chip label=t('placeholder.status') color="warning"
        └── Typography variant="body1" color="text.secondary"
              t('placeholder.message')
```

### i18n: Claves nuevas en `resources.ts`

```ts
// Añadir en es.translation y en.translation:
placeholder: {
  title: '{{module}} está en desarrollo',  // EN: '{{module}} is in development'
  status: 'En desarrollo',                 // EN: 'In development'
  message: 'Este módulo estará disponible próximamente.',
  // EN: 'This module will be available soon.'
},
```

`{{module}}` es un parámetro de interpolación i18next — se resuelve con `t(labelKey)` en tiempo de ejecución.

### Rutas: Cambios en `App.tsx`

Import a añadir:
```ts
import { PlaceholderPage } from './pages/placeholder/PlaceholderPage';
```

Dentro de `<Route path="/" element={<AppLayout />}>`, tras `<Route index element={<HomePage />} />`:
```tsx
<Route path="workouts"       element={<PlaceholderPage labelKey="nav.workout" />} />
<Route path="progress"       element={<PlaceholderPage labelKey="nav.progress" />} />
<Route path="routines"       element={<PlaceholderPage labelKey="nav.routines" />} />
<Route path="diets"          element={<PlaceholderPage labelKey="nav.diets" />} />
<Route path="meals"          element={<PlaceholderPage labelKey="nav.meals" />} />
<Route path="settings"       element={<PlaceholderPage labelKey="nav.settings" />} />
<Route path="administracion" element={<PlaceholderPage labelKey="nav.administracion" />} />
<Route path="profile"        element={<PlaceholderPage labelKey="nav.profile" />} />
```

> **Nota `/profile`**: No está en el array `links[]` de AppHeader. El chip mostrará `t('header.livePlan')` al navegar a `/profile`. Comportamiento esperado; no requiere cambios en AppHeader.

## Testing Strategy

| Tipo | Alcance | Criterio |
|------|---------|---------|
| Compilación (`tsc --noEmit`) | Proyecto completo | Sin errores de tipos — SC-003 |
| Manual — navegación | 8 rutas | Header + contenido + footer visible — SC-001 |
| Manual — i18n | Cambio ES↔EN en ruta placeholder | Textos actualizados sin recarga — SC-002 |
| Unit test (recomendado) | `PlaceholderPage` render | Renderiza título correcto con `labelKey` dado |

Los tests unitarios son recomendados pero no bloqueantes para SC-003.

