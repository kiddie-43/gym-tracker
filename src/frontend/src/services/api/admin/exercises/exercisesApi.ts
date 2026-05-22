import type {
  IConfirmExerciseMediaRequest,
  IExercise,
  IExercises,
  IExercisesFilter,
  IImportExercisesRequest,
  IImportExercisesResult,
  IMediaUploadTicket,
  IRequestUploadUrlRequest,
} from '../../../../interfaces/admin/exercises/exercises';
import type { IMuscle } from '../../../../interfaces/muscles/IMuscles';
import { apiFetch } from '../../../httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type ExerciseApiRow = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  difficulty: string;
  category: string;
  primaryMuscles: IMuscle[];
  secondaryMuscles: IMuscle[];
  primaryMuscleIds?: string[];
  secondaryMuscleIds?: string[];
  measurementTypeIds: string[];
  measurementTypeNames: string[];
  images?: string[];
  videos?: string[];
  isDeleted: boolean;
  deletedAt?: string | null;
};

type ExercisesPageApiRow = {
  items: ExerciseApiRow[];
  total?: number;
  totalCount?: number;
  page: number;
  pageSize: number;
};

type UpsertExerciseApiRequest = {
  name: string;
  code: string;
  description?: string | null;
  category: string;
  difficulty: string;
  measurementTypeIds: string[];
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
  images?: string[];
  videos?: string[];
};

function toIExercise(row: ExerciseApiRow): IExercise {
  return {
    id: row.id,
    name: row.name,
    code: row.code,
    description: row.description,
    active: !row.isDeleted,
    isDeleted: row.isDeleted,
    category: row.category,
    difficulty: row.difficulty,
    measurementTypeIds: row.measurementTypeIds ?? [],
    measurementTypeNames: row.measurementTypeNames ?? [],
    primaryMuscles: row.primaryMuscles ?? [],
    secondaryMuscles: row.secondaryMuscles ?? [],
    images: row.images ?? [],
    videos: row.videos ?? [],
    deletedAt: row.deletedAt,
  };
}

export function listExercises(includeDeleted = false) {
  return listExercisesPage({ includeDeleted }).then((page) => page.items);
}

export function listExercisesPage(query: IExercisesFilter): Promise<IExercises> {
  return apiFetch<ExercisesPageApiRow>(`/api/admin/exercises?${toListQueryParams(query)}`, {
    headers: adminAuthHeaders,
  }).then(toIExercisesPage);
}

export function createExercise(request: UpsertExerciseApiRequest): Promise<IExercise> {
  return apiFetch<ExerciseApiRow>('/api/admin/exercises', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toIExercise);
}

export function updateExercise(id: string, request: UpsertExerciseApiRequest): Promise<IExercise> {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toIExercise);
}

export function deleteExercise(id: string) {
  return apiFetch<void>(`/api/admin/exercises/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateExercise(id: string): Promise<IExercise> {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  }).then(toIExercise);
}

export function importExercisesCsv(request: IImportExercisesRequest): Promise<IImportExercisesResult> {
  return apiFetch<IImportExercisesResult>('/api/admin/exercises/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function requestExerciseUploadUrl(exerciseId: string, request: IRequestUploadUrlRequest): Promise<IMediaUploadTicket> {
  return apiFetch<IMediaUploadTicket>(`/api/admin/exercises/${exerciseId}/media/request-upload`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function confirmExerciseMedia(exerciseId: string, request: IConfirmExerciseMediaRequest): Promise<IExercise> {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${exerciseId}/media/confirm`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toIExercise);
}

function toIExercisesPage(row: ExercisesPageApiRow): IExercises {
  if (Array.isArray(row as unknown as ExerciseApiRow[])) {
    const legacyItems = row as unknown as ExerciseApiRow[];
    return {
      items: legacyItems.map(toIExercise),
      totalCount: legacyItems.length,
      page: 1,
      pageSize: legacyItems.length || 10,
    };
  }

  const items = Array.isArray(row.items) ? row.items : [];

  return {
    items: items.map(toIExercise),
    totalCount: typeof row.totalCount === 'number' ? row.totalCount : (typeof row.total === 'number' ? row.total : items.length),
    page: typeof row.page === 'number' && row.page >= 0 ? row.page : 0,
    pageSize: typeof row.pageSize === 'number' && row.pageSize > 0 ? row.pageSize : (items.length || 10),
  };
}

function toListQueryParams(query: IExercisesFilter): string {
  const params = new URLSearchParams();

  params.set('includeDeleted', String(Boolean(query.includeDeleted)));

  if (query.search && query.search.trim().length > 0) {
    params.set('search', query.search.trim());
  }

  if (query.sortBy) {
    params.set('sortBy', query.sortBy);
  }

  if (query.sortDirection) {
    params.set('sortDirection', query.sortDirection);
  }

  if (query.difficulties && query.difficulties.length > 0) {
    query.difficulties.forEach((d: string) => params.append('difficulties', d));
  }

  if (query.measurementTypeIds && query.measurementTypeIds.length > 0) {
    query.measurementTypeIds.forEach((id: string) => params.append('measurementTypeIds', id));
  }

  if (query.primaryMuscleIds && query.primaryMuscleIds.length > 0) {
    query.primaryMuscleIds.forEach((id: string) => params.append('primaryMuscleIds', id));
  }

  if (query.secondaryMuscleIds && query.secondaryMuscleIds.length > 0) {
    query.secondaryMuscleIds.forEach((id: string) => params.append('secondaryMuscleIds', id));
  }

  if (typeof query.page === 'number' && query.page >= 0) {
    params.set('page', String(query.page));
  }

  if (query.pageSize && query.pageSize > 0) {
    params.set('pageSize', String(query.pageSize));
  }

  return params.toString();
}
