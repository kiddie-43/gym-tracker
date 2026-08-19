import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import type { RootState } from '../../../src/redux/store';
import { RoutinesPage } from '../../../src/pages/routines/RoutinesPage';

const dispatchMock = vi.fn();
let mockedState: RootState;

vi.mock('react-redux', async () => {
  const actual = await vi.importActual<typeof import('react-redux')>('react-redux');

  return {
    ...actual,
    useDispatch: () => dispatchMock,
    useSelector: (selector: (state: RootState) => unknown) => selector(mockedState),
  };
});

describe('RoutinesPage states', () => {
  it('shows loading state', () => {
    mockedState = {
      adminMuscles: {} as RootState['adminMuscles'],
      adminMeasurementTypes: {} as RootState['adminMeasurementTypes'],
      adminExercises: {} as RootState['adminExercises'],
      workouts: {} as RootState['workouts'],
      routines: {
        list: [],
        filters: { search: '' },
        form: { title: '', goal: '' },
        error: null,
        loading: true,
        popUpCode: null,
        selectedRoutine: null,
        trainingFlowState: null,
        catalogAvailability: null,
      },
    };

    render(<RoutinesPage />);

    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('shows error state', () => {
    mockedState = {
      adminMuscles: {} as RootState['adminMuscles'],
      adminMeasurementTypes: {} as RootState['adminMeasurementTypes'],
      adminExercises: {} as RootState['adminExercises'],
      workouts: {} as RootState['workouts'],
      routines: {
        list: [],
        filters: { search: '' },
        form: { title: '', goal: '' },
        error: 'fallo',
        loading: false,
        popUpCode: null,
        selectedRoutine: null,
        trainingFlowState: null,
        catalogAvailability: null,
      },
    };

    render(<RoutinesPage />);

    expect(screen.getByText('fallo')).toBeInTheDocument();
  });

  it('shows empty state', () => {
    mockedState = {
      adminMuscles: {} as RootState['adminMuscles'],
      adminMeasurementTypes: {} as RootState['adminMeasurementTypes'],
      adminExercises: {} as RootState['adminExercises'],
      workouts: {} as RootState['workouts'],
      routines: {
        list: [],
        filters: { search: '' },
        form: { title: '', goal: '' },
        error: null,
        loading: false,
        popUpCode: null,
        selectedRoutine: null,
        trainingFlowState: null,
        catalogAvailability: null,
      },
    };

    render(<RoutinesPage />);

    expect(screen.getByText(/crear nueva rutina/i)).toBeInTheDocument();
  });
});
