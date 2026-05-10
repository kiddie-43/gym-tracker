import { useMemo } from 'react';

import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider } from '@mui/material/styles';
import { RouterProvider } from 'react-router-dom';

import { AppPreferencesProvider, useAppPreferences } from './context/AppPreferencesContext';
import { router } from './router';
import { getAppTheme } from '../theme/theme';

function AppShell() {
  const { themeMode } = useAppPreferences();
  const theme = useMemo(() => getAppTheme(themeMode), [themeMode]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <RouterProvider router={router} />
    </ThemeProvider>
  );
}

export function App() {
  return (
    <AppPreferencesProvider>
      <AppShell />
    </AppPreferencesProvider>
  );
}
