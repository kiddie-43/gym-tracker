# Quickstart: Página Placeholder para Módulos Pendientes

**Feature**: 005-pagina-placeholder  
**Date**: 2026-05-15

---

## ¿Qué hace esta feature?

Añade un componente `PlaceholderPage` reutilizable y registra 8 rutas de módulo en `App.tsx`. Con esto, cualquier enlace del menú lateral lleva a una página real (header + contenido + footer) en lugar de una pantalla en blanco.

---

## Archivos que se crean / modifican

```
CREAR  src/frontend/src/pages/placeholder/PlaceholderPage.tsx
EDITAR src/frontend/src/i18n/resources.ts       ← añadir placeholder.* en ES + EN
EDITAR src/frontend/src/App.tsx                 ← registrar 8 rutas + import
```

---

## 1 — Crear `PlaceholderPage`

```tsx
// src/frontend/src/pages/placeholder/PlaceholderPage.tsx
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import { useTranslation } from 'react-i18next';

interface PlaceholderPageProps {
  labelKey: string;
}

export function PlaceholderPage({ labelKey }: PlaceholderPageProps) {
  const { t } = useTranslation();

  return (
    <Container maxWidth="sm" sx={{ textAlign: 'center', py: { xs: 6, md: 10 } }}>
      <Stack spacing={3} alignItems="center">
        <Typography variant="h4" fontWeight={700}>
          {t('placeholder.title', { module: t(labelKey) })}
        </Typography>
        <Chip label={t('placeholder.status')} color="warning" />
        <Typography variant="body1" color="text.secondary">
          {t('placeholder.message')}
        </Typography>
      </Stack>
    </Container>
  );
}
```

---

## 2 — Añadir claves i18n en `resources.ts`

En la sección `es.translation` y `en.translation`, añadir el bloque `placeholder`:

```ts
// ES
placeholder: {
  title: '{{module}} está en desarrollo',
  status: 'En desarrollo',
  message: 'Este módulo estará disponible próximamente.',
},

// EN
placeholder: {
  title: '{{module}} is in development',
  status: 'In development',
  message: 'This module will be available soon.',
},
```

---

## 3 — Registrar rutas en `App.tsx`

```tsx
// Añadir import junto a los existentes:
import { PlaceholderPage } from './pages/placeholder/PlaceholderPage';

// Dentro de <Route path="/" element={<AppLayout />}>, añadir tras <Route index>:
<Route path="workouts"     element={<PlaceholderPage labelKey="nav.workout" />} />
<Route path="progress"     element={<PlaceholderPage labelKey="nav.progress" />} />
<Route path="routines"     element={<PlaceholderPage labelKey="nav.routines" />} />
<Route path="diets"        element={<PlaceholderPage labelKey="nav.diets" />} />
<Route path="meals"        element={<PlaceholderPage labelKey="nav.meals" />} />
<Route path="settings"     element={<PlaceholderPage labelKey="nav.settings" />} />
<Route path="administracion" element={<PlaceholderPage labelKey="nav.administracion" />} />
<Route path="profile"      element={<PlaceholderPage labelKey="nav.profile" />} />
```

---

## Verificación

```bash
# Desde src/frontend/
npx tsc --noEmit          # Sin errores de tipo
npm run dev               # Arrancar la app
```

Navegar a `/workouts`, `/progress`, `/routines`, etc. — cada ruta debe mostrar header + contenido ("Entrenamiento está en desarrollo") + footer.

---

## Implementar un módulo real en el futuro

Cuando llegue la feature de un módulo concreto (p. ej. Entrenamientos):

1. Crear `src/frontend/src/pages/workouts/WorkoutsPage.tsx` con la implementación real.
2. En `App.tsx`, reemplazar `<PlaceholderPage labelKey="nav.workout" />` por `<WorkoutsPage />`.
3. `PlaceholderPage` no se modifica — sigue disponible para otros módulos pendientes.
