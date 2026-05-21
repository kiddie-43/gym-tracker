import type { ExerciseDto, UpsertExerciseRequest } from '../../../../interfaces/admin/exercises/exercises';
import { apiFetch } from '../../../../shared/api/httpClient';
import { adminAuthHeaders } from '../common/adminAuthHeaders';

type ExerciseApiRow = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  difficulty: string;
  category: string;
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
  measurementTypeId: string;
  images: string[];
  videos: string[];
  isDeleted: boolean;
  deletedAt?: string | null;
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
    primaryMuscleIds: row.primaryMuscleIds,
    secondaryMuscleIds: row.secondaryMuscleIds,
    images: row.images,
    videos: row.videos,
    deletedAt: row.deletedAt,
  };
}

export function listExercises(includeDeleted = false) {
  return apiFetch<ExerciseApiRow[]>(`/api/admin/exercises?includeDeleted=${includeDeleted}`, {
    headers: adminAuthHeaders,
  }).then((rows) => rows.map(toExerciseDto));
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
