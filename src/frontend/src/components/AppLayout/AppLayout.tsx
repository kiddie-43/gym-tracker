import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

import { AppHeader } from '../AppHeader/AppHeader';

export function AppLayout() {
  return (
    <Box
      sx={{
        minHeight: '100dvh',
        bgcolor: 'background.default',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <AppHeader />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
}
