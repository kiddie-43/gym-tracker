import { useEffect, useState } from 'react';

import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';

import { PageHeader } from '../../../shared/components/PageHeader';
import type { MuscleGroup } from '../../../shared/types/catalog';
import { createRoutine, listMuscleGroups } from '../api/routinesApi';
import { RoutineForm } from '../components/RoutineForm';

export function RoutineBuilderPage() {
  const [muscleGroups, setMuscleGroups] = useState<MuscleGroup[]>([]);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadMuscleGroups() {
      try {
        const groups = await listMuscleGroups();
        if (isMounted) {
          setMuscleGroups(groups);
        }
      } catch {
        if (isMounted) {
          setMuscleGroups([]);
        }
      }
    }

    void loadMuscleGroups();

    return () => {
      isMounted = false;
    };
  }, []);

  async function handleSubmit(request: Parameters<typeof createRoutine>[0]) {
    setStatusMessage(null);
    setErrorMessage(null);

    try {
      await createRoutine(request);
      setStatusMessage('Rutina guardada.');
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'No se pudo guardar la rutina.');
    }
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="Constructor de rutinas" description="Crea plantillas de dias reutilizables por enfoque muscular." />
      {statusMessage ? <Alert severity="success">{statusMessage}</Alert> : null}
      {errorMessage ? <Alert severity="error">{errorMessage}</Alert> : null}
      <RoutineForm muscleGroups={muscleGroups} onSubmit={handleSubmit} />
    </Stack>
  );
}
