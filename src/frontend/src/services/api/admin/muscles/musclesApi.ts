import type {
  IImportMusclesRequest,
  IImportMusclesResult,
  IMuscle,
  IMuscles,
  IMusclesFilter,
  IUpsertMuscleRequest,
} from '../../../../interfaces/muscles/IMuscles';
import { apiFetch } from '../../../httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

export function listMusclesPage(query: IMusclesFilter) {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );

  return apiFetch<IMuscles>(`/api/admin/muscles?${params.toString()}`, {
    headers: adminAuthHeaders,
  });
}

export function getMuscleById(id: string) {
  return apiFetch<IMuscle>(`/api/admin/muscles/${id}`, {
    headers: adminAuthHeaders,
  });
}

export function createMuscle(request: IUpsertMuscleRequest) {
  return apiFetch<IMuscle>('/api/admin/muscles', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function updateMuscle(id: string, request: IUpsertMuscleRequest) {
  return apiFetch<IMuscle>(`/api/admin/muscles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function deleteMuscle(id: string) {
  return apiFetch<void>(`/api/admin/muscles/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateMuscle(id: string) {
  return apiFetch<IMuscle>(`/api/admin/muscles/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  });
}

export function importMusclesCsv(request: IImportMusclesRequest) {
  return apiFetch<IImportMusclesResult>('/api/admin/muscles/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}
