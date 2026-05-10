import { apiFetch } from '../../../shared/api/httpClient';
import type { CreateRoutineRequest, Routine, RoutineSummary } from '../../../shared/types/routines';
import type { Exercise, MuscleGroup } from '../../../shared/types/catalog';

const authHeaders = {
  Authorization: 'Bearer demo-user',
};

export function listRoutines() {
  return apiFetch<RoutineSummary[]>('/api/routines', { headers: authHeaders });
}

export function createRoutine(request: CreateRoutineRequest) {
  return apiFetch<Routine>('/api/routines', {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify(request),
  });
}

export function getRoutine(id: string) {
  return apiFetch<Routine>(`/api/routines/${id}`, { headers: authHeaders });
}

export function listMuscleGroups() {
  return apiFetch<MuscleGroup[]>('/api/catalog/muscle-groups', { headers: authHeaders });
}

export function listSuggestedExercises(muscleGroupIds: string[]) {
  const parameters = new URLSearchParams();
  parameters.set('muscleGroupIds', muscleGroupIds.join(','));

  return apiFetch<{ exercises: Exercise[] }>(`/api/routines/suggested-exercises?${parameters.toString()}`, { headers: authHeaders });
}
