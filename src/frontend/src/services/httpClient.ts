import { i18n } from '../i18n/i18n';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5092';
const DEV_AUTH_TOKEN = import.meta.env.VITE_DEV_AUTH_TOKEN ?? 'integration-demo-user';

export class ApiHttpError extends Error {
  readonly status: number;
  readonly detail?: string;

  constructor(status: number, message: string, detail?: string) {
    super(message);
    this.name = 'ApiHttpError';
    this.status = status;
    this.detail = detail;
  }
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const language = i18n.resolvedLanguage ?? i18n.language ?? 'es';
  const hasAuthorizationHeader = hasHeader(init?.headers, 'Authorization');

  const response = await fetch(new URL(path, API_BASE_URL), {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      'Accept-Language': language,
      ...(import.meta.env.DEV && !hasAuthorizationHeader
        ? { Authorization: `Bearer ${DEV_AUTH_TOKEN}` }
        : {}),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    let detail: string | undefined;

    try {
      const errorData = await response.json() as { detail?: string; message?: string };
      detail = errorData.detail || errorData.message;
    } catch {
      // Keep fallback message when body is not JSON.
    }

    throw new ApiHttpError(
      response.status,
      detail || `Request failed with status ${response.status}`,
      detail,
    );
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function hasHeader(headers: HeadersInit | undefined, headerName: string): boolean {
  if (!headers) {
    return false;
  }

  const normalizedHeaderName = headerName.toLowerCase();

  if (headers instanceof Headers) {
    return headers.has(headerName);
  }

  if (Array.isArray(headers)) {
    return headers.some(([name]) => name.toLowerCase() === normalizedHeaderName);
  }

  return Object.keys(headers).some((name) => name.toLowerCase() === normalizedHeaderName);
}
