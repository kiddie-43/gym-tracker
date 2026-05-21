import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { i18n } from '../../../src/i18n/i18n';
import { MusclesPanel } from '../../../src/components/administration/MusclesPanel';

const listMusclesMock = vi.fn();
const createMuscleMock = vi.fn();
const updateMuscleMock = vi.fn();
const deleteMuscleMock = vi.fn();
const reactivateMuscleMock = vi.fn();
const importMusclesCsvMock = vi.fn();

vi.mock('../../../src/services/api/admin/muscles/musclesApi', () => ({
  listMuscles: (...args: unknown[]) => listMusclesMock(...args),
  createMuscle: (...args: unknown[]) => createMuscleMock(...args),
  updateMuscle: (...args: unknown[]) => updateMuscleMock(...args),
  deleteMuscle: (...args: unknown[]) => deleteMuscleMock(...args),
  reactivateMuscle: (...args: unknown[]) => reactivateMuscleMock(...args),
  importMusclesCsv: (...args: unknown[]) => importMusclesCsvMock(...args),
}));

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <MusclesPanel />
    </I18nextProvider>,
  );
}

describe('MusclesPanel states', () => {
  beforeEach(() => {
    listMusclesMock.mockReset();
    createMuscleMock.mockReset();
    updateMuscleMock.mockReset();
    deleteMuscleMock.mockReset();
    reactivateMuscleMock.mockReset();
    importMusclesCsvMock.mockReset();
  });

  it('shows loading and then list state', async () => {
    listMusclesMock.mockResolvedValueOnce([
      {
        id: 'm1',
        name: 'Biceps',
        code: 'BICEPS',
        description: 'Brazo',
        active: true,
        isDeleted: false,
        muscleGroupIds: ['general'],
      },
    ]);

    renderWithI18n();

    expect(screen.getByLabelText('Cargando datos')).toBeInTheDocument();
    await waitFor(() => expect(screen.getByText('Biceps')).toBeInTheDocument());
  });

  it('shows empty state when no rows are returned', async () => {
    listMusclesMock.mockResolvedValueOnce([]);

    renderWithI18n();

    await waitFor(() => expect(screen.getByText('No hay datos')).toBeInTheDocument());
  });

  it('shows error state and retries', async () => {
    const user = userEvent.setup();

    listMusclesMock
      .mockRejectedValueOnce(new Error('Fallo remoto'))
      .mockResolvedValueOnce([]);

    renderWithI18n();

    await waitFor(() => expect(screen.getByText('Fallo remoto')).toBeInTheDocument());
    await user.click(screen.getByRole('button', { name: 'Reintentar' }));

    await waitFor(() => expect(listMusclesMock).toHaveBeenCalledTimes(2));
  });

  it('toggles includeDeleted and reloads list', async () => {
    const user = userEvent.setup();

    listMusclesMock
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([]);

    renderWithI18n();

    await waitFor(() => expect(listMusclesMock).toHaveBeenCalledWith(false));
    await user.click(screen.getByRole('checkbox', { name: 'Ver borrados' }));
    await waitFor(() => expect(listMusclesMock).toHaveBeenCalledWith(true));
  });
});
