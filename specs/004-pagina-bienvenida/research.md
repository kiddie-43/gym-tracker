# Research: Página de Bienvenida con Layout Principal

**Feature**: 004-pagina-bienvenida
**Date**: 2026-05-15

---

## 1. Lectura reactiva de `prefers-color-scheme`

**Decision**: Usar `window.matchMedia('(prefers-color-scheme: dark)')` con listener en `useEffect` dentro de `AppPreferencesContext`.

**Rationale**: Es la API estándar del navegador para detectar el tema del SO. React no tiene primitiva propia; el patrón de `matchMedia` + listener es el estándar recomendado sin librerías adicionales.

**Implementación concreta**:
```ts
const mq = window.matchMedia('(prefers-color-scheme: dark)');
const [themeMode, setThemeMode] = useState<'light' | 'dark'>(mq.matches ? 'dark' : 'light');

useEffect(() => {
  const handler = (e: MediaQueryListEvent) => setThemeMode(e.matches ? 'dark' : 'light');
  mq.addEventListener('change', handler);
  return () => mq.removeEventListener('change', handler);
}, []);
```

**Alternativas descartadas**:
- `useMediaQuery` de MUI: introduce acoplamiento de MUI en el contexto base; además requiere acceso al tema antes de que el tema esté definido (circular).
- `localStorage`: explícitamente excluido por clarificación del spec (el tema NO se persiste manualmente).

---

## 2. React Router v7 — layout anidado con `Outlet`

**Decision**: Usar `createBrowserRouter` con layout anidado: `AppLayout` como ruta padre con `<Outlet />`, `HomePage` como ruta hija índice.

**Rationale**: React Router v7 (instalado en el proyecto como `react-router-dom ^7.6.0`) recomienda `createBrowserRouter` + `RouterProvider` sobre `<BrowserRouter>` para mejor integración con loaders/actions futuros y soporte SSR. Sin embargo, dado que el proyecto usa únicamente navegación client-side SPA sin loaders, ambas APIs son equivalentes; se elige `<BrowserRouter>` + `<Routes>` por simplicidad y para no introducir refactors futuros innecesarios.

**Implementación concreta**:
```tsx
<BrowserRouter>
  <Routes>
    <Route path="/" element={<AppLayout />}>
      <Route index element={<HomePage />} />
    </Route>
  </Routes>
</BrowserRouter>
```

**Alternativas descartadas**:
- `createBrowserRouter` + `RouterProvider`: más potente pero introduce complejidad innecesaria sin loaders/actions en esta feature.

---

## 3. ThemeProvider dinámico desde `AppPreferencesContext`

**Decision**: `App.tsx` consume `themeMode` del contexto y pasa el tema generado a `ThemeProvider`. El tema se recalcula con `useMemo` cuando cambia `themeMode`.

**Rationale**: `getAppTheme(mode)` ya existe en `src/frontend/src/theme/theme.ts` y acepta `PaletteMode`. Solo hay que conectar el valor del contexto al `useMemo`.

**Implementación concreta**:
```tsx
const { themeMode } = useAppPreferences();
const theme = useMemo(() => getAppTheme(themeMode), [themeMode]);
```

**Constraint**: `AppPreferencesContext.Provider` debe envolver `ThemeProvider` (o estar por encima), no al revés, para que `App.tsx` pueda leer `themeMode` antes de crear el tema.

---

## 4. i18next — sincronía de inicialización

**Decision**: No se requiere estado de carga para i18n.

**Rationale**: `i18n/i18n.ts` usa recursos en memoria (`resources` importado de `resources.ts`). La inicialización con `initReactI18next` + recursos en memoria es **síncrona**: `i18n.isInitialized` es `true` antes del primer render. No hay red ni filesystem involucrado.

**Conclusión**: No hace falta `<Suspense>` ni estado de "cargando traducciones". Los componentes pueden usar `useTranslation()` directamente.

---

## 5. Eliminación de `shared/components/` tras migración

**Decision**: Eliminar los tres archivos (`AppHeader.tsx`, `AppFooter.tsx`, `AppLayout.tsx`) y el archivo `WorkoutImmersiveLayout.tsx` si no tiene dependientes fuera de `shared/components/`. Verificar dependencias antes de borrar.

**Rationale**: Clarificación del spec (Q1) establece que esta feature elimina los archivos previos. SC-006 requiere que no haya duplicados.

**Proceso**:
1. Implementar nuevos componentes en `components/`.
2. Buscar todos los imports de `shared/components/AppHeader`, `AppFooter`, `AppLayout` en el proyecto.
3. Redirigir imports a las nuevas rutas.
4. Eliminar los archivos de `shared/components/`.

**Nota sobre `WorkoutImmersiveLayout.tsx`**: es un componente adicional en `shared/components/`. Su migración o eliminación depende de si tiene dependientes activos. Se verifica en tareas; si no tiene dependientes, se deja intacto (fuera del scope de esta feature).

---

## 6. Estructura de archivos resultante (confirmada vs constitución)

| Artefacto | Ruta constitucional | Estado |
|---|---|---|
| `AppHeader` | `components/AppHeader/AppHeader.tsx` | A crear |
| `AppFooter` | `components/AppFooter/AppFooter.tsx` | A crear |
| `AppLayout` | `components/AppLayout/AppLayout.tsx` | A crear |
| `HomePage` | `pages/home/HomePage.tsx` | A crear |
| `AppPreferencesContext` | `context/AppPreferencesContext.tsx` | A crear |
| `App.tsx` | `src/frontend/src/App.tsx` | A actualizar |

Ningún NEEDS CLARIFICATION pendiente. Todos los puntos resueltos.
