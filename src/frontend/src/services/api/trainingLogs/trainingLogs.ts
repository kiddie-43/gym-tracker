import { ITrainingMetricLog } from '../../../interfaces/ITrainingMetricLogs/ITrainingMetricLog';
import { apiFetch } from '../../httpClient';


export function getTrainingLogsByExercise(
  weekNumber: number,
  dayNumber: number,
  exerciseCode: string,
) {
  return apiFetch<ITrainingMetricLog[]>(
    `/api/traininglogs/week/${weekNumber}/day/${dayNumber}/exercise/${exerciseCode}/logs`,
  );
}

export const listTrainingLogsByExercise = getTrainingLogsByExercise;

export function createTrainingLogGroup(request: ITrainingMetricLog) {
  return apiFetch<ITrainingMetricLog>('/api/traininglogs', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function updateTrainingLogGroup(groupId: string, request: ITrainingMetricLog) {
  return apiFetch<ITrainingMetricLog>(`/api/traininglogs/${groupId}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  });
}

export function deleteTrainingLogGroup(groupId: string) {
  return apiFetch<void>(`/api/traininglogs/${groupId}`, {
    method: 'DELETE',
  });
}
