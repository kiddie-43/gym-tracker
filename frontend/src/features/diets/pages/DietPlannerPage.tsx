import { useState } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import { PageHeader } from '../../../shared/components/PageHeader';
import { createDiet } from '../api/dietsApi';
import { DietDayEditor } from '../components/DietDayEditor';

export function DietPlannerPage() {
  const [name, setName] = useState('Semana definida');
  const [dayKey, setDayKey] = useState('lunes');
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  async function handleCreate() {
    await createDiet({
      name,
      days: [
        {
          dayKey,
          mealSlots: [
            {
              slotType: 'breakfast',
              items: [{ externalFoodId: 'oats', quantity: 80, unit: 'g', calories: 300 }],
            },
          ],
        },
      ],
    });

    setStatusMessage('Dieta guardada.');
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="Planificador de dieta" description="Planifica dias y comidas con detalles opcionales de calorias." />
      {statusMessage ? <Alert severity="success">{statusMessage}</Alert> : null}
      <TextField label="Nombre de la dieta" value={name} onChange={(event) => setName(event.target.value)} />
      <DietDayEditor dayKey={dayKey} onDayKeyChange={setDayKey} />
      <Button variant="contained" onClick={() => void handleCreate()}>Guardar dieta</Button>
    </Stack>
  );
}
