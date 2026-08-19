import type {
  ILinkMonthlyPlanExerciseRequest,
  ILinkMonthlyPlanExerciseResponse,
  IMonthlyPlan,
  IUpdatePlannedExerciseRequest,
  IUpsertMonthlyPlanRequest,
} from '../../../interfaces/monthlyPlan/IMonthlyPlan';
import { apiFetch } from '../../httpClient';

export interface ITruncateDaysRequest {
  activeDays: number;
  confirmed: boolean;
}

export function getMyMonthlyPlan() {
  return apiFetch<IMonthlyPlan>('/api/monthly-plans/mine');
}

export function upsertMyMonthlyPlan(request: IUpsertMonthlyPlanRequest) {
  return apiFetch<IMonthlyPlan>('/api/monthly-plans/mine', {
    method: 'PUT',
    body: JSON.stringify(request),
  });
}

export function truncateMonthlyPlanDays(request: ITruncateDaysRequest) {
  return apiFetch<IMonthlyPlan>('/api/monthly-plans/mine/truncate-days', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function linkExerciseToMyDay(request: ILinkMonthlyPlanExerciseRequest) {
  return apiFetch<ILinkMonthlyPlanExerciseResponse>('/api/monthly-plans/mine/exercises', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function updatePlannedExercise(id: string, request: IUpdatePlannedExerciseRequest) {
  return apiFetch<void>(`/api/monthly-plans/mine/exercises/${id}`, {
    method: 'PATCH',
    body: JSON.stringify(request),
  });
}

export function unlinkPlannedExercise(id: string) {
  return apiFetch<void>(`/api/monthly-plans/mine/exercises/${id}`, {
    method: 'DELETE',
  });
}

