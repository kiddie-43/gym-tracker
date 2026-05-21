import type {
  CreateRoutineRequest,
  CreateRoutineSessionRequest,
  RoutineCard,
  RoutineDetail,
  RoutineSession,
  SessionExercise,
  AddSessionExerciseRequest,
  PlannedSet,
  UpdatePlannedSetRequest,
  UpdateRoutineRequest,
  CatalogAvailability,
} from '../../../interfaces/routines/routines';
import { apiFetch } from '../../httpClient';

export function listRoutines(includeDeleted = false) {
  const query = includeDeleted ? '?includeDeleted=true' : '';
  return apiFetch<RoutineCard[]>(`/api/routines${query}`);
}

export function getRoutineById(routineId: string) {
  return apiFetch<RoutineDetail>(`/api/routines/${routineId}`);
}

export function createRoutine(request: CreateRoutineRequest) {
  return apiFetch<RoutineDetail>('/api/routines', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function updateRoutine(routineId: string, request: UpdateRoutineRequest) {
  return apiFetch<RoutineDetail>(`/api/routines/${routineId}`, {
    method: 'PATCH',
    body: JSON.stringify(request),
  });
}

export function archiveRoutine(routineId: string) {
  return apiFetch<void>(`/api/routines/${routineId}`, {
    method: 'DELETE',
  });
}

export function reactivateRoutine(routineId: string) {
  return apiFetch<RoutineDetail>(`/api/routines/${routineId}/reactivate`, {
    method: 'POST',
  });
}

export function createRoutineSession(routineId: string, request: CreateRoutineSessionRequest) {
  return apiFetch<RoutineSession>(`/api/routines/${routineId}/sessions`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function addSessionExercise(routineId: string, sessionId: string, request: AddSessionExerciseRequest) {
  return apiFetch<SessionExercise>(`/api/routines/${routineId}/sessions/${sessionId}/exercises`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function unlinkSessionExercise(routineId: string, sessionId: string, exerciseId: string) {
  return apiFetch<void>(`/api/routines/${routineId}/sessions/${sessionId}/exercises/${exerciseId}`, {
    method: 'DELETE',
  });
}

export function updatePlannedSet(
  routineId: string,
  sessionId: string,
  exerciseId: string,
  setId: string,
  request: UpdatePlannedSetRequest,
) {
  return apiFetch<PlannedSet>(`/api/routines/${routineId}/sessions/${sessionId}/exercises/${exerciseId}/planned-sets/${setId}`, {
    method: 'PATCH',
    body: JSON.stringify(request),
  });
}

export function deletePlannedSet(routineId: string, sessionId: string, exerciseId: string, setId: string) {
  return apiFetch<void>(`/api/routines/${routineId}/sessions/${sessionId}/exercises/${exerciseId}/planned-sets/${setId}`, {
    method: 'DELETE',
  });
}

export function getCatalogAvailability() {
  return apiFetch<CatalogAvailability>('/api/routines/catalog/availability');
}
