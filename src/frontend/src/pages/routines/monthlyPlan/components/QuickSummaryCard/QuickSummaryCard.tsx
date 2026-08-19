import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import TimerOutlinedIcon from '@mui/icons-material/TimerOutlined';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import FormatListNumberedOutlinedIcon from '@mui/icons-material/FormatListNumberedOutlined';

import type { ExerciseTrainingProfile } from '../../../../../utils/exerciseProfile';

export interface QuickSummaryCardProps {
  profile: ExerciseTrainingProfile;
  durationMinutes: number;
  secondaryMetricValue: number;
  tertiaryMetricValue: number;
  onEditTarget?: () => void;
}

interface MetricItemProps {
  icon: React.ReactNode;
  value: string;
  label: string;
}

function MetricItem({ icon, value, label }: MetricItemProps) {
  return (
    <Stack alignItems="center" spacing={0.5} sx={{ flex: 1, minWidth: 0 }}>
      <Box sx={{ color: 'primary.main' }}>{icon}</Box>
      <Typography variant="h6" sx={{ fontWeight: 700 }}>
        {value}
      </Typography>
      <Typography variant="caption" color="text.secondary" sx={{ textAlign: 'center' }}>
        {label}
      </Typography>
    </Stack>
  );
}

export function QuickSummaryCard({
  profile,
  durationMinutes,
  secondaryMetricValue,
  tertiaryMetricValue,
  onEditTarget,
}: QuickSummaryCardProps) {
  const { t } = useTranslation();

  const secondaryLabel = profile === 'cardio'
    ? t('monthlyPlan.quickSummary.calories') || 'Calorías'
    : t('monthlyPlan.quickSummary.volume') || 'Volumen total';

  const tertiaryLabel = profile === 'cardio'
    ? t('monthlyPlan.quickSummary.targetPace') || 'Ritmo objetivo'
    : t('monthlyPlan.quickSummary.exercises') || 'Ejercicios';

  const isValidRpe = tertiaryMetricValue >= 1 && tertiaryMetricValue <= 10;
  const tertiaryValue = profile === 'cardio'
    ? (isValidRpe ? t(`monthlyPlan.rpeLevels.${tertiaryMetricValue}`) : '—')
    : String(tertiaryMetricValue);

  return (
    <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.5 }}>
        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
          {t('monthlyPlan.quickSummary.title') || 'Resumen rápido'}
        </Typography>
        {onEditTarget ? (
          <Button size="small" onClick={onEditTarget}>
            {t('monthlyPlan.quickSummary.editTarget') || 'Editar objetivo'}
          </Button>
        ) : null}
      </Stack>
      <Stack direction="row" spacing={1}>
        <MetricItem
          icon={<TimerOutlinedIcon />}
          value={`${durationMinutes} ${t('monthlyPlan.quickSummary.durationUnit') || 'min'}`}
          label={t('monthlyPlan.quickSummary.duration') || 'Duración'}
        />
        <MetricItem
          icon={<FitnessCenterOutlinedIcon />}
          value={String(secondaryMetricValue)}
          label={secondaryLabel}
        />
        <MetricItem
          icon={<FormatListNumberedOutlinedIcon />}
          value={tertiaryValue}
          label={tertiaryLabel}
        />
      </Stack>
    </Paper>
  );
}
