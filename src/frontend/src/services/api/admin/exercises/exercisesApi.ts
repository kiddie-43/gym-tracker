import type {
  ExerciseDto,
  ImportExercisesRequest,
  ImportExercisesResult,
  ExercisesListQuery,
  ExercisesPageDto,
  UpsertExerciseRequest,
  RequestUploadUrlRequest,
  MediaUploadTicket,
  ConfirmExerciseMediaRequest,
} from '../../../../interfaces/admin/exercises/exercises';
import type { MuscleDto } from '../../../../interfaces/admin/muscles/muscles';
import { apiFetch } from '../../../httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type ExerciseApiRow = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  difficulty: string;
  category: string;
  primaryMuscles: MuscleDto[];
  secondaryMuscles: MuscleDto[];
  measurementTypeId: string;
  measurementTypeName: string;
  images: string[];
  videos: string[];
  isDeleted: boolean;
  deletedAt?: string | null;
};

type ExercisesPageApiRow = {
  items: ExerciseApiRow[];
  totalCount: number;
  page: number;
  pageSize: number;
};

function toExerciseDto(row: ExerciseApiRow): ExerciseDto {
  return {
    id: row.id,
    name: row.name,
    code: row.code,
    description: row.description,
    active: !row.isDeleted,
    isDeleted: row.isDeleted,
    category: row.category,
    difficulty: row.difficulty,
    measurementTypeId: row.measurementTypeId,
    measurementTypeName: row.measurementTypeName,
    primaryMuscles: row.primaryMuscles ?? [],
    secondaryMuscles: row.secondaryMuscles ?? [],
    images: row.images,
    videos: row.videos,
    deletedAt: row.deletedAt,
  };
}

export function listExercises(includeDeleted = false) {
  return listExercisesPage({ includeDeleted }).then((page) => page.items);
}

export function listExercisesPage(query: ExercisesListQuery) {
  return apiFetch<ExercisesPageApiRow>(`/api/admin/exercises?${toListQueryParams(query)}`, {
    headers: adminAuthHeaders,
  }).then(toExercisesPageDto);
}

export function createExercise(request: UpsertExerciseRequest) {
  return apiFetch<ExerciseApiRow>('/api/admin/exercises', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toExerciseDto);
}

export function updateExercise(id: string, request: UpsertExerciseRequest) {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toExerciseDto);
}

export function deleteExercise(id: string) {
  return apiFetch<void>(`/api/admin/exercises/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateExercise(id: string) {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  }).then(toExerciseDto);
}

export function importExercisesCsv(request: ImportExercisesRequest) {
  return apiFetch<ImportExercisesResult>('/api/admin/exercises/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function requestExerciseUploadUrl(exerciseId: string, request: RequestUploadUrlRequest) {
  return apiFetch<MediaUploadTicket>(`/api/admin/exercises/${exerciseId}/media/request-upload`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}

export function confirmExerciseMedia(exerciseId: string, request: ConfirmExerciseMediaRequest) {
  return apiFetch<ExerciseApiRow>(`/api/admin/exercises/${exerciseId}/media/confirm`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  }).then(toExerciseDto);
}

function toExercisesPageDto(row: ExercisesPageApiRow): ExercisesPageDto {
  if (Array.isArray(row as unknown as ExerciseApiRow[])) {
    const legacyItems = row as unknown as ExerciseApiRow[];
    return {
      items: legacyItems.map(toExerciseDto),
      totalCount: legacyItems.length,
      page: 1,
      pageSize: legacyItems.length || 10,
    };
  }

  const items = Array.isArray(row.items) ? row.items : [];

  return {
    items: items.map(toExerciseDto),
    totalCount: typeof row.totalCount === 'number' ? row.totalCount : items.length,
    page: typeof row.page === 'number' && row.page > 0 ? row.page : 1,
    pageSize: typeof row.pageSize === 'number' && row.pageSize > 0 ? row.pageSize : (items.length || 10),
  };
}

function toListQueryParams(query: ExercisesListQuery): string {
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
    query.difficulties.forEach((d) => params.append('difficulties', d));
  }

  if (query.measurementTypeIds && query.measurementTypeIds.length > 0) {
    query.measurementTypeIds.forEach((id) => params.append('measurementTypeIds', id));
  }

  if (query.primaryMuscleIds && query.primaryMuscleIds.length > 0) {
    query.primaryMuscleIds.forEach((id) => params.append('primaryMuscleIds', id));
  }

  if (query.secondaryMuscleIds && query.secondaryMuscleIds.length > 0) {
    query.secondaryMuscleIds.forEach((id) => params.append('secondaryMuscleIds', id));
  }

  if (query.page && query.page > 0) {
    params.set('page', String(query.page));
  }

  if (query.pageSize && query.pageSize > 0) {
    params.set('pageSize', String(query.pageSize));
  }

  return params.toString();
}
