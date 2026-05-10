import { useState } from 'react';

import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import type { CreateRoutineRequest } from '../../../shared/types/routines';
import type { MuscleGroup } from '../../../shared/types/catalog';
import { MuscleGroupSelector } from './MuscleGroupSelector';

type RoutineFormProps = {
  muscleGroups: MuscleGroup[];
  onSubmit: (request: CreateRoutineRequest) => Promise<void> | void;
};

export function RoutineForm({ muscleGroups, onSubmit }: RoutineFormProps) {
  const [name, setName] = useState('Division empuje');
  const [description, setDescription] = useState('Dias de tren superior');
  const [dayLabel, setDayLabel] = useState('Lunes');
  const [muscleGroupIds, setMuscleGroupIds] = useState<string[]>(['chest']);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    await onSubmit({
      name,
      description,
      tags: ['template'],
      days: [
        {
          dayLabel,
          exercises: [
            {
              externalExerciseId: 'bench-press',
              exerciseName: 'Press de banca',
              muscleGroupIds,
              targetSets: 4,
              targetRepetitions: 8,
              targetRestSeconds: 120,
            },
          ],
        },
      ],
    });
  }

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit}>
      <TextField label="Nombre de la rutina" value={name} onChange={(event) => setName(event.target.value)} />
      <TextField label="Descripcion" value={description} onChange={(event) => setDescription(event.target.value)} />
      <TextField label="Nombre del dia" value={dayLabel} onChange={(event) => setDayLabel(event.target.value)} />
      <MuscleGroupSelector value={muscleGroupIds} options={muscleGroups} onChange={setMuscleGroupIds} />
      <Button type="submit" variant="contained">Guardar rutina</Button>
    </Stack>
  );
}
