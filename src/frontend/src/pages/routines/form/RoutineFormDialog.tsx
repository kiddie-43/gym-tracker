import { useEffect, useState } from 'react';

import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';

interface RoutineFormDialogProps {
  open: boolean;
  initialTitle?: string;
  initialGoal?: string;
  onClose: () => void;
  onSubmit: (title: string, goal?: string) => void;
}

const FORM_ID = 'routine-form-dialog';

export function RoutineFormDialog({
  open,
  initialTitle = '',
  initialGoal = '',
  onClose,
  onSubmit,
}: RoutineFormDialogProps) {
  const [title, setTitle] = useState(initialTitle);
  const [goal, setGoal] = useState(initialGoal);

  useEffect(() => {
    if (open) {
      setTitle(initialTitle);
      setGoal(initialGoal);
    }
  }, [open, initialTitle, initialGoal]);

  const hasTitleError = title.trim().length === 0;

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (hasTitleError) {
      return;
    }

    onSubmit(title.trim(), goal.trim() || undefined);
  };

  return (
    <PopupDialog
      open={open}
      title="Rutina"
      onClose={onClose}
      formId={FORM_ID}
      closeLabel="Cancelar"
      saveLabel="Guardar"
      disableSave={hasTitleError}
      maxWidth="sm"
    >
      <Stack component="form" id={FORM_ID} onSubmit={handleSubmit} spacing={2} sx={{ mt: 1 }}>
        <TextField
          label="Nombre"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          required
          error={hasTitleError}
          helperText={hasTitleError ? 'El nombre es obligatorio' : ' '}
        />
        <TextField
          label="Objetivo"
          value={goal}
          onChange={(event) => setGoal(event.target.value)}
          multiline
          minRows={2}
        />
      </Stack>
    </PopupDialog>
  );
}