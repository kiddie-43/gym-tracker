import { apiFetch } from '../../../shared/api/httpClient';
import type { CreateMealLogRequest, MealLog } from '../../../shared/types/meals';

const authHeaders = {
  Authorization: 'Bearer demo-user',
};

export function listMealLogs(loggedDate: string) {
  return apiFetch<MealLog[]>(`/api/meals/logs?date=${encodeURIComponent(loggedDate)}`, { headers: authHeaders });
}

export function createMealLog(request: CreateMealLogRequest) {
  return apiFetch<MealLog>('/api/meals/logs', {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify(request),
  });
}

export function getMealHistory(page = 1, pageSize = 20) {
  const params = new URLSearchParams();
  params.set('page', String(page));
  params.set('pageSize', String(pageSize));

  return apiFetch<{ items: Array<{ id: string; loggedDate: string; slotType: string; itemCount: number }>; totalCount: number; page: number; pageSize: number }>(
    `/api/meals/history?${params.toString()}`,
    { headers: authHeaders },
  );
}
