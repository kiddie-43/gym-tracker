import { useState } from 'react';

import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import type { CreateMealLogRequest } from '../../../shared/types/meals';

type MealLogFormProps = {
  onSubmit: (request: CreateMealLogRequest) => Promise<void> | void;
};

export function MealLogForm({ onSubmit }: MealLogFormProps) {
  const [loggedDate, setLoggedDate] = useState('2026-05-05');

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    await onSubmit({
      loggedDate,
      slotType: 'breakfast',
      items: [{ externalFoodId: 'oats', quantity: 80, unit: 'g', calories: 300 }],
    });
  }

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit}>
      <TextField label="Fecha" value={loggedDate} onChange={(event) => setLoggedDate(event.target.value)} />
      <Button type="submit" variant="contained">Guardar registro de comida</Button>
    </Stack>
  );
}
