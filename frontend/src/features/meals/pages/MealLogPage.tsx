import { useState } from 'react';

import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';

import { PageHeader } from '../../../shared/components/PageHeader';
import { createMealLog } from '../api/mealsApi';
import { CalorieStatusBadge } from '../components/CalorieStatusBadge';
import { MealLogForm } from '../components/MealLogForm';

export function MealLogPage() {
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [totalCalories, setTotalCalories] = useState<number | null>(null);

  return (
    <Stack spacing={2}>
      <PageHeader title="Registro de comidas" description="Registra comidas y revisa el estado de calorias." />
      {statusMessage ? <Alert severity="success">{statusMessage}</Alert> : null}
      <MealLogForm
        onSubmit={async (request) => {
          const response = await createMealLog(request);
          setTotalCalories(response.totalCalories ?? null);
          setStatusMessage('Registro de comida guardado.');
        }}
      />
      <CalorieStatusBadge calorieTrackingEnabled totalCalories={totalCalories} />
    </Stack>
  );
}
