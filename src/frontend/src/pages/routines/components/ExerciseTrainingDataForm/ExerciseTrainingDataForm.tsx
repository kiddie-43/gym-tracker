import { useState } from 'react';

import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import type { CreateExerciseTrainingLogRequest, TrainingAttachment } from '../../../../interfaces/routines/routines';

interface ExerciseTrainingDataFormProps {
  routineId: string;
  sessionId: string;
  exerciseId: string;
  onSubmit: (request: CreateExerciseTrainingLogRequest) => void;
}

export function ExerciseTrainingDataForm({ routineId, sessionId, exerciseId, onSubmit }: ExerciseTrainingDataFormProps) {
  const [repetitions, setRepetitions] = useState(8);
  const [weightKg, setWeightKg] = useState(20);
  const [notes, setNotes] = useState('');
  const [attachments, setAttachments] = useState<TrainingAttachment[]>([]);

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Datos del ejercicio</Typography>
      <TextField type="number" label="Repeticiones" value={repetitions} onChange={(event) => setRepetitions(Number(event.target.value))} />
      <TextField type="number" label="Peso (kg)" value={weightKg} onChange={(event) => setWeightKg(Number(event.target.value))} />
      <TextField label="Notas" value={notes} onChange={(event) => setNotes(event.target.value)} multiline minRows={3} />
      <Button
        variant="outlined"
        onClick={() => {
          if (attachments.length >= 5) {
            return;
          }

          setAttachments((current) => [...current, { type: 'photo', url: `https://files.local/${current.length + 1}` }]);
        }}
      >
        Agregar adjunto
      </Button>
      <Typography variant="caption">Adjuntos: {attachments.length}/5</Typography>
      <Button
        variant="contained"
        onClick={() => onSubmit({
          routineId,
          sessionId,
          exerciseId,
          performedSets: [
            {
              repetitions,
              weightKg,
              order: 1,
            },
          ],
          notes,
          attachments,
        })}
      >
        Guardar log
      </Button>
    </Stack>
  );
}
