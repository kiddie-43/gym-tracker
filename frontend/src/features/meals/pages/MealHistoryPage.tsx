import { useEffect, useState } from 'react';

import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import { PageHeader } from '../../../shared/components/PageHeader';
import { getMealHistory } from '../api/mealsApi';

type MealHistoryItem = {
  id: string;
  loggedDate: string;
  slotType: string;
  itemCount: number;
};

function getSlotTypeLabel(slotType: string) {
  switch (slotType) {
    case 'breakfast':
      return 'desayuno';
    case 'lunch':
      return 'almuerzo';
    case 'dinner':
      return 'cena';
    case 'snack':
      return 'snack';
    default:
      return slotType;
  }
}

export function MealHistoryPage() {
  const [items, setItems] = useState<MealHistoryItem[]>([]);

  useEffect(() => {
    void getMealHistory().then((response) => setItems(response.items)).catch(() => setItems([]));
  }, []);

  return (
    <Stack spacing={2}>
      <PageHeader title="Historial de comidas" description="Explora los registros de comida en orden cronologico." />
      {items.map((item) => (
        <Typography key={item.id}>{item.loggedDate} - {getSlotTypeLabel(item.slotType)} ({item.itemCount})</Typography>
      ))}
    </Stack>
  );
}
