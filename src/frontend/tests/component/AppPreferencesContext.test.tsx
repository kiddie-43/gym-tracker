import { renderHook, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

import { AppPreferencesProvider, useAppPreferences } from '../../src/context/AppPreferencesContext';

type MediaQueryCallback = (e: MediaQueryListEvent) => void;

function createMatchMediaMock(matches: boolean) {
  let listener: MediaQueryCallback | null = null;

  const mediaQueryList = {
    matches,
    addEventListener: vi.fn((_: string, cb: MediaQueryCallback) => {
      listener = cb;
    }),
    removeEventListener: vi.fn((_: string, cb: MediaQueryCallback) => {
      if (listener === cb) listener = null;
    }),
    dispatchChange: (newMatches: boolean) => {
      listener?.({ matches: newMatches } as MediaQueryListEvent);
    },
  };

  return mediaQueryList;
}

describe('AppPreferencesContext', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns "dark" when prefers-color-scheme is dark', () => {
    const mock = createMatchMediaMock(true);
    vi.spyOn(window, 'matchMedia').mockReturnValue(mock as unknown as MediaQueryList);

    const { result } = renderHook(() => useAppPreferences(), {
      wrapper: AppPreferencesProvider,
    });

    expect(result.current.themeMode).toBe('dark');
  });

  it('updates themeMode reactively when OS theme changes without reloading', () => {
    const mock = createMatchMediaMock(false);
    vi.spyOn(window, 'matchMedia').mockReturnValue(mock as unknown as MediaQueryList);

    const { result } = renderHook(() => useAppPreferences(), {
      wrapper: AppPreferencesProvider,
    });

    expect(result.current.themeMode).toBe('light');

    act(() => {
      mock.dispatchChange(true);
    });

    expect(result.current.themeMode).toBe('dark');
  });

  it('removes the matchMedia event listener when the provider unmounts', () => {
    const mock = createMatchMediaMock(false);
    vi.spyOn(window, 'matchMedia').mockReturnValue(mock as unknown as MediaQueryList);

    const { unmount } = renderHook(() => useAppPreferences(), {
      wrapper: AppPreferencesProvider,
    });

    unmount();

    expect(mock.removeEventListener).toHaveBeenCalledOnce();
  });
});
