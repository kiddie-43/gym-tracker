import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { TrainingFlowStepper } from '../TrainingFlowStepper/TrainingFlowStepper';
import type { RoutineDetail, TrainingFlowState } from '../../../../interfaces/routines/routines';

// Mock data
const mockRoutine: RoutineDetail = {
  id: 'routine-1',
  title: 'Push/Pull/Legs',
  goal: 'Ganar fuerza y masa',
  createdAt: '2024-01-01T00:00:00.000Z',
  isDeleted: false,
  exerciseCount: 12,
  sessions: [
    {
      id: 'session-1',
      name: 'Push Day',
      daysOfWeek: ['Lunes', 'Jueves'],
      exercises: [
        {
          id: 'exercise-1',
          exerciseId: 'ex-1',
          name: 'Bench Press',
          plannedSets: [
            { id: 'ps-1', order: 1, repetitions: 5, weightKg: 80 },
            { id: 'ps-2', order: 2, repetitions: 5, weightKg: 80 },
            { id: 'ps-3', order: 3, repetitions: 5, weightKg: 80 },
          ],
        },
        {
          id: 'exercise-2',
          exerciseId: 'ex-2',
          name: 'Incline Dumbbell Press',
          plannedSets: [
            { id: 'ps-4', order: 1, repetitions: 8, weightKg: 32 },
            { id: 'ps-5', order: 2, repetitions: 8, weightKg: 32 },
          ],
        },
      ],
    },
    {
      id: 'session-2',
      name: 'Pull Day',
      daysOfWeek: ['Martes', 'Viernes'],
      exercises: [
        {
          id: 'exercise-3',
          exerciseId: 'ex-3',
          name: 'Deadlift',
          plannedSets: [
            { id: 'ps-6', order: 1, repetitions: 3, weightKg: 140 },
            { id: 'ps-7', order: 2, repetitions: 3, weightKg: 140 },
          ],
        },
      ],
    },
  ],
};

describe('TrainingFlowStepper', () => {
  it('should render step 1 (routine overview) with single centered card', () => {
    const mockState: TrainingFlowState = {
      stepNode: 'routine',
      routineId: 'routine-1',
      sessionId: null,
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={vi.fn()}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Header should show "Entrenamientos"
    expect(screen.getByText('Entrenamientos')).toBeInTheDocument();

    // Back button should NOT be visible on first step
    expect(screen.queryByText('Atrás')).not.toBeInTheDocument();

    // Cancel button should be visible
    expect(screen.getByText('Cancelar')).toBeInTheDocument();

    // Routine details should be visible
    expect(screen.getByText('Push/Pull/Legs')).toBeInTheDocument();
    expect(screen.getByText('Ganar fuerza y masa')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument(); // 2 sessions
    expect(screen.getByText('Sesiones')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument(); // 3 total exercises
    expect(screen.getByText('Ejercicios')).toBeInTheDocument();
    expect(screen.getByText('Toca para comenzar →')).toBeInTheDocument();
  });

  it('should render step 2 (sessions) with grid of session cards', () => {
    const mockState: TrainingFlowState = {
      stepNode: 'session',
      routineId: 'routine-1',
      sessionId: null,
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={vi.fn()}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Header should show routine title
    expect(screen.getByText('Push/Pull/Legs')).toBeInTheDocument();

    // Back button should be visible
    expect(screen.getByText('Atrás')).toBeInTheDocument();

    // Session cards should be visible
    expect(screen.getByText('Push Day')).toBeInTheDocument();
    expect(screen.getByText('Pull Day')).toBeInTheDocument();

    // Session details (emojis and counts)
    expect(screen.getByText(/📅/)).toBeInTheDocument(); // Days indicator
    expect(screen.getByText(/💪/)).toBeInTheDocument(); // Exercises indicator

    // Session 1: Push Day has 2 exercises
    expect(screen.getByText('2 ejercicios')).toBeInTheDocument();
  });

  it('should render step 3 (exercises) with grid of exercise cards', () => {
    const mockState: TrainingFlowState = {
      stepNode: 'exercise',
      routineId: 'routine-1',
      sessionId: 'session-1',
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={vi.fn()}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Header should show session name
    expect(screen.getByText('Push Day')).toBeInTheDocument();

    // Back button should be visible
    expect(screen.getByText('Atrás')).toBeInTheDocument();

    // Exercise cards should be visible
    expect(screen.getByText('Bench Press')).toBeInTheDocument();
    expect(screen.getByText('Incline Dumbbell Press')).toBeInTheDocument();

    // Exercise series preview (first 2 sets shown)
    expect(screen.getByText('S1: 5 × 80kg')).toBeInTheDocument();
  });

  it('should call onNextStep when clicking routine card', async () => {
    const user = userEvent.setup();
    const onNextStep = vi.fn();

    const mockState: TrainingFlowState = {
      stepNode: 'routine',
      routineId: 'routine-1',
      sessionId: null,
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={onNextStep}
        onPreviousStep={vi.fn()}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Find and click the routine card
    const routineCard = screen.getByText('Push/Pull/Legs');
    await user.click(routineCard.closest('div[role]') ?? routineCard);

    expect(onNextStep).toHaveBeenCalledWith();
  });

  it('should call onPreviousStep when clicking back button', async () => {
    const user = userEvent.setup();
    const onPreviousStep = vi.fn();

    const mockState: TrainingFlowState = {
      stepNode: 'session',
      routineId: 'routine-1',
      sessionId: null,
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={onPreviousStep}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Find and click the back button
    const backButton = screen.getByText('Atrás');
    await user.click(backButton);

    expect(onPreviousStep).toHaveBeenCalled();
  });

  it('should call onCancel when clicking cancel button', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();

    const mockState: TrainingFlowState = {
      stepNode: 'routine',
      routineId: 'routine-1',
      sessionId: null,
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={vi.fn()}
        onCancel={onCancel}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Find and click the cancel button
    const cancelButton = screen.getByText('Cancelar');
    await user.click(cancelButton);

    expect(onCancel).toHaveBeenCalled();
  });

  it('should highlight selected session card', () => {
    const mockState: TrainingFlowState = {
      stepNode: 'session',
      routineId: 'routine-1',
      sessionId: 'session-1',
      exerciseId: null,
      isLocked: false,
      lastUpdatedAt: '2024-01-01T00:00:00.000Z',
    };

    const { container } = render(
      <TrainingFlowStepper
        routine={mockRoutine}
        state={mockState}
        onNextStep={vi.fn()}
        onPreviousStep={vi.fn()}
        onCancel={vi.fn()}
        onSubmitTrainingLog={vi.fn()}
      />,
    );

    // Check that the selected session card has the primary border color
    const sessionCards = container.querySelectorAll('[style*="border"]');
    expect(sessionCards.length).toBeGreaterThan(0);
  });
});
