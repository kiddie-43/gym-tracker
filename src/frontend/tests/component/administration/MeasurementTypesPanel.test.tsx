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

vi.mock('../../../src/services/api/admin/measurementTypes/measurementTypesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/measurementTypes/measurementTypesApi');

  return {
    ...actual,
    listMeasurementTypesPage: (...args: unknown[]) => listMeasurementTypesPageMock(...args),
    createMeasurementType: (...args: unknown[]) => createMeasurementTypeMock(...args),
    updateMeasurementType: (...args: unknown[]) => updateMeasurementTypeMock(...args),
    deleteMeasurementType: (...args: unknown[]) => deleteMeasurementTypeMock(...args),
    reactivateMeasurementType: (...args: unknown[]) => reactivateMeasurementTypeMock(...args),
  };
});

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <MeasurementTypesPanel />
    </I18nextProvider>,
  );
}

describe('MeasurementTypesPanel', () => {
  beforeEach(() => {
    listMeasurementTypesPageMock.mockReset();
    createMeasurementTypeMock.mockReset();
    updateMeasurementTypeMock.mockReset();
    deleteMeasurementTypeMock.mockReset();
    reactivateMeasurementTypeMock.mockReset();

    listMeasurementTypesPageMock.mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 10 });
    createMeasurementTypeMock.mockResolvedValue({
      id: 'mt-1',
      key: 'PESO_REPS',
      name: 'Peso/Reps',
      unit: 'kg',
      dataType: 'integer',
      category: 'general',
      description: null,
      active: true,
      isDeleted: false,
    });
  });

  it('creates a measurement type from modal form fields', async () => {
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

    await user.click(screen.getByRole('button', { name: 'Añadir' }));
    await user.type(screen.getByLabelText('Nombre'), 'Kilos');
    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    await waitFor(() =>
      expect(createMeasurementTypeMock).toHaveBeenCalledWith({
        name: 'Kilos',
        category: 'general',
        description: null,
      }),
    );
  });
});
