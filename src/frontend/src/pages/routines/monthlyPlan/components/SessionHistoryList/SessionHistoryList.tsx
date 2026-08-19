import { useTranslation } from 'react-i18next';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import type { ITrainingSession } from '../../../../../interfaces/trainingSessions/trainingSessions';

export interface SessionHistoryListProps {
  sessions: ITrainingSession[];
}

export function SessionHistoryList({ sessions }: SessionHistoryListProps) {
  const { t } = useTranslation();

  if (sessions.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
        {t('monthlyPlan.historyEmpty') || 'Todavía no hay sesiones registradas.'}
      </Typography>
    );
  }

  return (
    <Stack spacing={1.5}>
      {sessions.map((session) => (
        <Stack
          key={session.id}
          spacing={0.5}
          sx={{ p: 1.5, border: 1, borderColor: 'divider', borderRadius: 2 }}
        >
          <Typography variant="subtitle2">
            {new Date(session.timestamp).toLocaleString()}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {session.durationMinutes} {t('monthlyPlan.quickSummary.durationUnit') || 'min'} ·{' '}
            {session.secondaryMetricValue} {session.secondaryMetricUnitCode} ·{' '}
            {session.tertiaryMetricValue}
          </Typography>
          {session.blocks.length > 0 ? (
            <Typography variant="caption" color="text.secondary">
              {session.blocks.map((block) => block.name).join(' · ')}
            </Typography>
          ) : null}
        </Stack>
      ))}
    </Stack>
  );
}
