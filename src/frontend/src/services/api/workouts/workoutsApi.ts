import { CreateWorkoutRequest, WorkoutProgress, WorkoutSummary } from '../../../interfaces/workouts';
import { apiFetch } from '../../httpClient';

type WorkoutCatalogExerciseDto = {
  id: string;
  name: string;
  muscleGroupIds: string[];
  coverStoragePath?: string | null;
  formTypeId: string;
  formTypeCode: string;
};

export function createWorkout(request: CreateWorkoutRequest) {
  return apiFetch<WorkoutSummary>('/api/workouts', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}

export function getExerciseProgress(exerciseId: string) {
  return apiFetch<WorkoutProgress>(`/api/progress/exercises/${exerciseId}`, {
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}

export function getWorkoutHistory(page = 1, pageSize = 20, from?: string, to?: string) {
  const params = new URLSearchParams();
  params.set('page', String(page));
  params.set('pageSize', String(pageSize));
  if (from) {
    params.set('from', from);
  }
  if (to) {
    params.set('to', to);
  }

  return apiFetch<{ items: WorkoutSummary[]; totalCount: number; page: number; pageSize: number }>(`/api/workouts?${params.toString()}`, {
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}

export function listCatalogExercises(query?: string, muscleGroupIds?: string[]) {
  const params = new URLSearchParams();
  if (query) {
    params.set('query', query);
  }

  for (const id of muscleGroupIds ?? []) {
    params.append('muscleGroupIds', id);
  }

  const queryString = params.toString();
  const path = queryString ? `/api/workouts/exercise-catalog?${queryString}` : '/api/workouts/exercise-catalog';

  return apiFetch<WorkoutCatalogExerciseDto[]>(path, {
    headers: {
      Authorization: 'Bearer demo-user',
    },
  }).then((rows) => rows.map((row) => ({
    id: row.id,
    name: row.name,
    muscleGroupIds: row.muscleGroupIds,
    imageUrl: row.coverStoragePath ?? null,
    formTypeId: row.formTypeId,
    formTypeCode: row.formTypeCode,
  })));
}

export function getWorkout(workoutId: string) {
  return apiFetch<WorkoutSummary>(`/api/workouts/${workoutId}`, {
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}

export function updateWorkout(workoutId: string, request: CreateWorkoutRequest) {
  return apiFetch<WorkoutSummary>(`/api/workouts/${workoutId}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}

export function deleteWorkout(workoutId: string) {
  return apiFetch<void>(`/api/workouts/${workoutId}`, {
    method: 'DELETE',
    headers: {
      Authorization: 'Bearer demo-user',
    },
  });
}
