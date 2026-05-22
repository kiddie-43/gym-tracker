import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import type { MeasurementTypeFormState } from './measurementTypeForm';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';

type MeasurementTypeFormDialogProps = {
  open: boolean;
  loading: boolean;
  error: string | null;
  formState: MeasurementTypeFormState;
  onClose: () => void;
  onSubmit: () => void;
  onChange: (patch: Partial<MeasurementTypeFormState>) => void;
};

export function MeasurementTypeFormDialog({
  open,
  loading,
  error,
  formState,
  onClose,
  onSubmit,
  onChange,
}: MeasurementTypeFormDialogProps) {
  const { t } = useTranslation();

  return (
    <PopupDialog
      open={open}
      title={formState.id ? t('administration.measurementTypes.editTitle') : t('administration.measurementTypes.createTitle')}
      onClose={onClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      isSaving={loading}
      onSubmit={onSubmit}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <TextField
          label={t('common.fields.code')}
          value={formState.code ?? ''}
          onChange={(event) => onChange({ code: event.target.value })}
          fullWidth
        />
        <TextField
          label={t('administration.measurementTypes.fields.name')}
          value={formState.name ?? ''}
          onChange={(event) => onChange({ name: event.target.value })}
          fullWidth
        />
        <TextField
          label={t('administration.measurementTypes.fields.description')}
          value={formState.description ?? ''}
          onChange={(event) => onChange({ description: event.target.value })}
          multiline
          minRows={2}
          fullWidth
        />
        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
