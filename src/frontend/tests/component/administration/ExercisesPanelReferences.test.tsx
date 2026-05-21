import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { ExercisesPanel } from '../../../src/components/administration/ExercisesPanel';
import { i18n } from '../../../src/i18n/i18n';

const listExercisesPageMock = vi.fn();
const listMusclesPageMock = vi.fn();
const listAssignableMeasurementTypesMock = vi.fn();

vi.mock('../../../src/services/api/admin/exercises/exercisesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/exercises/exercisesApi');

  return {
    ...actual,
    listExercisesPage: (...args: unknown[]) => listExercisesPageMock(...args),
  };
});

vi.mock('../../../src/services/api/admin/muscles/musclesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/muscles/musclesApi');

  return {
    ...actual,
    listMusclesPage: (...args: unknown[]) => listMusclesPageMock(...args),
  };
});

vi.mock('../../../src/services/api/admin/measurementTypes/measurementTypesApi', async () => {
  const actual = await vi.importActual<object>('../../../src/services/api/admin/measurementTypes/measurementTypesApi');

  return {
    ...actual,
    listAssignableMeasurementTypes: (...args: unknown[]) => listAssignableMeasurementTypesMock(...args),
  };
});

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <ExercisesPanel />
    </I18nextProvider>,
  );
}

describe('ExercisesPanel references', () => {
  beforeEach(() => {
    listExercisesPageMock.mockReset();
    listMusclesPageMock.mockReset();
    listAssignableMeasurementTypesMock.mockReset();

    listExercisesPageMock.mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 10 });
    listMusclesPageMock.mockResolvedValue({
      items: [
        { id: 'm1', name: 'Biceps', code: 'BICEPS', description: null, active: true, isDeleted: false, muscleGroupIds: [] },
        { id: 'm2', name: 'Deleted muscle', code: 'DEL', description: null, active: false, isDeleted: true, muscleGroupIds: [] },
      ],
      totalCount: 2,
      page: 1,
      pageSize: 500,
    });
    listAssignableMeasurementTypesMock.mockResolvedValue([
      { id: 'mt1', key: 'WEIGHT_REPS', name: 'Peso', unit: 'kg', dataType: 'decimal', category: 'strength' },
    ]);
  });

  it('excludes deleted muscles and measurement types from form selectors', async () => {
    const user = userEvent.setup();
    renderWithI18n();

    await waitFor(() =>
      expect(listExercisesPageMock).toHaveBeenCalledWith(
        expect.objectContaining({ includeDeleted: false, page: 1, pageSize: 10 }),
      ));

    await user.click(screen.getByRole('button', { name: 'Añadir' }));

    fireEvent.mouseDown(screen.getByLabelText('Tipo de medición'));
    expect(await screen.findByRole('option', { name: 'Peso' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Deleted type' })).not.toBeInTheDocument();

    fireEvent.mouseDown(screen.getByLabelText('Músculo principal'));
    expect(await screen.findByRole('option', { name: 'Biceps' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Deleted muscle' })).not.toBeInTheDocument();
  });
});
