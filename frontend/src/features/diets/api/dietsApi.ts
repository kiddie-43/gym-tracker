import { apiFetch } from '../../../shared/api/httpClient';
import type { CreateDietRequest, Diet, DietSummary } from '../../../shared/types/diets';

const authHeaders = {
  Authorization: 'Bearer demo-user',
};

export function listDiets() {
  return apiFetch<DietSummary[]>('/api/diets', { headers: authHeaders });
}

export function createDiet(request: CreateDietRequest) {
  return apiFetch<Diet>('/api/diets', {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify(request),
  });
}
