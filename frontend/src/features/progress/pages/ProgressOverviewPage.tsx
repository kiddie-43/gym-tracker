import { useEffect, useState } from 'react';

import { useParams } from 'react-router-dom';

import { PageHeader } from '../../../shared/components/PageHeader';
import type { WorkoutProgress } from '../../../shared/types/workouts';
import { getExerciseProgress } from '../../../services/workouts/workoutsApi';
import { ProgressSummaryPanel } from '../components/ProgressSummaryPanel';

export function ProgressOverviewPage() {
  const { exerciseId = 'bench-press' } = useParams();
  const [progress, setProgress] = useState<WorkoutProgress | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadProgress() {
      setIsLoading(true);
      setError(null);

      try {
        const response = await getExerciseProgress(exerciseId);
        if (isMounted) {
          setProgress(response);
        }
      } catch (loadError) {
        if (isMounted) {
          setError(loadError instanceof Error ? loadError.message : 'No se pudo cargar el progreso.');
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadProgress();

    return () => {
      isMounted = false;
    };
  }, [exerciseId]);

  return (
    <section>
      <PageHeader title="Resumen de progreso" description="Compara la ultima sesion contra el historial reciente de un ejercicio." />
      <ProgressSummaryPanel progress={progress} isLoading={isLoading} error={error} />
    </section>
  );
}
