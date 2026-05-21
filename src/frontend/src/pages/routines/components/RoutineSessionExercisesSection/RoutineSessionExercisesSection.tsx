import { useMemo, useState } from 'react';

import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import type { RoutineSession } from '../../../../interfaces/routines/routines';
import { SessionExerciseCard } from '../SessionExerciseCard/SessionExerciseCard';

interface RoutineSessionExercisesSectionProps {
  routineId: string;
  session: RoutineSession;
  onAddExercise: (routineId: string, sessionId: string, exerciseId: string, name: string) => void;
  onUnlinkExercise: (routineId: string, sessionId: string, exerciseId: string) => void;
  onUpdateSet: (routineId: string, sessionId: string, exerciseId: string, setId: string, repetitions: number, weightKg: number) => void;
  onDeleteSet: (routineId: string, sessionId: string, exerciseId: string, setId: string) => void;
}

const catalogExercises = [
  { id: 'exercise-1', name: 'Press banca' },
  { id: 'exercise-2', name: 'Sentadilla' },
  { id: 'exercise-3', name: 'Peso muerto' },
];

export function RoutineSessionExercisesSection({
  routineId,
  session,
  onAddExercise,
  onUnlinkExercise,
  onUpdateSet,
  onDeleteSet,
}: RoutineSessionExercisesSectionProps) {
  const [selectedExerciseId, setSelectedExerciseId] = useState(catalogExercises[0].id);

  const selectedExerciseName = useMemo(
    () => catalogExercises.find((item) => item.id === selectedExerciseId)?.name ?? 'Ejercicio',
    [selectedExerciseId],
  );

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="h6">Ejercicios de {session.name}</Typography>
        <Stack direction="row" spacing={1}>
          <TextField
            select
            label="Catalogo"
            value={selectedExerciseId}
            onChange={(event) => setSelectedExerciseId(event.target.value)}
            sx={{ minWidth: 220 }}
          >
            {catalogExercises.map((exercise) => (
              <MenuItem key={exercise.id} value={exercise.id}>{exercise.name}</MenuItem>
            ))}
          </TextField>
          <Button
            variant="outlined"
            onClick={() => onAddExercise(routineId, session.id, selectedExerciseId, selectedExerciseName)}
          >
            Agregar ejercicio
          </Button>
        </Stack>

        <Stack spacing={1}>
          {session.exercises.map((exercise) => (
            <SessionExerciseCard
              key={exercise.id}
              exercise={exercise}
              onUnlink={(exerciseId) => onUnlinkExercise(routineId, session.id, exerciseId)}
              onUpdateSet={(exerciseId, setId, repetitions, weightKg) => onUpdateSet(routineId, session.id, exerciseId, setId, repetitions, weightKg)}
              onDeleteSet={(exerciseId, setId) => onDeleteSet(routineId, session.id, exerciseId, setId)}
            />
          ))}
        </Stack>
      </Stack>
    </Paper>
  );
}
