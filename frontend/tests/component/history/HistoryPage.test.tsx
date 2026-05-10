import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import { WorkoutHistoryPage } from '../../../src/features/workouts/pages/WorkoutHistoryPage';

const apiFetchMock = vi.fn();

vi.mock('../../../src/shared/api/httpClient', () => ({
  apiFetch: (...args: unknown[]) => apiFetchMock(...args),
}));

describe('WorkoutHistoryPage', () => {
  it('renders workout history header', () => {
    apiFetchMock.mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 });
    render(<WorkoutHistoryPage />);
    expect(screen.getByText(/workout history/i)).toBeInTheDocument();
  });
});
