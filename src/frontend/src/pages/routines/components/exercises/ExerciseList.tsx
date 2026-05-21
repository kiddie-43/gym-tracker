import React from 'react';
import AddIcon from '@mui/icons-material/Add';
import Fab from '@mui/material/Fab';
import { Box, Card, CardActionArea, CardContent, Stack, Typography } from '@mui/material';

interface Exercise {
  id: string;
  name: string;
  plannedSets: unknown[];
}

interface ExerciseListProps {
  exercises: Exercise[];
  onExerciseSelect: (exerciseId: string) => void;
}

export const ExerciseList: React.FC<ExerciseListProps> = ({ exercises, onExerciseSelect }) => {

  const handleExerciseSelect = (exerciseId: string) => {
    onExerciseSelect(exerciseId);
  };

  return (
    <>
      <Stack
        direction={{ xs: 'column', md: 'row' }}
        flexWrap={{ xs: 'nowrap', md: 'wrap' }}
        spacing={2}
        sx={{ width: '100%' }}
      >
        {exercises.map((exercise) => (
          <Box
            key={exercise.id}
            sx={{
              width: {
                xs: '100%',
                md: 'calc(50% - 8px)',
                lg: 'calc(50% - 8px)'
              }
            }}
          >
            <Card variant="outlined" sx={{ width: '100%', minHeight: 160 }}>
              <CardActionArea onClick={() => handleExerciseSelect(exercise.id)}>
                <CardContent sx={{ p: 3 }}>
                  <Typography variant="h6">{exercise.name}</Typography>
                  <Typography variant="body2" color="text.secondary">Series: {exercise.plannedSets.length}</Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Box>
        ))}
      </Stack>

      <Fab
        color="primary"
        aria-label="Crear ejercicio"
        sx={{ position: 'fixed', right: 32, bottom: 80, zIndex: 1200 }}
        onClick={() => {}}
      >
        <AddIcon />
      </Fab>
    </>
  );
};