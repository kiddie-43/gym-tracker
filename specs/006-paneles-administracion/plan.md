# Implementation Plan: Paneles de Administración con Tabs

**Branch**: `006-paneles-administracion` | **Date**: 2026-05-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/006-paneles-administracion/spec.md`

## Summary

Reemplazar el `PlaceholderPage` de la ruta `/administracion` con `AdministracionPage`: una página con 4 pestañas MUI (Ejercicios, Músculos, Tipo de ejercicios, Tipo de formulario). El cambio de pestaña es puramente controlado por estado local (`useState`); no modifica la URL. El contenido de cada pestaña es un placeholder. No hay cambios en backend, Redux ni servicios.

## Technical Context

**Language/Version**: TypeScript 5.8 (strict) — React 19  
**Primary Dependencies**: MUI v7 (`Tabs`, `Tab`, `Box`), react-i18next v17, react-router-dom v7.6  
**Storage**: N/A — no llamadas a API ni estado global  
**Testing**: Vitest v3 + @testing-library/react v16  
**Target Platform**: SPA web (Vite, http://localhost:5173)  
**Project Type**: Frontend-only — una nueva página React + una modificación de ruta  
**Performance Goals**: N/A — componente puramente estático sin data fetching  
**Constraints**: Sin `any`; props tipadas con interfaces; MUI v7 Grid usa prop `size` (no `xs/md`)  
**Scale/Scope**: 1 archivo nuevo + 1 línea modificada en App.tsx

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **OpenAPI-first**: ✅ N/A — no endpoints nuevos en esta feature.
- **Security-first**: ✅ N/A — no llamadas autenticadas; sin cambios de permisos. Auth guard aplica a feature futura.
- **Data boundary**: ✅ N/A — no hay lectura ni escritura de datos. Sin Wger.
- **Reliability**: ✅ N/A — sin llamadas externas ni caché.
- **Quality gates**: ✅ Test de interacción de tabs cubierto en `AdministracionPage.test.tsx` (clic entre tabs, renderizado de placeholder por pestaña activa).
- **Operations**: ✅ N/A — sin logging ni métricas para un componente de tabs puramente estático.

## Project Structure

### Documentation (this feature)

```text
specs/006-paneles-administracion/
├── plan.md              # Este archivo
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (only files changed or created)

```text
src/frontend/src/
├── pages/
│   └── administracion/
│       ├── AdministracionPage.tsx          # NUEVO — página con MUI Tabs
│       └── AdministracionPage.test.tsx     # NUEVO — test de interacción
└── App.tsx                                 # MODIFICADO — cambiar ruta /administracion
```

**Structure Decision**: Frontend-only, opción "Web application". Solo capa `pages/` — sin components/, services/, redux/ ni interfaces/ porque el componente no necesita estado externo ni APIs.

## Complexity Tracking

> Sin violaciones de constitución.

---

## Phase 0: Research

> Sin incógnitas técnicas. No se requiere investigación externa.

**Decisiones ya resueltas por el contexto del proyecto**:

| Decisión | Valor | Fuente |
|----------|-------|--------|
| Librería de tabs | MUI v7 `<Tabs>` / `<Tab>` | Proyecto usa MUI v7 exclusivamente |
| Estado activo | `useState<number>` local en la página | No hay estado global que compartir |
| Cambio de tab | `onChange` callback de `<Tabs>` | MUI API estándar |
| Claves i18n de tabs | `administration.tabs.{exercises,muscles,exerciseTypes,formTypes}` | Ya existen en `resources.ts` |
| Contenido placeholder | Reutilizar patrón de `PlaceholderPage` (Typography + Chip) | Consistencia visual con el resto |
| Ubicación del archivo | `src/frontend/src/pages/administracion/` | Skill `frontend-architecture`: vistas del router → `pages/` |

---

## Phase 1: Design

### Componente: `AdministracionPage`

```
src/frontend/src/pages/administracion/AdministracionPage.tsx
```

**Props**: ninguna (página raíz de ruta, sin props externas).

**Estado local**:
```ts
const [activeTab, setActiveTab] = useState<number>(0);
```

**Estructura de render**:
```
<Box>                          // contenedor raíz
  <Tabs value={activeTab} onChange={...}>
    <Tab label={t('administration.tabs.exercises')} />
    <Tab label={t('administration.tabs.muscles')} />
    <Tab label={t('administration.tabs.exerciseTypes')} />
    <Tab label={t('administration.tabs.formTypes')} />
  </Tabs>
  <Box role="tabpanel">        // contenido de la tab activa
    {activeTab === 0 && <TabPlaceholder labelKey="administration.tabs.exercises" />}
    {activeTab === 1 && <TabPlaceholder labelKey="administration.tabs.muscles" />}
    {activeTab === 2 && <TabPlaceholder labelKey="administration.tabs.exerciseTypes" />}
    {activeTab === 3 && <TabPlaceholder labelKey="administration.tabs.formTypes" />}
  </Box>
</Box>
```

`TabPlaceholder` es un helper interno (no exportado, definido en el mismo archivo) que renderiza un mensaje de "panel en construcción" con la clave de etiqueta de la pestaña activa.

### Modificación: `App.tsx`

Línea a cambiar:
```tsx
// Antes:
<Route path="administracion" element={<PlaceholderPage labelKey="nav.administracion" />} />

// Después:
<Route path="administracion" element={<AdministracionPage />} />
```

### Test: `AdministracionPage.test.tsx`

Casos cubiertos:
1. Renderiza las 4 pestañas con los textos correctos.
2. La primera pestaña está activa por defecto.
3. Al hacer clic en la pestaña "Músculos", el contenido cambia al panel correspondiente.

---

## Constitution Check (post-diseño)

- **OpenAPI-first**: ✅ N/A confirmado — sin endpoints.
- **Security-first**: ✅ N/A confirmado.
- **Data boundary**: ✅ N/A confirmado.
- **Reliability**: ✅ N/A confirmado.
- **Quality gates**: ✅ Test scope definido — 3 casos en `AdministracionPage.test.tsx`.
- **Operations**: ✅ N/A confirmado.
