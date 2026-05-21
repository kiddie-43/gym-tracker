import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { MealLogPage } from '../../../src/features/meals/pages/MealLogPage';

const apiFetchMock = vi.fn();

vi.mock('../../../src/shared/api/httpClient', () => ({
  apiFetch: (...args: unknown[]) => apiFetchMock(...args),
}));

describe('MealLogPage', () => {
  it('creates meal log and shows known calories badge', async () => {
    const user = userEvent.setup();

    apiFetchMock.mockResolvedValue({
      id: 'meal-1',
      loggedDate: '2026-05-05',
      slotType: 'breakfast',
      items: [],
      totalCalories: 300,
    });

    render(<MealLogPage />);

    await user.click(screen.getByRole('button', { name: /guardar registro de comida/i }));

    await waitFor(() => expect(apiFetchMock).toHaveBeenCalledWith('/api/meals/logs', expect.objectContaining({ method: 'POST' })));
    expect(screen.getByText(/registro de comida guardado/i)).toBeInTheDocument();
    expect(screen.getByText(/calorias 300/i)).toBeInTheDocument();
  });
});
