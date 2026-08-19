import type {
  IMuscle,
  IMusclesFilter,
} from '../../../interfaces/IMuscles/IMuscles';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { apiFetch } from '../../httpClient';
import { adminAuthHeaders } from '../admin/common/adminAuthHeaders';

export function listMusclesPage(query: IMusclesFilter) {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );

  return apiFetch<IPaginated<IMuscle>>(`/api/muscles?${params.toString()}`, {
    headers: adminAuthHeaders,
  });
}

export function getMuscleById(id: string) {
  return apiFetch<IMuscle>(`/api/muscles/${id}`, {
    headers: adminAuthHeaders,
  });
}

export function createMuscle(request: IMuscle) {
  return apiFetch<IMuscle>('/api/muscles', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function updateMuscle(id: string, request: IMuscle) {
  return apiFetch<IMuscle>(`/api/muscles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function deleteMuscle(id: string) {
  return apiFetch<void>(`/api/muscles/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateMuscle(id: string) {
  return apiFetch<IMuscle>(`/api/muscles/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  });
}

export function importMusclesCsv(request: string) {
  const formData = new FormData();
  formData.append('file', new Blob([request], { type: 'text/csv' }), 'muscles.csv');
  return apiFetch<IMuscle>('/api/muscles/import-csv', {
    method: 'POST',
    body: formData,
    headers: adminAuthHeaders,
  });
}
