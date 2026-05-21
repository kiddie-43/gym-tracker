import type { PaletteMode } from '@mui/material';
import { createTheme } from '@mui/material/styles';

export function getAppTheme(mode: PaletteMode) {
  const isLight = mode === 'light';

  return createTheme({
    palette: {
      mode,

      /* Energía / fuerza */
      primary: {
        main: isLight ? '#E76F51' : '#F08A6B', // rojo coral cálido
        light: '#FFD6CC',
        dark: '#C8553D',
        contrastText: '#FFFFFF',
      },

      /* Motivación / energía */
      secondary: {
        main: isLight ? '#F4B942' : '#FFD166', // amarillo gym
        light: '#FFE9B3',
        dark: '#D99A1E',
        contrastText: '#3A2A00',
      },

      background: {
        default: isLight ? '#FFF8F1' : '#1A1410',
        paper: isLight ? '#FFFFFF' : '#241C17',
      },

      success: {
        main: '#7BC47F',
      },

      info: {
        main: '#7FB3D5',
      },

      warning: {
        main: '#F6C453',
      },

      error: {
        main: '#E57373',
      },

      text: {
        primary: isLight ? '#332822' : '#F5EEE8',
        secondary: isLight ? '#6E5D52' : '#CDBFB5',
      },

      divider: isLight ? '#F1DDD0' : '#3A2E27',
    },

    typography: {
      fontFamily: '"Segoe UI", sans-serif',
    },

    shape: {
      borderRadius: 14,
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
            backgroundColor: isLight ? '#FFF8F1' : '#1A1410',

            '& *::-webkit-scrollbar': {
              width: 6,
              height: 6,
            },

            '& *::-webkit-scrollbar-track': {
              background: 'transparent',
            },

            '& *::-webkit-scrollbar-thumb': {
              backgroundColor: isLight
                ? 'rgba(231,111,81,0.25)'
                : 'rgba(255,209,102,0.25)',

              borderRadius: 999,

              border: `2px solid ${
                isLight ? '#FFF8F1' : '#1A1410'
              }`,

              backgroundClip: 'content-box',
            },

            '& *::-webkit-scrollbar-thumb:hover': {
              backgroundColor: isLight
                ? 'rgba(231,111,81,0.45)'
                : 'rgba(255,209,102,0.45)',
            },

            '& *::-webkit-scrollbar-corner': {
              background: 'transparent',
            },

            '& *': {
              scrollbarWidth: 'thin',

              scrollbarColor: isLight
                ? 'rgba(231,111,81,0.25) transparent'
                : 'rgba(255,209,102,0.25) transparent',
            },
          },

          '#root': {
            height: '100%',
            overflow: 'hidden',
          },
        },
      },

      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            border: isLight
              ? '1px solid rgba(244,185,66,0.18)'
              : '1px solid rgba(255,255,255,0.06)',
          },
        },
      },

      MuiButton: {
        styleOverrides: {
          root: {
            textTransform: 'none',
            fontWeight: 600,
          },

          containedPrimary: {
            boxShadow: '0 6px 18px rgba(231,111,81,0.22)',
          },

          containedSecondary: {
            boxShadow: '0 6px 18px rgba(244,185,66,0.22)',
          },
        },
      },
    },
  });
}