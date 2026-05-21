import { fireEvent, render, screen, waitFor } from '@testing-library/react';
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

describe('Muscles CSV import', () => {
  beforeEach(() => {
    listMusclesMock.mockReset();
    createMuscleMock.mockReset();
    updateMuscleMock.mockReset();
    deleteMuscleMock.mockReset();
    reactivateMuscleMock.mockReset();
    importMusclesCsvMock.mockReset();

    listMusclesMock.mockResolvedValue([]);
  });

  it('shows partial import summary and row-level results', async () => {
    const user = userEvent.setup();

    importMusclesCsvMock.mockResolvedValue({
      totalRows: 2,
      importedRows: 1,
      rejectedRows: 1,
      results: [
        { rowNumber: 1, code: 'BICEPS', imported: true },
        { rowNumber: 2, code: 'BICEPS', imported: false, reason: 'Duplicate code' },
      ],
    });

    renderWithI18n();

    await waitFor(() => expect(listMusclesMock).toHaveBeenCalled());

    await user.click(screen.getByRole('button', { name: 'Importar CSV' }));
    fireEvent.change(screen.getByLabelText('Contenido CSV'), {
      target: {
        value: 'code,description\nBICEPS,Brazo\nBICEPS,Duplicado',
      },
    });
    await user.click(screen.getByRole('button', { name: 'Procesar importación' }));

    await waitFor(() => expect(importMusclesCsvMock).toHaveBeenCalled());
    expect(screen.getByText('Total: 2 | Importadas: 1 | Rechazadas: 1')).toBeInTheDocument();
    expect(screen.getByText('Duplicate code')).toBeInTheDocument();
  });
});
