import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { RoutineBuilderPage } from '../../../src/features/routines/pages/RoutineBuilderPage';

const apiFetchMock = vi.fn();

vi.mock('../../../src/shared/api/httpClient', () => ({
  apiFetch: (...args: unknown[]) => apiFetchMock(...args),
}));

describe('RoutineBuilderPage', () => {
  beforeEach(() => {
    apiFetchMock.mockReset();
  });

  it('loads muscle groups and submits routine request', async () => {
    const user = userEvent.setup();

    apiFetchMock.mockImplementation(async (path: string) => {
      if (path.startsWith('/api/catalog/muscle-groups')) {
        return [{ id: 'chest', name: 'Chest' }];
      }

      if (path === '/api/routines') {
        return { id: 'routine-1', name: 'Push split', tags: ['template'], days: [] };
      }

      return [];
    });

    render(<RoutineBuilderPage />);

    await waitFor(() => expect(apiFetchMock).toHaveBeenCalledWith('/api/catalog/muscle-groups', expect.anything()));

    await user.click(screen.getByRole('button', { name: /save routine/i }));

    await waitFor(() => expect(apiFetchMock).toHaveBeenCalledWith('/api/routines', expect.objectContaining({ method: 'POST' })));
    expect(screen.getByText(/routine saved/i)).toBeInTheDocument();
  });
});
