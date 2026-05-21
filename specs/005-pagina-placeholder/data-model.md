# Data Model: Página Placeholder para Módulos Pendientes

**Feature**: 005-pagina-placeholder  
**Date**: 2026-05-15  
**Scope**: Frontend-only. No new persistent entities, no API changes.

---

## Componente: PlaceholderPage

**Tipo**: Componente React de presentación (sin estado, sin efectos, sin llamadas a API).  
**Ubicación**: `src/frontend/src/pages/placeholder/PlaceholderPage.tsx`  
**Ciclo de vida**: Temporal — se reemplaza por la implementación real del módulo en su feature correspondiente.

### Props

```ts
interface PlaceholderPageProps {
  labelKey: string; // clave i18n del módulo, p. ej. 'nav.workout'
}
```

### Render tree

```
Container (maxWidth="sm", textAlign="center", py={xs:6, md:10})
  └── Stack (spacing=3, alignItems="center")
        ├── Typography variant="h4"   ← t('placeholder.title', { module: t(labelKey) })
        ├── Chip label=t('placeholder.status')  ← indicador visual de estado
        └── Typography variant="body1" color="text.secondary"  ← t('placeholder.message')
```

### Reglas de validación

- `labelKey` debe ser una clave válida en el namespace de traducción activo.  
  Si la clave no existe, i18next devuelve la clave en bruto (fallback de librería) — la app no lanza error.

---

## i18n: Claves nuevas en `resources.ts`

Namespace `translation`, sección `placeholder`:

| Clave | ES | EN |
|-------|----|----|
| `placeholder.title` | `'{{module}} está en desarrollo'` | `'{{module}} is in development'` |
| `placeholder.status` | `'En desarrollo'` | `'In development'` |
| `placeholder.message` | `'Este módulo estará disponible próximamente.'` | `'This module will be available soon.'` |

`{{module}}` es un parámetro de interpolación i18next; se resuelve en tiempo de ejecución con `t(labelKey)`.

---

## Rutas: Registros nuevos en `App.tsx`

Todas las rutas nuevas se añaden bajo `<Route path="/" element={<AppLayout />}>`. No se modifica `AppLayout` ni `AppHeader`.

| Path | Componente | `labelKey` |
|------|------------|------------|
| `workouts` | `<PlaceholderPage />` | `nav.workout` |
| `progress` | `<PlaceholderPage />` | `nav.progress` |
| `routines` | `<PlaceholderPage />` | `nav.routines` |
| `diets` | `<PlaceholderPage />` | `nav.diets` |
| `meals` | `<PlaceholderPage />` | `nav.meals` |
| `settings` | `<PlaceholderPage />` | `nav.settings` |
| `administracion` | `<PlaceholderPage />` | `nav.administracion` |
| `profile` | `<PlaceholderPage />` | `nav.profile` |

> **Nota sobre `/profile`**: No está en el array `links[]` del AppHeader (se accede via menú de avatar). El chip del header mostrará `t('header.livePlan')` al navegar a `/profile`. Esto es comportamiento esperado — no requiere cambios en AppHeader.

---

## Archivos modificados / creados

| Archivo | Operación | Descripción |
|---------|-----------|-------------|
| `src/frontend/src/pages/placeholder/PlaceholderPage.tsx` | CREATE | Nuevo componente |
| `src/frontend/src/i18n/resources.ts` | MODIFY | Añadir `placeholder.*` en ES + EN |
| `src/frontend/src/App.tsx` | MODIFY | Registrar 8 rutas nuevas + import |

**Dependencias nuevas**: ninguna. Todos los packages (MUI, react-i18next, react-router-dom) ya están instalados.
