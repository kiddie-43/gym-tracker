import type {
  MeasurementTypeDto,
  UpsertMeasurementTypeRequest,
} from '../../../../interfaces/admin/measurementTypes/measurementTypes';
import { apiFetch } from '../../../../shared/api/httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type MeasurementTypeApiRow = {
  id: string;
  name: string;
  fields: Array<{ name: string }>;
  isDeleted: boolean;
  deletedAt?: string | null;
};

function toMeasurementTypeDto(row: MeasurementTypeApiRow): MeasurementTypeDto {
  return {
    id: row.id,
    name: row.name,
    fields: row.fields,
    active: !row.isDeleted,
    isDeleted: row.isDeleted,
    deletedAt: row.deletedAt,
  };
}

export function listMeasurementTypes(includeDeleted = false) {
  return apiFetch<MeasurementTypeApiRow[]>(`/api/admin/measurement-types?includeDeleted=${includeDeleted}`, {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map(toMeasurementTypeDto));
}

export function createMeasurementType(request: UpsertMeasurementTypeRequest) {
  return apiFetch<MeasurementTypeApiRow>('/api/admin/measurement-types', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toMeasurementTypeDto);
}

export function updateMeasurementType(id: string, request: UpsertMeasurementTypeRequest) {
  return apiFetch<MeasurementTypeApiRow>(`/api/admin/measurement-types/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toMeasurementTypeDto);
}

export function deleteMeasurementType(id: string) {
  return apiFetch<void>(`/api/admin/measurement-types/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateMeasurementType(id: string) {
  return apiFetch<MeasurementTypeApiRow>(`/api/admin/measurement-types/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  }).then(toMeasurementTypeDto);
}
