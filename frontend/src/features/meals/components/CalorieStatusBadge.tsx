import Chip from '@mui/material/Chip';

import { resolveCalorieStatus } from '../utils/calorieStatus';

type CalorieStatusBadgeProps = {
  calorieTrackingEnabled: boolean;
  totalCalories?: number | null;
};

export function CalorieStatusBadge({ calorieTrackingEnabled, totalCalories }: CalorieStatusBadgeProps) {
  const status = resolveCalorieStatus(calorieTrackingEnabled, totalCalories);

  if (status === 'disabled') {
    return <Chip label="Calorias desactivadas" size="small" />;
  }

  if (status === 'unknown') {
    return <Chip label="Calorias desconocidas" color="warning" size="small" />;
  }

  return <Chip label={`Calorias ${totalCalories}`} color="success" size="small" />;
}
