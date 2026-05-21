import { useEffect, useState } from 'react';

import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import type { ProgressComparison } from '../../../../interfaces/routines/routines';
import { getExerciseProgressComparison } from '../../../../services/api/routines/trainingFlowApi';

interface ExerciseProgressComparisonPanelProps {
  exerciseId: string;
}

const trendLabel: Record<ProgressComparison['trend'], string> = {
  improves: 'Mejora',
  stable: 'Estable',
  declines: 'Empeora',
};

export function ExerciseProgressComparisonPanel({ exerciseId }: ExerciseProgressComparisonPanelProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [comparison, setComparison] = useState<ProgressComparison | null>(null);

  useEffect(() => {
    let mounted = true;

    const run = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await getExerciseProgressComparison(exerciseId);
        if (mounted) {
          setComparison(response);
        }
      } catch {
        if (mounted) {
          setError('No hay datos suficientes para comparar progreso.');
          setComparison(null);
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    void run();

    return () => {
      mounted = false;
    };
  }, [exerciseId]);

  if (loading) {
    return <CircularProgress size={20} />;
  }

  if (error || !comparison) {
    return <Typography variant="body2" color="text.secondary">{error}</Typography>;
  }

  return (
    <Box sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 2 }}>
      <Stack spacing={1}>
        <Typography variant="subtitle1" fontWeight={700}>Comparativa de progreso</Typography>
        <Typography variant="body2">Tendencia: {trendLabel[comparison.trend]}</Typography>
        <Typography variant="body2">Variacion: {comparison.variationPercent}%</Typography>
        <Typography variant="body2">Volumen actual: {comparison.current.volumeTotal}</Typography>
        <Typography variant="body2">Carga actual: {comparison.current.loadTotal} kg</Typography>
        <Typography variant="body2">Repeticiones actuales: {comparison.current.repetitionsTotal}</Typography>
      </Stack>
    </Box>
  );
}
