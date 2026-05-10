import Alert from '@mui/material/Alert';

import type { WorkoutProgress } from '../../../shared/types/workouts';
import { getTrendLabel } from '../utils/progressDisplay';

type TrendAlertProps = {
  trend: WorkoutProgress['trend'];
};

export function TrendAlert({ trend }: TrendAlertProps) {
  if (trend === 'Stable' || trend === 'NoReference') {
    return null;
  }

  return (
    <Alert severity={trend === 'Declining' ? 'warning' : 'info'} sx={{ mb: 2 }}>
      {getTrendLabel(trend)}
    </Alert>
  );
}
