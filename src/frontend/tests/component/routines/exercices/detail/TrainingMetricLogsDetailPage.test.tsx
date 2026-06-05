import { fireEvent, render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { PopUpCode } from '../../../../../src/enums/popUp/popUp';
import { TrainingMetricLogsDetailPage } from '../../../../../src/pages/routines/exercices/detail/TrainingMetricLogsDetailPage';

const dispatchMock = vi.fn();
let mockedState: {
  trainingMetricLogs: {
    table: {
      list: Array<{
        logId: string;
        userId: string;
        routineId: string;
        sessionId: string;
        trainingId: string;
        exerciseId: string;
        metricId: string;
        groupId: string;
        date: string;
        value: number;
        createdAt: string;
        updatedAt: string;
      }>;
      page: number;
      rowsPerPage: number;
      totalCount: number;
      sortBy: 'createdAt';
      sortDirection: 'desc';
      selectedIds: string[];
    };
    filters: {
      routineId: string;
      sessionId: string;
      trainingId: string;
      exerciseId: string;
      metricId: string;
    };
    form: {
      routineId: string;
      sessionId: string;
      trainingId: string;
      exerciseId: string;
      date: string;
      metrics: Array<{ metricId: string; value: number }>;
    };
    metrics: Array<{ metricId: string; name: string; dataType: string; unit?: string | null }>;
    metricsDegraded: boolean;
    error: string | null;
    loading: boolean;
    popUpCode: PopUpCode;
    message: string | null;
  };
};

vi.mock('../../../../../src/redux/hooks', () => ({
  useAppDispatch: () => dispatchMock,
  useAppSelector: (selector: (state: typeof mockedState) => unknown) => selector(mockedState),
}));

vi.mock('../../../../../src/redux/trainingMetricLogs/thunks', () => ({
  fetchAvailableTrainingMetricsAction: vi.fn(() => ({ type: 'thunk/fetchMetrics' })),
  fetchCurrentDayTrainingMetricLogsAction: vi.fn(() => ({ type: 'thunk/fetchCurrentDay' })),
  createTrainingMetricGroupAction: vi.fn(() => ({ type: 'thunk/create' })),
  updateTrainingMetricValueAction: vi.fn(() => ({ type: 'thunk/update' })),
  deleteTrainingMetricGroupAction: vi.fn(() => ({ type: 'thunk/delete' })),
}));

function buildState(overrides?: Partial<typeof mockedState.trainingMetricLogs>): typeof mockedState {
  return {
    trainingMetricLogs: {
      table: {
        list: [],
        page: 0,
        rowsPerPage: 20,
        totalCount: 0,
        sortBy: 'createdAt',
        sortDirection: 'desc',
        selectedIds: [],
      },
      filters: {
        routineId: 'r-1',
        sessionId: 's-1',
        trainingId: 't-1',
        exerciseId: 'e-1',
        metricId: '',
      },
      form: {
        routineId: '',
        sessionId: '',
        trainingId: '',
        exerciseId: '',
        date: '',
        metrics: [],
      },
      metrics: [{ metricId: 'm-1', name: 'Peso', dataType: 'number', unit: 'kg' }],
      metricsDegraded: false,
      error: null,
      loading: false,
      popUpCode: PopUpCode.Default,
      message: null,
      ...overrides,
    },
  };
}

describe('TrainingMetricLogsDetailPage', () => {
  beforeEach(() => {
    dispatchMock.mockReset();
  });

  it('shows FeedbackMessage for error state', () => {
    mockedState = buildState({ error: 'error de carga' });

    render(
      <TrainingMetricLogsDetailPage
        routineId="r-1"
        sessionId="s-1"
        trainingId="t-1"
        exerciseId="e-1"
      />,
    );

    expect(screen.getAllByText('error de carga').length).toBeGreaterThan(0);
  });

  it('shows FeedbackMessage for empty state', () => {
    mockedState = buildState();

    render(
      <TrainingMetricLogsDetailPage
        routineId="r-1"
        sessionId="s-1"
        trainingId="t-1"
        exerciseId="e-1"
      />,
    );

    expect(screen.getByText('No hay logs para el dia actual.')).toBeInTheDocument();
  });

  it('dispatches popup action from ActionMenu edit flow', async () => {
    mockedState = buildState({
      table: {
        list: [
          {
            logId: 'log-1',
            userId: 'user-1',
            routineId: 'r-1',
            sessionId: 's-1',
            trainingId: 't-1',
            exerciseId: 'e-1',
            metricId: 'm-1',
            groupId: 'g-1',
            date: '2026-05-29',
            value: 55,
            createdAt: '2026-05-29T12:00:00Z',
            updatedAt: '2026-05-29T12:00:00Z',
          },
        ],
        page: 0,
        rowsPerPage: 20,
        totalCount: 1,
        sortBy: 'createdAt',
        sortDirection: 'desc',
        selectedIds: [],
      },
    });

    render(
      <TrainingMetricLogsDetailPage
        routineId="r-1"
        sessionId="s-1"
        trainingId="t-1"
        exerciseId="e-1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /Acciones del log log-1/i }));
    fireEvent.click(await screen.findByRole('menuitem', { name: 'Editar' }));

    expect(dispatchMock).toHaveBeenCalledWith(
      expect.objectContaining({
        type: 'trainingMetricLogs/setPopUpCode',
        payload: PopUpCode.Update,
      }),
    );
  });

  it('opens create popup and submits action', () => {
    mockedState = buildState({ popUpCode: PopUpCode.Create });

    render(
      <TrainingMetricLogsDetailPage
        routineId="r-1"
        sessionId="s-1"
        trainingId="t-1"
        exerciseId="e-1"
      />,
    );

    fireEvent.change(screen.getByLabelText('Valor'), { target: { value: '80' } });
    fireEvent.click(screen.getByRole('button', { name: 'Guardar' }));

    expect(dispatchMock).toHaveBeenCalledWith(
      expect.objectContaining({
        type: 'thunk/create',
      }),
    );
  });
});
