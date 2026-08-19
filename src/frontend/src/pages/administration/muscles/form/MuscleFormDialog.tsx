import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import { PopUpCode } from '../../../../enums/popUp/popUp';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { fetchAdminMuscles, setMusclesForm, setMusclesPopUpCode, submitAdminMuscleForm } from '../../../../redux/actions/muscles/musclesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { selectMusclesState } from '../../../../redux/states/adminMuscles/adminMusclesState';

export function MuscleFormDialog() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { form, loading, error, popUpCode } = useAppSelector(selectMusclesState);
  const open = popUpCode === PopUpCode.Create || popUpCode === PopUpCode.Update;

  const onClose = () => {
    if (loading) return;
    dispatch(setMusclesPopUpCode(PopUpCode.Default));
  };

  const onSubmit = async () => {
    const result = await dispatch(submitAdminMuscleForm());
    if (submitAdminMuscleForm.fulfilled.match(result)) {
      dispatch(setMusclesPopUpCode(PopUpCode.Default));
      void dispatch(fetchAdminMuscles());
    }
  };

  const disableSave =
    loading ||
    form.name.trim().length === 0 ||
    form.code.trim().length === 0;

  return (
    <PopupDialog
      open={open}
      title={form.id ? t('administration.muscles.editTitle') : t('administration.muscles.createTitle')}
      onClose={onClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      disableSave={disableSave}
      isSaving={loading}
      onSubmit={() => { void onSubmit(); }}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        {
          !form.id ? (
            <TextField
              label={t('common.fields.code')}
              value={form.code}
              onChange={(event) => dispatch(setMusclesForm({ ...form, code: event.target.value }))}
              disabled={Boolean(form.id)}
              helperText={form.id ? t('common.messages.codeReadOnly') : undefined}
              fullWidth
            />
          ) : null
        }
        <TextField
          label={t('common.fields.name')}
          value={form.name}
          onChange={(event) => dispatch(setMusclesForm({ ...form, name: event.target.value }))}
          fullWidth
        />

        <TextField
          label={t('common.fields.description')}
          value={form.description ?? ''}
          onChange={(event) => dispatch(setMusclesForm({ ...form, description: event.target.value }))}
          fullWidth
        />
        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
