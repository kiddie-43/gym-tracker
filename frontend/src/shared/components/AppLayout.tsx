import Container from '@mui/material/Container';
import Box from '@mui/material/Box';
import { Outlet, useLocation } from 'react-router-dom';

import { AppHeader } from './AppHeader';
import { AppFooter } from './AppFooter';

export function AppLayout() {
  const location = useLocation();
  const isAdministrationRoute = location.pathname.startsWith('/administration');

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
      <Container
        maxWidth={isAdministrationRoute ? false : 'lg'}
        sx={{
          pt: isAdministrationRoute ? 0 : { xs: 3, md: 4 },
          pb: isAdministrationRoute ? 0 : { xs: 3, md: 4 },
          px: isAdministrationRoute ? 0 : undefined,
          flexGrow: 1,
          minHeight: 0,
          overflowY: 'auto',
          overflowX: 'hidden',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <Outlet />
      </Container>
      <AppFooter />
    </Box>
  );
}
