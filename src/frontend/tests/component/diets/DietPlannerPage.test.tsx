import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { DietPlannerPage } from '../../../src/features/diets/pages/DietPlannerPage';

const apiFetchMock = vi.fn();

vi.mock('../../../src/shared/api/httpClient', () => ({
  apiFetch: (...args: unknown[]) => apiFetchMock(...args),
}));

describe('DietPlannerPage', () => {
  it('submits diet creation', async () => {
    const user = userEvent.setup();

    apiFetchMock.mockResolvedValue({ id: 'diet-1', name: 'Lean week', days: [] });

    render(<DietPlannerPage />);

    await user.click(screen.getByRole('button', { name: /guardar dieta/i }));

    await waitFor(() => expect(apiFetchMock).toHaveBeenCalledWith('/api/diets', expect.objectContaining({ method: 'POST' })));
    expect(screen.getByText(/dieta guardada/i)).toBeInTheDocument();
  });
});
