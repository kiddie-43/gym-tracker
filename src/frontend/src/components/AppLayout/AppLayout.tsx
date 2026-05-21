import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

import { AppFooter } from '../AppFooter/AppFooter';
import { AppHeader } from '../AppHeader/AppHeader';

export function AppLayout() {
  return (
    <Box
      sx={{
        height: '100dvh',
        bgcolor: 'background.default',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      <AppHeader />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          minHeight: 0,
          overflowY: 'auto',
          overflowX: 'hidden',
        }}
      >
        <Outlet />
      </Box>
      <AppFooter />
    </Box>
  );
}
