import type {
  IAssignableMeasurementType,
  IImportMeasurementTypesRequest,
  IImportMeasurementTypesResult,
  IMeasurementType,
  IMeasurementTypesFilter,
  IMeasurementTypes,
} from '../../../../interfaces/admin/measurementTypes/measurementTypes';
import { apiFetch } from '../../../httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type AssignableMeasurementTypeApiRow = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
};

type MeasurementTypesPageApiRow = {
  items: IMeasurementType[];
  total?: number;
  totalCount?: number;
  page: number;
  pageSize: number;
};

function toAssignableMeasurementTypeDto(row: AssignableMeasurementTypeApiRow): IAssignableMeasurementType {
  return {
    id: row.id,
    code: row.code,
    name: row.name,
    description: row.description,
  };
}

export function listMeasurementTypesPage(query: IMeasurementTypesFilter): Promise<IMeasurementTypes> {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );
  return apiFetch<MeasurementTypesPageApiRow>(`/api/admin/measurement-types?${params.toString()}`, {
    headers: adminAuthHeaders,
  }).then((row) => ({
    items: Array.isArray(row.items) ? row.items : [],
    totalCount: typeof row.totalCount === 'number' ? row.totalCount : (typeof row.total === 'number' ? row.total : 0),
    page: typeof row.page === 'number' ? row.page : 0,
    pageSize: typeof row.pageSize === 'number' ? row.pageSize : 10,
  }));
}

export function getMeasurementTypeById(id: string): Promise<IMeasurementType> {
  return apiFetch<IMeasurementType>(`/api/admin/measurement-types/${id}`, {
    headers: adminAuthHeaders,
  });
}

export function createMeasurementType(request: IMeasurementType): Promise<IMeasurementType> {
  return apiFetch<IMeasurementType>('/api/admin/measurement-types', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function updateMeasurementType(id: string, request: IMeasurementType): Promise<IMeasurementType> {
  return apiFetch<IMeasurementType>(`/api/admin/measurement-types/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function deleteMeasurementType(id: string): Promise<void> {
  return apiFetch<void>(`/api/admin/measurement-types/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateMeasurementType(id: string): Promise<IMeasurementType> {
  return apiFetch<IMeasurementType>(`/api/admin/measurement-types/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  });
}

export function importMeasurementTypesCsv(request: IImportMeasurementTypesRequest): Promise<IImportMeasurementTypesResult> {
  return apiFetch<IImportMeasurementTypesResult>('/api/admin/measurement-types/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function listAssignableMeasurementTypes(): Promise<IAssignableMeasurementType[]> {
  return apiFetch<AssignableMeasurementTypeApiRow[]>('/api/admin/measurement-types/search', {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map(toAssignableMeasurementTypeDto));
}