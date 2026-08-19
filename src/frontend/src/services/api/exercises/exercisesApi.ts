import type { IExercise, IExercisesFilter } from '../../../interfaces/IExercises/IExercises';
import type { IImportExercisesResult } from '../../../interfaces/IExercises/IExercises';
import type { ISelectorOption } from '../../../interfaces/skeleton/ISelectorOption/ISelectorOption';
import type { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { apiFetch } from '../../httpClient';
import { adminAuthHeaders } from '../admin/common/adminAuthHeaders';

export function listExercisesPage(query: IExercisesFilter = {}) {
  const params = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') {
      return;
    }

    if (Array.isArray(value)) {
      value.forEach((item) => {
        if (item !== undefined && item !== null && item !== '') {
          params.append(key, String(item));
        }
      });
      return;
    }

    params.append(key, String(value));
  });

  return apiFetch<IPaginated<IExercise>>(`/api/exercices${params.toString() ? `?${params.toString()}` : ''}`, {
    headers: adminAuthHeaders,
  });
}

export function getExerciseById(id: string) {
  return apiFetch<IExercise>(`/api/exercices/${id}`, {
    headers: adminAuthHeaders,
  });
}

export function listExerciseTypes() {
  return apiFetch<ISelectorOption[]>('/api/exercices/types', {
    headers: adminAuthHeaders,
  });
}

export function createExercise(request: IExercise) {
 const exerciceToSave = {
    code: request.code,
    name: request.name,
    description: request.description,
   exerciseType: request.exerciseType ?? 'STRENGTH',
    primaryMuscles: request.primaryMuscles.map(muscle => muscle.id),
    secondaryMuscles: request.secondaryMuscles.map(muscle => muscle.id),
    units: request.units.map(unit => unit.id),
  };
  return apiFetch<IExercise>('/api/exercices', {
    method: 'POST',
    body: JSON.stringify(exerciceToSave),
    headers: adminAuthHeaders,
  });
}

export function updateExercise(id: string, request: IExercise) {

  const exerciceToSave = {
    name: request.name,
    description: request.description,
    exerciseType: request.exerciseType ?? 'STRENGTH',
    primaryMuscles: request.primaryMuscles.map(muscle => muscle.id),
    secondaryMuscles: request.secondaryMuscles.map(muscle => muscle.id),
    units: request.units.map(unit => unit.id),
  };



  return apiFetch<IExercise>(`/api/exercices/${id}`, {
    method: 'PUT',
    body: JSON.stringify(exerciceToSave),
    headers: adminAuthHeaders,
  });
}

export function deleteExercise(id: string) {
  return apiFetch<void>(`/api/exercices/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
}

export function reactivateExercise(id: string) {
  return apiFetch<IExercise>(`/api/exercices/${id}/reactivate`, {
    method: 'POST',
    headers: adminAuthHeaders,
  });
}

/* export function importExercisesCsv(request: IImportExercisesRequest) {
  return apiFetch<IImportExercisesResult>('/api/exercices/import-csv', {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
} */

export function importExercisesCsv(request: string) {
  const formData = new FormData();
  formData.append('file', new Blob([request], { type: 'text/csv' }), 'exercises.csv');
  return apiFetch<IImportExercisesResult>('/api/exercices/import-csv', {
    method: 'POST',
    body: formData,
    headers: adminAuthHeaders,
  });
}

/* export function requestExerciseUploadUrl(exerciseId: string, request: IRequestUploadUrlRequest) {
  return apiFetch<IMediaUploadTicket>(`/api/exercices/${exerciseId}/media/request-upload`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
}
 */
/* export function confirmExerciseMedia(exerciseId: string, request: IConfirmExerciseMediaRequest) {
  return apiFetch<IExercise>(`/api/exercices/${exerciseId}/media/confirm`, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: adminAuthHeaders,
  });
} */



