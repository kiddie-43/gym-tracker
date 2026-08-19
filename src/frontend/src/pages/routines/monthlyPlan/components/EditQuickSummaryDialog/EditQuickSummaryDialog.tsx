import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import { PopupDialog } from '../../../../../components/PopupDialog/PopupDialog';
import type { ExerciseTrainingProfile } from '../../../../../utils/exerciseProfile';

export interface QuickSummaryDraft {
  durationMinutes: number;
  secondaryMetricValue: number;
  tertiaryMetricValue: number;
}

export interface EditQuickSummaryDialogProps {
  open: boolean;
  profile: ExerciseTrainingProfile;
  value: QuickSummaryDraft;
  onClose: () => void;
  onSave: (value: QuickSummaryDraft) => void;
}

export function EditQuickSummaryDialog({ open, profile, value, onClose, onSave }: EditQuickSummaryDialogProps) {
  const { t } = useTranslation();
  const [draft, setDraft] = useState<QuickSummaryDraft>(value);

  useEffect(() => {
    if (open) {
      setDraft(value);
    }
  }, [open, value]);

  const secondaryLabel = profile === 'cardio'
    ? t('monthlyPlan.quickSummary.calories') || 'Calorías'
    : t('monthlyPlan.quickSummary.volume') || 'Volumen total';

  const tertiaryLabel = profile === 'cardio'
    ? t('monthlyPlan.quickSummary.targetPace') || 'Ritmo objetivo (RPE)'
    : t('monthlyPlan.quickSummary.exercises') || 'Ejercicios';

  const onSubmit = () => {
    onSave(draft);
  };

  return (
    <PopupDialog
      open={open}
      title={t('monthlyPlan.quickSummary.editTarget') || 'Editar objetivo'}
      onClose={onClose}
      onSubmit={onSubmit}
      closeLabel={t('common.actions.cancel') || 'Cancelar'}
      saveLabel={t('common.actions.save') || 'Guardar cambios'}
    >
      <Stack spacing={2}>
        <TextField
          label={`${t('monthlyPlan.quickSummary.duration') || 'Duración'} (${t('monthlyPlan.quickSummary.durationUnit') || 'min'})`}
          type="number"
          value={draft.durationMinutes}
          onChange={(event) => setDraft((prev) => ({ ...prev, durationMinutes: Number(event.target.value) || 0 }))}
          slotProps={{ htmlInput: { min: 0 } }}
          fullWidth
        />
        <TextField
          label={secondaryLabel}
          type="number"
          value={draft.secondaryMetricValue}
          onChange={(event) => setDraft((prev) => ({ ...prev, secondaryMetricValue: Number(event.target.value) || 0 }))}
          slotProps={{ htmlInput: { min: 0 } }}
          fullWidth
        />
        {profile === 'strength' ? (
          <TextField
            label={tertiaryLabel}
            type="number"
            value={draft.tertiaryMetricValue}
            onChange={(event) => setDraft((prev) => ({ ...prev, tertiaryMetricValue: Number(event.target.value) || 0 }))}
            slotProps={{ htmlInput: { min: 0 } }}
            fullWidth
          />
        ) : (
          <TextField
            select
            label={tertiaryLabel}
            value={draft.tertiaryMetricValue}
            onChange={(event) => setDraft((prev) => ({ ...prev, tertiaryMetricValue: Number(event.target.value) }))}
            fullWidth
            slotProps={{ select: { native: true } }}
          >
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((level) => (
              <option key={level} value={level}>
                {level} — {t(`monthlyPlan.rpeLevels.${level}`)}
              </option>
            ))}
          </TextField>
        )}
      </Stack>
    </PopupDialog>
  );
}
