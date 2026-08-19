# Data Model: Página de Bienvenida con Layout Principal

**Feature**: 004-pagina-bienvenida
**Date**: 2026-05-15

> Esta feature es **puramente frontend**. No hay entidades persistidas en base de datos ni en Firebase. Los modelos aquí descritos son tipos TypeScript que viven en memoria de sesión.

---

## Entidades

### `UserProfile`

Datos del usuario mostrados en el header (menú de perfil). Sin persistencia en esta feature; valores por defecto vacíos hasta que haya integración con Firebase Auth.

```ts
interface UserProfile {
  firstName: string;   // Nombre. Default: ''
  lastName: string;    // Apellidos. Default: ''
  email: string;       // Email. Default: ''
  photoUrl: string;    // URL de foto de perfil. Default: ''
}
```

**Fuente en esta feature**: estado local en `AppPreferencesContext` (valores vacíos por defecto).  
**Fuente futura**: Firebase Auth + Firestore (feature de autenticación).

---

### `AppPreferences`

Agrupa el perfil y el tema. Es la forma del contexto expuesto por `AppPreferencesContext`.

```ts
interface AppPreferences {
  profile: UserProfile;
  themeMode: 'light' | 'dark';
}
```

**Ciclo de vida**: creado al montar `AppPreferencesContext.Provider`; destruido al cerrar el navegador. Sin persistencia en `localStorage`.  
**themeMode**: derivado de `window.matchMedia('(prefers-color-scheme: dark)')` y actualizado reactivamente si el usuario cambia la preferencia del SO durante la sesión.

---

### `NavigationLink`

Representa un ítem del menú lateral del header. Lista estática definida en el componente `AppHeader`.

```ts
interface NavigationLink {
  to: string;        // Ruta destino (ej. '/workouts')
  labelKey: string;  // Clave i18n (ej. 'nav.workout')
  icon: React.ComponentType<SvgIconProps>;  // Ícono MUI
}
```

**Fuente**: array constante definido en `AppHeader.tsx`. No se obtiene de API ni de Redux.

---

## Interfaces TypeScript — ubicación en el proyecto

Según la constitución, las interfaces de dominio van en `interfaces/<dominio>/`.  
Para esta feature:

| Interfaz | Archivo |
|---|---|
| `UserProfile` | `interfaces/preferences/preferences.ts` |
| `AppPreferences` | `interfaces/preferences/preferences.ts` |
| `NavigationLink` | Definida inline en `components/AppHeader/AppHeader.tsx` (tipo privado, no de dominio) |

**Decisión sobre `NavigationLink`**: es un detalle de implementación del componente, no una entidad de dominio reutilizable entre módulos. Se define como tipo local en el archivo del componente.

---

## Contratos de API

**No aplica.** Esta feature no expone ni consume endpoints REST. No se genera `openapi.yaml` para esta feature.

---

## Estado en `AppPreferencesContext`

```ts
// context/AppPreferencesContext.tsx
type AppPreferencesContextValue = {
  profile: UserProfile;
  themeMode: 'light' | 'dark';
};

// No se expone toggleThemeMode() — el tema es solo lectura desde el SO.
```

**Patrón de consumo**:
```ts
const { profile, themeMode } = useAppPreferences();
```

**Inicialización**:
```ts
const mq = window.matchMedia('(prefers-color-scheme: dark)');
// themeMode inicial = mq.matches ? 'dark' : 'light'
// Listener en useEffect actualiza themeMode al cambiar el SO
```
