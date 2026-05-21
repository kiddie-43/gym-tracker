import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { MeasurementTypesPanel } from '../../../src/components/administration/MeasurementTypesPanel';
import { i18n } from '../../../src/i18n/i18n';

const listMeasurementTypesPageMock = vi.fn();
const createMeasurementTypeMock = vi.fn();
const updateMeasurementTypeMock = vi.fn();
const deleteMeasurementTypeMock = vi.fn();
const reactivateMeasurementTypeMock = vi.fn();
const importMeasurementTypesCsvMock = vi.fn();

vi.mock('../../../src/services/api/admin/measurementTypes/measurementTypesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/measurementTypes/measurementTypesApi');

  return {
    ...actual,
    listMeasurementTypesPage: (...args: unknown[]) => listMeasurementTypesPageMock(...args),
    createMeasurementType: (...args: unknown[]) => createMeasurementTypeMock(...args),
    updateMeasurementType: (...args: unknown[]) => updateMeasurementTypeMock(...args),
    deleteMeasurementType: (...args: unknown[]) => deleteMeasurementTypeMock(...args),
    reactivateMeasurementType: (...args: unknown[]) => reactivateMeasurementTypeMock(...args),
    importMeasurementTypesCsv: (...args: unknown[]) => importMeasurementTypesCsvMock(...args),
  };
});

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <MeasurementTypesPanel />
    </I18nextProvider>,
  );
}

describe('MeasurementTypesPanel popup flow', () => {
  beforeEach(() => {
    listMeasurementTypesPageMock.mockReset();
    createMeasurementTypeMock.mockReset();
    updateMeasurementTypeMock.mockReset();
    deleteMeasurementTypeMock.mockReset();
    reactivateMeasurementTypeMock.mockReset();
    importMeasurementTypesCsvMock.mockReset();

    listMeasurementTypesPageMock.mockResolvedValue({
      items: [
        {
          id: 'mt-1',
          key: 'WEIGHT',
          name: 'Peso',
          unit: 'kg',
          dataType: 'decimal',
          category: 'strength',
          description: 'Carga',
          active: true,
          isDeleted: false,
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 10,
    });
    createMeasurementTypeMock.mockResolvedValue(undefined);
    updateMeasurementTypeMock.mockResolvedValue(undefined);
  });

  it('opens edit popup and updates a measurement type', async () => {
    const user = userEvent.setup();

    renderWithI18n();

    await waitFor(() =>
      expect(listMeasurementTypesPageMock).toHaveBeenCalledWith({
        includeInactive: false,
        search: '',
        code: '',
        sortBy: 'name',
        sortDirection: 'asc',
        page: 1,
        pageSize: 10,
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Más acciones' }));
    await user.click(screen.getByRole('menuitem', { name: 'Editar' }));

    const nameInput = screen.getByLabelText('Nombre');
    await user.clear(nameInput);
    await user.type(nameInput, 'Peso actualizado');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    await waitFor(() => {
      expect(updateMeasurementTypeMock).toHaveBeenCalledWith('mt-1', {
        name: 'Peso actualizado',
        category: 'strength',
        description: 'Carga',
      });
    });
  });
});
