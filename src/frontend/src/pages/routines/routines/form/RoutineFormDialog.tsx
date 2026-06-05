
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { IRoutine } from '../../../../interfaces/routines/IRoutines';
import { useSelector } from 'react-redux';
import { selectRoutinesForm } from '../../../../redux/states/routines/routinesState';
import { updateRoutineFormAction } from '../../../../redux/actions/routines/routinesActions';
import { useAppDispatch } from '../../../../redux/hooks';
import { useTranslation } from 'react-i18next';

interface RoutineFormDialogProps {
  open: boolean;
  initialTitle?: string;
  initialGoal?: string;
  onClose: () => void;
  onSubmit: (routine: IRoutine) => void;
}

const FORM_ID = 'routine-form-dialog';

export function RoutineFormDialog({
  open,
  onClose,
  onSubmit,
}: RoutineFormDialogProps) {
  const { id, name, description } = useSelector(selectRoutinesForm);
  const dispatch = useAppDispatch();
  const hasTitleError = name?.trim().length === 0;
  const { t } = useTranslation();
  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (hasTitleError) {
      return;
    }
    const routine: IRoutine = {
      id,
      name: name?.trim() || '',
      description: description?.trim() || '',
    }
    onSubmit(routine);
  };

  const onEditForm = (key: string, value: unknown) => {
    dispatch(updateRoutineFormAction({ key, value }));
  }
  return (
    <PopupDialog
      open={open}
      title={t('Rutina')}
      onClose={onClose}
      formId={FORM_ID}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      disableSave={hasTitleError}
      maxWidth="sm"
    >
      <Stack component="form" id={FORM_ID} onSubmit={handleSubmit} spacing={2} sx={{ mt: 1 }}>
        <TextField
          label={t('common.fields.name')}
          value={name}
          onChange={(event) => onEditForm('name', event.target.value)}
          required
          error={hasTitleError}
          helperText={hasTitleError ? t('common.messages.nameRequired') : ' '}
        />
        <TextField
          label={t('common.fields.objective')}
          value={description}
          onChange={(event) => onEditForm('description', event.target.value)}
          multiline
          minRows={2}
        />
      </Stack>
    </PopupDialog>
  );
}