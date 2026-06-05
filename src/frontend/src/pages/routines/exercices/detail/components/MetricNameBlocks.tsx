import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import { FeedbackMessage } from '../../../../../components/FeedbackMessage/FeedbackMessage';
import type { MetricDefinition } from '../../../../../interfaces/routines/trainingMetricLogs/TrainingMetricLog';

interface MetricNameBlocksProps {
  metrics: MetricDefinition[];
  loading: boolean;
  error: string | null;
  degraded: boolean;
}

export function MetricNameBlocks({ metrics, loading, error, degraded }: MetricNameBlocksProps) {
  if (loading) {
    return (
      <Stack alignItems="center" justifyContent="center" sx={{ py: 4 }}>
        <CircularProgress size={28} aria-label="Cargando metricas" />
      </Stack>
    );
  }

  if (error) {
    return <FeedbackMessage type="error" message={error} />;
  }

  if (metrics.length === 0) {
    if (degraded) {
      return (
        <FeedbackMessage
          type="empty"
          message="Modo degradado activo: no se pudieron cargar las definiciones de metricas, pero tus logs siguen disponibles."
        />
      );
    }

    return <FeedbackMessage type="empty" message="No hay metricas disponibles para este ejercicio." />;
  }

  return (
    <Stack spacing={1.5}>
      {degraded ? (
        <Typography variant="body2" color="warning.main">
          Modo degradado activo: no se pudieron resolver todas las definiciones de metricas.
        </Typography>
      ) : null}

      <Box
        sx={{
          display: 'grid',
          gap: 1,
          gridTemplateColumns: {
            xs: '1fr',
            sm: 'repeat(2, minmax(0, 1fr))',
            md: 'repeat(3, minmax(0, 1fr))',
          },
        }}
      >
        {metrics.map((metric) => (
          <Stack
            key={metric.metricId}
            spacing={0.5}
            sx={{
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 2,
              p: 1.25,
            }}
          >
            <Typography variant="subtitle2">{metric.name}</Typography>
            <Stack direction="row" spacing={1}>
              <Chip size="small" label={metric.dataType} />
              {metric.unit ? <Chip size="small" variant="outlined" label={metric.unit} /> : null}
            </Stack>
          </Stack>
        ))}
      </Box>
    </Stack>
  );
}