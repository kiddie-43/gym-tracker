import { useEffect, useMemo } from 'react';

import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';

import { setPreferencesThemeMode } from './redux/actions/preferences/preferencesActions';
import { selectPreferencesState } from './redux/states/preferences/preferencesState';
import { AppRoutes } from './routes/routes';
import { getAppTheme } from './theme/theme';

function AppShell() {
  const dispatch = useDispatch();
  const { themeMode } = useSelector(selectPreferencesState);
  const theme = useMemo(() => getAppTheme(themeMode), [themeMode]);

  useEffect(() => {
    if (typeof window === 'undefined' || !window.matchMedia) return;

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    const handler = (e: MediaQueryListEvent) => {
      dispatch(setPreferencesThemeMode(e.matches ? 'dark' : 'light'));
    };

    mediaQuery.addEventListener('change', handler);
    return () => {
      mediaQuery.removeEventListener('change', handler);
    };
  }, [dispatch]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </ThemeProvider>
  );
}

export function App() {
  return <AppShell />;
}
