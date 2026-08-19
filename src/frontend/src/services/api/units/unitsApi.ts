
import { IUnit, IUnitFilters } from '../../../interfaces/admin/units/IUnit';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { apiFetch } from '../../httpClient';
import { adminAuthHeaders } from '../admin/common/adminAuthHeaders';




export function listUnitsPage(query: IUnitFilters): Promise<IPaginated<IUnit>> {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );
  return apiFetch<IPaginated<IUnit>>(`/api/Units?${params.toString()}`, {
    headers: adminAuthHeaders,
  }).then((row) => ({
    items: Array.isArray(row.items) ? row.items : [],
    totalCount: row.totalCount,
    page: typeof row.page === 'number' ? row.page : 0,
    pageSize: typeof row.pageSize === 'number' ? row.pageSize : 10,
  }));
}

export function getUnitsById(id: string): Promise<IUnit> {
  return apiFetch<IUnit>(`/api/Units/${id}`, {
    headers: adminAuthHeaders,
  });
}

export function createUnit(request: IUnit): Promise<IUnit> {
  return apiFetch<IUnit>('/api/Units', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function updateUnit(id: string, request: IUnit): Promise<IUnit> {
  return apiFetch<IUnit>(`/api/Units/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function deleteUnit(id: string): Promise<void> {
  return apiFetch<void>(`/api/Units/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateUnit(id: string): Promise<IUnit> {
  return apiFetch<IUnit>(`/api/Units/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  });
}

export function importUnitsCsv(request: string): Promise<IUnit> {
  const formData = new FormData();
  formData.append('file', new Blob([request], { type: 'text/csv' }), 'units.csv');
  return apiFetch<IUnit>('/api/Units/import-csv', {
    method: 'POST',
    body: formData,
    headers: adminAuthHeaders,
  });
}

export function listAssignableUnits(): Promise<IUnit[]> {
  return apiFetch<IUnit[]>('/api/Units/search', {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map((row) => ({
    id: row.id,
    code: row.code,
    name: row.name,
    description: row.description,
  })));
}