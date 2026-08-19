import {
  IBlockTemplateItem,
  ICreateTrainingSessionRequest,
  ITrainingSession,
} from '../../../interfaces/trainingSessions/trainingSessions';
import { apiFetch } from '../../httpClient';

export function createTrainingSession(request: ICreateTrainingSessionRequest) {
  return apiFetch<ITrainingSession>('/api/trainingsessions', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function getTrainingSessionById(id: string) {
  return apiFetch<ITrainingSession>(`/api/trainingsessions/${id}`);
}

export function getTrainingSessionHistory(
  weekNumber: number,
  dayNumber: number,
  exerciseId: string,
) {
  return apiFetch<ITrainingSession[]>(
    `/api/trainingsessions/week/${weekNumber}/day/${dayNumber}/exercise/${exerciseId}/history`,
  );
}

export function getBlockTemplates(exerciseType: string) {
  return apiFetch<IBlockTemplateItem[]>(`/api/trainingsessions/block-templates/${exerciseType}`);
}
