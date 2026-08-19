# Research: Página Placeholder para Módulos Pendientes

**Feature**: 005-pagina-placeholder  
**Date**: 2026-05-15  
**Scope**: Frontend-only. No backend research required.

---

## Pregunta 1: ¿Cómo detecta el AppHeader la sección activa?

**Decision**: El chip usa `useLocation()` + búsqueda en el array `links[]` local — sin props ni contexto externo.

**Rationale**: En `AppHeader.tsx` (líneas 98–102):
```ts
const activeLink = links.find((link) => {
  if (link.to === '/') return location.pathname === '/';
  return location.pathname.startsWith(link.to);
});
```
Cuando `App.tsx` registre rutas como `/workouts`, `/progress`, etc., `location.pathname` coincidirá con los `to` del array `links` y el chip se actualizará automáticamente. No requiere cambios en `AppHeader`.

**Alternativas consideradas**: Pasar el activeLink como prop desde App.tsx → rechazado (innecesario; la detección por pathname ya funciona).

**Implicación para `/profile`**: La ruta `/profile` *no está* en el array `links[]` de AppHeader (se accede via menú de avatar). Al navegar a `/profile`, `activeLink` será `undefined` y el chip mostrará `t('header.livePlan')`. Esto es comportamiento aceptable; no requiere cambios.

---

## Pregunta 2: ¿Qué claves i18n se necesitan y cuáles ya existen?

**Decision**: Todas las claves `nav.*` ya existen. Solo hay que añadir `placeholder.*`.

**Claves existentes a usar como `labelKey`**:

| Ruta | `labelKey` | ES | EN |
|------|------------|----|----|
| `/workouts` | `nav.workout` | Entrenamiento | Workout |
| `/progress` | `nav.progress` | Progreso | Progress |
| `/routines` | `nav.routines` | Rutinas | Routines |
| `/diets` | `nav.diets` | Dietas | Diets |
| `/meals` | `nav.meals` | Comidas | Meals |
| `/settings` | `nav.settings` | Ajustes | Settings |
| `/administracion` | `nav.administracion` | Administración | Administration |
| `/profile` | `nav.profile` | Perfil | Profile |

**Claves nuevas a añadir** (`placeholder.*`):

| Clave | ES | EN |
|-------|----|----|
| `placeholder.title` | `'{{module}} está en desarrollo'` | `'{{module}} is in development'` |
| `placeholder.status` | `'En desarrollo'` | `'In development'` |
| `placeholder.message` | `'Este módulo estará disponible próximamente.'` | `'This module will be available soon.'` |

`placeholder.title` usa interpolación i18next (`{{module}}`) para combinar `t(labelKey)` con el texto estático, manteniendo toda la internacionalización en `resources.ts`.

**Alternativas consideradas**:
- Concatenar en TSX: `{t(labelKey)} está en desarrollo` → rechazado (el sufijo no sería traducible).
- Clave separada `placeholder.suffix` → rechazado (más complejidad sin beneficio; la interpolación es el patrón i18next estándar).

---

## Pregunta 3: ¿Cómo estructura sus páginas el proyecto?

**Decision**: Las páginas usan su propio `<Container>` — `AppLayout` no provee Container.

**Rationale**: `AppLayout` es un wrapper de layout (header + `<Outlet />` scrolleable + footer), sin Container propio. `HomePage` usa `<Container maxWidth="lg" sx={{ py: {xs:3, md:5} }}>`. `PlaceholderPage` seguirá el mismo patrón con `maxWidth="sm"` (contenido centrado, sin columnas ni grid complejos).

---

## Resolución de NEEDS CLARIFICATION

**Total marcadores**: 0 (la feature no tenía ninguno — confirmado por investigación).

Todos los detalles técnicos están resueltos. La implementación puede proceder directamente.
