import type {
  ImportMusclesRequest,
  ImportMusclesResult,
  MuscleDto,
  UpsertMuscleRequest,
} from '../../../../interfaces/admin/muscles/muscles';
import { apiFetch } from '../../../../shared/api/httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type MuscleApiRow = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isDeleted: boolean;
  deletedAt?: string | null;
  muscleGroupIds: string[];
};

function toMuscleDto(row: MuscleApiRow): MuscleDto {
  return {
    id: row.id,
    name: row.name,
    code: row.code,
    description: row.description,
    active: !row.isDeleted,
    isDeleted: row.isDeleted,
    deletedAt: row.deletedAt,
    muscleGroupIds: row.muscleGroupIds,
  };
}

export function listMuscles(includeDeleted = false) {
  return apiFetch<MuscleApiRow[]>(`/api/admin/muscles?includeDeleted=${includeDeleted}`, {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map(toMuscleDto));
}

export function createMuscle(request: UpsertMuscleRequest) {
  return apiFetch<MuscleApiRow>('/api/admin/muscles', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toMuscleDto);
}

export function updateMuscle(id: string, request: UpsertMuscleRequest) {
  return apiFetch<MuscleApiRow>(`/api/admin/muscles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toMuscleDto);
}

export function deleteMuscle(id: string) {
  return apiFetch<void>(`/api/admin/muscles/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateMuscle(id: string) {
  return apiFetch<MuscleApiRow>(`/api/admin/muscles/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  }).then(toMuscleDto);
}

export function importMusclesCsv(request: ImportMusclesRequest) {
  return apiFetch<ImportMusclesResult>('/api/admin/muscles/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}
