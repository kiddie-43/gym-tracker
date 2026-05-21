import { render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';

import { RoutinesPage } from '../../../src/pages/routines/RoutinesPage';
import { store } from '../../../src/redux/store';

vi.mock('../../../src/redux/actions/routines/routinesActions', async () => {
  const actual = await vi.importActual<typeof import('../../../src/redux/actions/routines/routinesActions')>(
    '../../../src/redux/actions/routines/routinesActions',
  );

  return {
    ...actual,
    fetchRoutines: () => ({ type: 'tests/fetchRoutines' }),
    restoreTrainingFlowAction: () => ({ type: 'tests/restoreTrainingFlow' }),
    fetchCatalogAvailabilityAction: () => ({ type: 'tests/fetchCatalogAvailability' }),
  };
});

describe('RoutinesPage cards', () => {
  it('renders page title', () => {
    render(
      <Provider store={store}>
        <MemoryRouter>
          <RoutinesPage />
        </MemoryRouter>
      </Provider>,
    );

    expect(screen.getByRole('heading', { name: /^rutinas$/i })).toBeInTheDocument();
  });
});
