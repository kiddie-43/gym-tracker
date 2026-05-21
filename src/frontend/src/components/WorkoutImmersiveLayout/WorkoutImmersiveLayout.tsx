import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

export function WorkoutImmersiveLayout() {
  return (
    <Box
      sx={{
        height: '100dvh',
        overflow: 'hidden',
        bgcolor: 'background.default',
      }}
    >
      <Outlet />
    </Box>
  );
}
