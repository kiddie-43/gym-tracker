import type { PaletteMode } from '@mui/material';
import { createTheme } from '@mui/material/styles';

export function getAppTheme(mode: PaletteMode) {
  return createTheme({
    palette: {
      mode,
      primary: {
        main: '#1f4f46',
      },
      secondary: {
        main: '#d97706',
      },
      background: {
        default: mode === 'light' ? '#f4efe7' : '#111a18',
        paper: mode === 'light' ? '#fffdf8' : '#1b2a26',
      },
    },
    typography: {
      fontFamily: '"Segoe UI", sans-serif',
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          html: {
            height: '100%',
            overflow: 'hidden',
          },
          body: {
            height: '100%',
            overflow: 'hidden',
          },
          '#root': {
            height: '100%',
            overflow: 'hidden',
          },
        },
      },
    },
  });
}
