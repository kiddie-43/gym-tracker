import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { fetchUnits, setUnitsForm, setUnitsPopUpCode, submitUnitForm } from '../../../../redux/actions/units/unitsActions';
import { selectUnitState } from '../../../../redux/states/units/unitsState';

export function MeasurementTypeFormDialog() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { form, loading, error, popUpCode } = useAppSelector(selectUnitState);
  const open = popUpCode === PopUpCode.Create || popUpCode === PopUpCode.Update;

  const handleClose = () => {
    if (loading) return;
    dispatch(setUnitsPopUpCode(PopUpCode.Default));
  };

  const handleSubmit = async () => {
    const result = await dispatch(submitUnitForm());
    if (submitUnitForm.fulfilled.match(result)) {
      dispatch(setUnitsPopUpCode(PopUpCode.Default));
      void dispatch(fetchUnits());
    }
  };

  const disableSave =
    loading ||
    (form.code ?? '').trim().length === 0 ||
    (form.name ?? '').trim().length === 0;

  return (
    <PopupDialog
      open={open}
      title={form.id ? t('administration.measurementTypes.editTitle') : t('administration.measurementTypes.createTitle')}
      onClose={handleClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      disableSave={disableSave}
      isSaving={loading}
      onSubmit={() => { void handleSubmit(); }}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        {
          !form.id ? (
            <TextField
              label={t('common.fields.code')}
              value={form.code}
              onChange={(event) => dispatch(setUnitsForm({ ...form, code: event.target.value }))}
              disabled={Boolean(form.id)}
              helperText={form.id ? t('common.messages.codeReadOnly') : undefined}
              fullWidth
            />
          ) : null
        }
        <TextField
          label={t('common.fields.name')}
          value={form.name ?? ''}
          onChange={(event) => dispatch(setUnitsForm({ ...form, name: event.target.value }))}
          fullWidth
        />
        <TextField
          label={t('common.fields.description')}
          value={form.description ?? ''}
          onChange={(event) => dispatch(setUnitsForm({ ...form, description: event.target.value }))}
          multiline
          minRows={2}
          fullWidth
        />
        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
