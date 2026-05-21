import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import type { PlannedSet } from '../../../../interfaces/routines/routines';

interface PlannedSetsEditorProps {
  sets: PlannedSet[];
  onUpdateSet: (setId: string, repetitions: number, weightKg: number) => void;
  onDeleteSet: (setId: string) => void;
}

export function PlannedSetsEditor({ sets, onUpdateSet, onDeleteSet }: PlannedSetsEditorProps) {
  return (
    <Stack spacing={1}>
      <Typography variant="subtitle2">Series planificadas</Typography>
      {sets.map((setItem) => (
        <Stack key={setItem.id} direction="row" spacing={1} alignItems="center">
          <TextField
            type="number"
            size="small"
            label="Reps"
            defaultValue={setItem.repetitions}
            inputProps={{ min: 1 }}
            onBlur={(event) => onUpdateSet(setItem.id, Number(event.target.value), setItem.weightKg)}
          />
          <TextField
            type="number"
            size="small"
            label="Kg"
            defaultValue={setItem.weightKg}
            inputProps={{ min: 1 }}
            onBlur={(event) => onUpdateSet(setItem.id, setItem.repetitions, Number(event.target.value))}
          />
          <Button size="small" color="error" onClick={() => onDeleteSet(setItem.id)}>
            Eliminar serie
          </Button>
        </Stack>
      ))}
    </Stack>
  );
}
