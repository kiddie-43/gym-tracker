import type {
  AssignableMeasurementTypeDto,
  ImportMeasurementTypesRequest,
  ImportMeasurementTypesResult,
  MeasurementTypesListQuery,
  MeasurementTypesPageDto,
  MeasurementTypeDto,
  UpsertMeasurementTypeRequest,
} from '../../../../interfaces/admin/measurementTypes/measurementTypes';
import { apiFetch } from '../../../httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type MeasurementTypeApiRow = {
  id: string;
  key?: string;
  name: string;
  unit?: string;
  dataType?: string;
  category?: string;
  description?: string | null;
  fields?: Array<{ name: string }>;
  active?: boolean;
  isDeleted: boolean;
  deletedAt?: string | null;
};

type AssignableMeasurementTypeApiRow = {
  id: string;
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  fields?: Array<{ name: string }>;
};

type MeasurementTypesPageApiRow = {
  items: MeasurementTypeApiRow[];
  totalCount: number;
  page: number;
  pageSize: number;
};

function toMeasurementTypeDto(row: MeasurementTypeApiRow): MeasurementTypeDto {
  return {
    id: row.id,
    key: row.key ?? row.name.trim().toUpperCase().replace(/\s+/g, '_'),
    name: row.name,
    unit: row.unit ?? 'count',
    dataType: row.dataType ?? 'integer',
    category: row.category ?? 'general',
    description: row.description ?? null,
    active: row.active ?? !row.isDeleted,
    isDeleted: row.isDeleted,
    deletedAt: row.deletedAt,
  };
}

function toAssignableMeasurementTypeDto(row: AssignableMeasurementTypeApiRow): AssignableMeasurementTypeDto {
  const metrics = Array.isArray(row.fields)
    ? row.fields
      .map((field) => field.name.trim())
      .filter((field) => field.length > 0)
    : [];

  return {
    id: row.id,
    key: row.key,
    name: row.name,
    unit: row.unit,
    dataType: row.dataType,
    category: row.category,
    metrics: metrics.length > 0 ? metrics : [row.name],
  };
}

export function listMeasurementTypes(includeInactive = false) {
  return listMeasurementTypesPage({ includeInactive }).then((page) => page.items);
}

export function listMeasurementTypesPage(query: MeasurementTypesListQuery) {
  return apiFetch<MeasurementTypesPageApiRow>(`/api/admin/measurement-types?${toListQueryParams(query)}`, {
    headers: adminAuthHeaders,
  }).then(toMeasurementTypesPageDto);
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

export function importMeasurementTypesCsv(request: ImportMeasurementTypesRequest) {
  return apiFetch<ImportMeasurementTypesResult>('/api/admin/measurement-types/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function listAssignableMeasurementTypes() {
  return apiFetch<AssignableMeasurementTypeApiRow[]>('/api/admin/measurement-types/assignable', {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map(toAssignableMeasurementTypeDto));
}

function toMeasurementTypesPageDto(row: MeasurementTypesPageApiRow): MeasurementTypesPageDto {
  if (Array.isArray(row as unknown as MeasurementTypeApiRow[])) {
    const legacyItems = row as unknown as MeasurementTypeApiRow[];
    return {
      items: legacyItems.map(toMeasurementTypeDto),
      totalCount: legacyItems.length,
      page: 1,
      pageSize: legacyItems.length || 10,
    };
  }

  const items = Array.isArray(row.items) ? row.items : [];

  return {
    items: items.map(toMeasurementTypeDto),
    totalCount: typeof row.totalCount === 'number' ? row.totalCount : items.length,
    page: typeof row.page === 'number' && row.page > 0 ? row.page : 1,
    pageSize: typeof row.pageSize === 'number' && row.pageSize > 0 ? row.pageSize : (items.length || 10),
  };
}

function toListQueryParams(query: MeasurementTypesListQuery): string {
  const params = new URLSearchParams();

  params.set('includeInactive', String(Boolean(query.includeInactive)));

  if (query.search && query.search.trim().length > 0) {
    params.set('search', query.search.trim());
  }

  if (query.code && query.code.trim().length > 0) {
    params.set('code', query.code.trim());
  }

  if (query.sortBy) {
    params.set('sortBy', query.sortBy);
  }

  if (query.sortDirection) {
    params.set('sortDirection', query.sortDirection);
  }

  if (query.page && query.page > 0) {
    params.set('page', String(query.page));
  }

  if (query.pageSize && query.pageSize > 0) {
    params.set('pageSize', String(query.pageSize));
  }

  return params.toString();
}
