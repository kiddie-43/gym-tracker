
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import {
  IRoutine,
  IRoutineFilter,
} from '../../../interfaces/routines/IRoutines';
import { apiFetch } from '../../httpClient';



export function listRoutinesPage(query: IRoutineFilter) {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );

  return apiFetch<IPaginated<IRoutine>>(`/api/routines${params.toString() ? `?${params.toString()}` : ''}`);
}

export function getRoutineById(id: string) {
  return apiFetch<IRoutine>(`/api/routines/${id}`);
}

export function createRoutine(request: IRoutine) {
  return apiFetch<IRoutine>('/api/routines', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function updateRoutine(id: string, request: IRoutine) {
  return apiFetch<IRoutine>(`/api/routines/${id}`, {
    method: 'PATCH',
    body: JSON.stringify(request),
  });
}

export function deleteRoutine(id: string) {
  return apiFetch<void>(`/api/routines/${id}`, {
    method: 'DELETE',
  });
}

export function reactivateRoutine(id: string) {
  return apiFetch<IRoutine>(`/api/routines/${id}/reactivate`, {
    method: 'POST',
  });
}
