# Research: Paneles de Administración con Tabs

**Feature**: 006-paneles-administracion  
**Date**: 2026-05-15  
**Status**: Complete — sin incógnitas

## Resultado

No se requirió investigación externa. Todas las decisiones técnicas se resolvieron con el contexto existente del proyecto.

## Decisiones

### MUI Tabs — API v7

- **Decision**: Usar `<Tabs value={number}>` + `<Tab>` con índice numérico (`useState<number>`)
- **Rationale**: La API de MUI v7 Tabs es estándar y bien documentada; el índice numérico es más simple que valores string para un componente sin persistencia en URL.
- **Alternatives considered**: `value` como string slug (p. ej. `"muscles"`) — rechazado porque añade complejidad innecesaria sin beneficio cuando no hay URL binding.

### Contenido de cada pestaña

- **Decision**: Helper interno `TabPlaceholder` en el mismo archivo, no exportado.
- **Rationale**: Un solo archivo, sin archivos adicionales. El placeholder es específico de esta página; no es un componente reutilizable en otro contexto todavía.
- **Alternatives considered**: Reutilizar `PlaceholderPage` directamente — rechazado porque `PlaceholderPage` espera prop `labelKey` con claves `nav.*` y muestra layout de página completa (Container centrado), que no encaja dentro de un tabpanel.

### Ubicación del archivo

- **Decision**: `src/frontend/src/pages/administracion/AdministracionPage.tsx`
- **Rationale**: Skill `frontend-architecture` — las vistas que el router renderiza van en `pages/`.
- **Alternatives considered**: `features/administracion/` — rechazado porque `features/` es para lógica con estado externo o componentes de dominio; esta página es solo estructura de navegación.

### Sin Redux / Sin servicios

- **Decision**: `useState` local — sin slice Redux, sin llamada a API.
- **Rationale**: El estado de la tab activa (un entero) es puramente local a la página y no necesita ser compartido con ningún otro módulo.
- **Alternatives considered**: Redux slice `administracion` — rechazado como sobre-ingeniería para un booleano de índice local.
