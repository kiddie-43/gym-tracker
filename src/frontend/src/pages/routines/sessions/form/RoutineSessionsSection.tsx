
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useSelector } from 'react-redux';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { useAppDispatch } from '../../../../redux/hooks';
import { updateSessionFormAction } from '../../../../redux/actions/sessions/sessionsAction';
import { selectSessionsForm } from '../../../../redux/states/session/session';
import { SessionDaysPicker } from './SessionDaysPicker';
import { ISession } from '../../../../interfaces/ISession/ISession';

interface RoutineSessionsFormProps {
  open: boolean;
  onCreate: (session: ISession) => void;
  onClose: () => void;
}

export function RoutineSessionsForm({ onCreate, open, onClose }: RoutineSessionsFormProps) {
  const dispatch = useAppDispatch();
  const { id, name, daysOfWeek } = useSelector(selectSessionsForm);
  const formId = 'routine-session-form';

  const hasNameError = name.trim().length === 0;
  const hasDaysError = daysOfWeek.length === 0;

  const onEditForm = (key: string, value: unknown) => {
    dispatch(updateSessionFormAction({ key, value }));
  };



  const handleSubmit = (event?: React.FormEvent<HTMLFormElement>) => {
    event?.preventDefault();

    
    const session: ISession = {
      name: name.trim(),
      daysOfWeek,
      id: id ?? ''
    }
    onCreate(session);
    
  };

  return (
    <PopupDialog
      open={open}
      title="Sesion"
      onClose={onClose}
      onSubmit={() => handleSubmit()}
      formId={formId}

      closeLabel="Cancelar"
      saveLabel="Guardar"
      disableSave={hasNameError || hasDaysError}
      maxWidth="sm"
    >
      <Stack id={formId} component="form" onSubmit={handleSubmit} spacing={2} sx={{ mt: 1 }}>
        <TextField
          label="Nombre"
          value={name}
          onChange={(event) => onEditForm('name', event.target.value)}
          required
          error={hasNameError}
          helperText={hasNameError ? 'El nombre es obligatorio' : ' '}
        />

        <SessionDaysPicker
          selectedDays={daysOfWeek}
          onChange={(days) => onEditForm('daysOfWeek', days)}
        />

        {hasDaysError ? (
          <Typography variant="caption" color="error">Selecciona al menos un dia</Typography>
        ) : null}
      </Stack>
    </PopupDialog>
  );
}
