import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { MeasurementTypesPanel } from '../../../src/components/administration/MeasurementTypesPanel';
import { i18n } from '../../../src/i18n/i18n';
import { ApiHttpError } from '../../../src/shared/api/httpClient';

const listMeasurementTypesPageMock = vi.fn();

vi.mock('../../../src/services/api/admin/measurementTypes/measurementTypesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/measurementTypes/measurementTypesApi');

  return {
    ...actual,
    listMeasurementTypesPage: (...args: unknown[]) => listMeasurementTypesPageMock(...args),
  };
});

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <MeasurementTypesPanel />
    </I18nextProvider>,
  );
}

describe('MeasurementTypesPanel states', () => {
  it('shows empty state when no rows are returned', async () => {
    listMeasurementTypesPageMock.mockResolvedValueOnce({ items: [], totalCount: 0, page: 1, pageSize: 10 });

    renderWithI18n();

    expect(screen.getByLabelText('Cargando datos')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('No hay datos')).toBeInTheDocument();
    });
  });

  it('shows error state and allows retry', async () => {
    const user = userEvent.setup();
    listMeasurementTypesPageMock.mockRejectedValueOnce(new Error('Error al cargar'));
    listMeasurementTypesPageMock.mockResolvedValueOnce({ items: [], totalCount: 0, page: 1, pageSize: 10 });

    renderWithI18n();

    await waitFor(() => {
      expect(screen.getByText('Error al cargar')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Reintentar' }));

    await waitFor(() => {
      expect(listMeasurementTypesPageMock.mock.calls.length).toBeGreaterThanOrEqual(2);
    });
  });

  it('shows friendly message when backend returns 503', async () => {
    listMeasurementTypesPageMock.mockRejectedValueOnce(
      new ApiHttpError(503, 'Service temporarily unavailable'),
    );

    renderWithI18n();

    await waitFor(() => {
      expect(screen.getByText('Servicio temporalmente saturado. Intenta de nuevo en unos minutos.')).toBeInTheDocument();
    });
  });
});
