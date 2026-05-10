import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import type { MetricDelta, WorkoutProgress } from '../../../shared/types/workouts';
import { AsyncState } from '../../../shared/components/AsyncState';
import { TrendAlert } from './TrendAlert';
import { formatPercentage, getTrendLabel } from '../utils/progressDisplay';

type ProgressSummaryPanelProps = {
  progress: WorkoutProgress | null;
  isLoading?: boolean;
  error?: string | null;
};

function ComparisonBlock({ label, delta }: { label: string; delta: MetricDelta }) {
  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h6">{label}</Typography>
        <Typography>Volumen: {formatPercentage(delta.volume.percentageChange)}</Typography>
        <Typography>Carga: {formatPercentage(delta.load.percentageChange)}</Typography>
        <Typography>Repeticiones: {formatPercentage(delta.repetitions.percentageChange)}</Typography>
      </CardContent>
    </Card>
  );
}

export function ProgressSummaryPanel({ progress, isLoading = false, error = null }: ProgressSummaryPanelProps) {
  return (
    <AsyncState isLoading={isLoading} error={error} isEmpty={!progress} emptyMessage="Selecciona un ejercicio para ver el progreso.">
      {progress ? (
        <Stack spacing={2}>
          <Typography variant="h5">{progress.exerciseId}</Typography>
          <Typography>{getTrendLabel(progress.trend)}</Typography>
          <TrendAlert trend={progress.trend} />
          <Card variant="outlined">
            <CardContent>
              <Typography variant="h6">Sesion actual</Typography>
              <Typography>Volumen: {progress.current.volume}</Typography>
              <Typography>Carga: {progress.current.load}</Typography>
              <Typography>Repeticiones: {progress.current.repetitions}</Typography>
            </CardContent>
          </Card>
          {progress.lastSessionComparison ? <ComparisonBlock label="Ultima sesion" delta={progress.lastSessionComparison} /> : null}
          {progress.rollingAverageComparison ? <ComparisonBlock label="Promedio movil" delta={progress.rollingAverageComparison} /> : null}
          {progress.bestRecentComparison ? <ComparisonBlock label="Mejor reciente" delta={progress.bestRecentComparison} /> : null}
        </Stack>
      ) : null}
    </AsyncState>
  );
}
