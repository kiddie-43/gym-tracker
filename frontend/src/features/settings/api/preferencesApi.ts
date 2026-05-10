import { apiFetch } from '../../../shared/api/httpClient';
import type { UserPreferences } from '../../../shared/types/settings';

const authHeaders = {
  Authorization: 'Bearer demo-user',
};

export function getPreferences() {
  return apiFetch<UserPreferences>('/api/settings/preferences', { headers: authHeaders });
}

export function updatePreferences(request: UserPreferences) {
  return apiFetch<UserPreferences>('/api/settings/preferences', {
    method: 'PUT',
    headers: authHeaders,
    body: JSON.stringify(request),
  });
}
