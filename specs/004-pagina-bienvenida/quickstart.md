# Quickstart: Página de Bienvenida con Layout Principal

**Feature**: 004-pagina-bienvenida
**Date**: 2026-05-15

## Descripción del cambio

Establece la base navegable mínima de la app: layout principal (`AppHeader` + `AppFooter` + `AppLayout`), enrutamiento base (`BrowserRouter`, ruta `/`), página de bienvenida (`HomePage`) y contexto de preferencias (`AppPreferencesContext`). El tema visual se deriva del SO del usuario (`prefers-color-scheme`). Todos los textos usan i18n (ES/EN). Los componentes previos en `shared/components/` son reemplazados por las versiones constitucionales en `components/`.

---

## Archivos creados / modificados

### Nuevos

| Archivo | Descripción |
|---|---|
| `src/frontend/src/context/AppPreferencesContext.tsx` | Contexto con `profile` (vacío) y `themeMode` desde OS |
| `src/frontend/src/components/AppHeader/AppHeader.tsx` | Header con drawer, idioma, avatar; aria-labels en controles clave |
| `src/frontend/src/components/AppFooter/AppFooter.tsx` | Footer con nombre de app y año |
| `src/frontend/src/components/AppLayout/AppLayout.tsx` | Layout wrapper con Header + Outlet + Footer |
| `src/frontend/src/pages/home/HomePage.tsx` | Página de bienvenida: hero + 3 cards + flujo + CTA |
| `src/frontend/src/interfaces/preferences/preferences.ts` | Tipos `UserProfile` y `AppPreferences` |

### Modificados

| Archivo | Cambio |
|---|---|
| `src/frontend/src/App.tsx` | BrowserRouter + rutas + AppPreferencesContext.Provider + ThemeProvider dinámico |

### Eliminados

| Archivo | Razón |
|---|---|
| `src/frontend/src/shared/components/AppHeader.tsx` | Reemplazado por `components/AppHeader/AppHeader.tsx` |
| `src/frontend/src/shared/components/AppFooter.tsx` | Reemplazado por `components/AppFooter/AppFooter.tsx` |
| `src/frontend/src/shared/components/AppLayout.tsx` | Reemplazado por `components/AppLayout/AppLayout.tsx` |

---

## Comandos de validación

### Verificar compilación TypeScript

```bash
cd src/frontend
npx tsc --noEmit
```

### Ejecutar tests existentes

```bash
cd src/frontend
npm test
```

### Arrancar en desarrollo

```bash
cd src/frontend
npm run dev
```

Navegar a `http://localhost:5173/` — debe mostrarse la página de bienvenida completa.

---

## Checklist de validación manual

- [ ] La ruta `/` muestra: header + hero + 3 cards + flujo + CTA + footer.
- [ ] El selector de idioma cambia todos los textos (ES ↔ EN) sin recarga.
- [ ] El menú hamburguesa abre el drawer con todos los módulos navegables.
- [ ] El ítem activo del drawer se resalta cuando la ruta coincide.
- [ ] Con SO en modo oscuro, la app arranca con tema oscuro. Con modo claro, tema claro.
- [ ] Si el SO cambia el tema durante la sesión (sin recargar), la app se actualiza.
- [ ] El menú de perfil muestra nombre vacío y botón logout (sin acción).
- [ ] No hay overflow horizontal en 320 px, 768 px ni 1440 px de ancho.
- [ ] `tsc --noEmit` termina sin errores.
- [ ] No existen archivos `.tsx` con strings en español o inglés hardcodeados en los componentes nuevos.

---

## Estructura resultante

```
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
│       ├── AppHeader.tsx                ← ELIMINADO
│       ├── AppFooter.tsx                ← ELIMINADO
│       ├── AppLayout.tsx                ← ELIMINADO
│       ├── WorkoutImmersiveLayout.tsx   ← sin cambios
│       └── ui/                          ← sin cambios
└── App.tsx                              ← MODIFICADO
```
