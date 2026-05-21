import { useEffect, useState } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import { apiFetch } from '../../services/httpClient';
import { MuscleGroupSelector } from './components/MuscleGroupSelector/MuscleGroupSelector';

interface MuscleGroupOption {
  id: string;
  name: string;
}

export function RoutineBuilderPage() {
  const [title, setTitle] = useState('');
  const [muscles, setMuscles] = useState<MuscleGroupOption[]>([]);
  const [selectedMuscles, setSelectedMuscles] = useState<string[]>([]);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    let active = true;

    const load = async () => {
      const response = await apiFetch<MuscleGroupOption[]>('/api/catalog/muscle-groups', { method: 'GET' });
      if (active) {
        setMuscles(response);
      }
    };

    void load();

    return () => {
      active = false;
    };
  }, []);

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="h5">Constructor de rutina</Typography>
        <TextField
          label="Nombre"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
        />
        <MuscleGroupSelector
          value={selectedMuscles}
          options={muscles}
          onChange={setSelectedMuscles}
        />
        <Button
          variant="contained"
          onClick={async () => {
            await apiFetch('/api/routines', {
              method: 'POST',
              body: JSON.stringify({
                title: title || 'Nueva rutina',
                tags: ['template'],
                days: [],
                muscleGroupIds: selectedMuscles,
              }),
            });
            setSaved(true);
          }}
        >
          Guardar rutina
        </Button>
        {saved ? <Alert severity="success">Rutina guardada</Alert> : null}
      </Stack>
    </Paper>
  );
}
