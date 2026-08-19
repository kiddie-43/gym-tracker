import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { MeasurementTypesCsvImportDialog } from '../../../src/pages/administration/measurement-types/MeasurementTypesCsvImportDialog';
import { i18n } from '../../../src/i18n/i18n';

function renderDialog(onImport: (rows: Array<Record<string, string | null>>) => Promise<void>) {
  return render(
    <I18nextProvider i18n={i18n}>
      <MeasurementTypesCsvImportDialog
        open
        loading={false}
        error={null}
        result={null}
        onClose={() => {}}
        onImport={onImport}
      />
    </I18nextProvider>,
  );
}

describe('MeasurementTypesCsvImportDialog', () => {
  it('parses CSV and sends rows to import callback', async () => {
    const user = userEvent.setup();
    const importMock = vi.fn().mockResolvedValue(undefined);

    renderDialog(importMock);

    const csv = [
      'code,name,description',
      'WEIGHT,Peso,Carga',
      'DURATION,Duracion,Tiempo total',
    ].join('\n');

    const file = new File([csv], 'measurement-types.csv', { type: 'text/csv' });
    Object.defineProperty(file, 'text', {
      value: vi.fn().mockResolvedValue(csv),
    });

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText('measurement-types.csv')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Procesar importación' }));

    await waitFor(() => {
      expect(importMock).toHaveBeenCalledTimes(1);
      expect(importMock).toHaveBeenCalledWith([
        {
          code: 'WEIGHT',
          name: 'Peso',
          description: 'Carga',
        },
        {
          code: 'DURATION',
          name: 'Duracion',
          description: 'Tiempo total',
        },
      ]);
    });
  });

  it('renders import results by row', async () => {
    render(
      <I18nextProvider i18n={i18n}>
        <MeasurementTypesCsvImportDialog
          open
          loading={false}
          error={null}
          onClose={() => {}}
          onImport={vi.fn().mockResolvedValue(undefined)}
          result={{
            totalRows: 2,
            createdRows: 1,
            rejectedRows: 1,
            rows: [
              { rowNumber: 1, key: 'WEIGHT', created: true, reason: null, measurementType: null },
              { rowNumber: 2, key: 'BAD', created: false, reason: 'dataType inválido', measurementType: null },
            ],
          }}
        />
      </I18nextProvider>,
    );

    expect(screen.getByText('Total: 2 | Creadas: 1 | Rechazadas: 1')).toBeInTheDocument();
    expect(screen.getByText('Creada')).toBeInTheDocument();
    expect(screen.getByText('Rechazada')).toBeInTheDocument();
    expect(screen.getByText('dataType inválido')).toBeInTheDocument();
  });
});
