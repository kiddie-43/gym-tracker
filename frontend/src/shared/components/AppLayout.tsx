import Container from '@mui/material/Container';
import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

import { AppHeader } from './AppHeader';
import { AppFooter } from './AppFooter';

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
      <Container
        maxWidth="lg"
        sx={{
          pt: { xs: 3, md: 4 },
          pb: { xs: 3, md: 4 },
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
