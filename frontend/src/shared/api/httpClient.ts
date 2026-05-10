import { i18n } from '../../i18n/i18n';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5092';

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const language = i18n.resolvedLanguage ?? i18n.language ?? 'es';

  const response = await fetch(new URL(path, API_BASE_URL), {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      'Accept-Language': language,
      ...init?.headers,
    },
  });

  if (!response.ok) {
    const error = new Error(`Request failed with status ${response.status}`);
    try {
      const errorData = await response.json() as { detail?: string; message?: string };
      throw new Error(errorData.detail || errorData.message || error.message);
    } catch {
      throw error;
    }
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
