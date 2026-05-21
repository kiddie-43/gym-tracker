import React from 'react';
import { Box, Card, CardActionArea, CardContent, Stack, Typography } from '@mui/material';

interface Session {
  id: string;
  name: string;
  daysOfWeek: string[];
  exercises: unknown[];
}

interface SessionListProps {
  sessions: Session[];
  onSessionSelect: (sessionId: string) => void;
}

export const SessionList: React.FC<SessionListProps> = ({ sessions, onSessionSelect }) => {

  const handleSessionSelect = (sessionId: string) => {
    onSessionSelect(sessionId);
  };

  return (
    <Stack
      direction={{ xs: 'column', md: 'row' }}
      flexWrap={{ xs: 'nowrap', md: 'wrap' }}
      spacing={2}
      sx={{ width: '100%' }}
    >
      {sessions.map((session) => (
        <Box
          key={session.id}
          sx={{
            width: {
              xs: '100%',
              md: 'calc(50% - 8px)',
              lg: 'calc(50% - 8px)'
            }
          }}
        >
          <Card variant="outlined" sx={{ width: '100%', minHeight: 180 }}>
            <CardActionArea onClick={() => handleSessionSelect(session.id)}>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="h6">{session.name}</Typography>
                <Typography variant="body2" color="text.secondary">Dias: {session.daysOfWeek.join(', ')}</Typography>
                <Typography variant="body2" color="text.secondary">Ejercicios: {session.exercises.length}</Typography>
              </CardContent>
            </CardActionArea>
          </Card>
        </Box>
      ))}
    </Stack>
  );
};