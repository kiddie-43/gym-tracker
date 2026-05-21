import { useState, type Dispatch, type SetStateAction } from 'react';

import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import type { RoutineDetail } from '../../../../interfaces/routines/routines';
import { SessionDaysPicker } from '../SessionDaysPicker/SessionDaysPicker';

interface RoutineSessionsSectionProps {
  routine: RoutineDetail;
  onCreateSession: (name: string, days: string[]) => void;
  openDialog: boolean;
  setOpenDialog: Dispatch<SetStateAction<boolean>>;
}

export function RoutineSessionsSection({ routine, onCreateSession, openDialog, setOpenDialog }: RoutineSessionsSectionProps) {
  const [name, setName] = useState('');
  const [days, setDays] = useState<string[]>([]);
  const hasNameError = name.trim().length === 0;
  const hasDaysError = days.length === 0;

  const resetForm = () => {
    setName('');
    setDays([]);
  };

  const handleClose = () => {
    setOpenDialog(false);
    resetForm();
  };

  const handleSubmit = () => {
    if (hasNameError || hasDaysError) {
      return;
    }
    onCreateSession(name.trim(), days);
    handleClose();
  };

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        {routine.sessions.map((session) => (
          <Paper key={session.id} variant="outlined" sx={{ p: 1.5 }}>
            <Typography fontWeight={600}>{session.name}</Typography>
            <Typography variant="body2" color="text.secondary">
              {session.daysOfWeek.join(', ')}
            </Typography>
          </Paper>
        ))}
      </Stack>

      <Dialog open={openDialog} onClose={handleClose} fullWidth maxWidth="sm">
        <DialogTitle>Nueva sesion</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Nombre de sesion"
              value={name}
              onChange={(event) => setName(event.target.value)}
              required
              error={hasNameError}
              helperText={hasNameError ? 'El nombre es obligatorio' : ' '}
            />

            <SessionDaysPicker selectedDays={days} onChange={setDays} />
            {hasDaysError ? (
              <Typography variant="caption" color="error">Selecciona al menos un dia</Typography>
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancelar</Button>
          <Button variant="contained" onClick={handleSubmit} disabled={hasNameError || hasDaysError}>
            Guardar sesion
          </Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
}
