import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import type { SessionExercise } from '../../../../interfaces/routines/routines';
import { PlannedSetsEditor } from '../PlannedSetsEditor/PlannedSetsEditor';

interface SessionExerciseCardProps {
  exercise: SessionExercise;
  onUnlink: (exerciseId: string) => void;
  onUpdateSet: (exerciseId: string, setId: string, repetitions: number, weightKg: number) => void;
  onDeleteSet: (exerciseId: string, setId: string) => void;
}

export function SessionExerciseCard({ exercise, onUnlink, onUpdateSet, onDeleteSet }: SessionExerciseCardProps) {
  return (
    <Paper variant="outlined" sx={{ p: 1.5 }}>
      <Stack spacing={1}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography fontWeight={600}>{exercise.name}</Typography>
          <Button size="small" color="error" onClick={() => onUnlink(exercise.id)}>
            Quitar ejercicio
          </Button>
        </Stack>
        <PlannedSetsEditor
          sets={exercise.plannedSets}
          onUpdateSet={(setId, repetitions, weightKg) => onUpdateSet(exercise.id, setId, repetitions, weightKg)}
          onDeleteSet={(setId) => onDeleteSet(exercise.id, setId)}
        />
      </Stack>
    </Paper>
  );
}
