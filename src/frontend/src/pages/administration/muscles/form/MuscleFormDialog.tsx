import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import type { MuscleFormState } from './muscleForm';

type MuscleFormDialogProps = {
  open: boolean;
  loading: boolean;
  error: string | null;
  formState: MuscleFormState;
  onClose: () => void;
  onSubmit: () => void;
  onChange: (patch: Partial<MuscleFormState>) => void;
};

export function MuscleFormDialog({
  open,
  loading,
  error,
  formState,
  onClose,
  onSubmit,
  onChange,
}: MuscleFormDialogProps) {
  const { t } = useTranslation();
  const disableSave =
    loading ||
    formState.name.trim().length === 0 ||
    formState.code.trim().length === 0;

  return (
    <PopupDialog
      open={open}
      title={formState.id ? t('administration.muscles.editTitle') : t('administration.muscles.createTitle')}
      onClose={onClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      disableSave={disableSave}
      isSaving={loading}
      onSubmit={onSubmit}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <TextField
          label={t('administration.common.fields.name')}
          value={formState.name}
          onChange={(event) => onChange({ name: event.target.value })}
          fullWidth
        />
        <TextField
          label={t('common.fields.code')}
          value={formState.code}
          onChange={(event) => onChange({ code: event.target.value })}
          disabled={Boolean(formState.id)}
          helperText={formState.id ? t('common.messages.codeReadOnly') : undefined}
          fullWidth
        />
        <TextField
          label={t('common.fields.description')}
          value={formState.description ?? ''}
          onChange={(event) => onChange({ description: event.target.value })}
          fullWidth
        />
        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
