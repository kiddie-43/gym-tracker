import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { RoutineSessionExercisesSection } from '../../../src/features/routines/components/RoutineSessionExercisesSection';

describe('Session exercises catalog', () => {
  it('adds from catalog and unlinks exercise from card', async () => {
    const user = userEvent.setup();
    const onAddExercise = vi.fn();
    const onUnlinkExercise = vi.fn();

    render(
      <RoutineSessionExercisesSection
        routineId="r1"
        session={{
          id: 's1',
          name: 'Dia A',
          daysOfWeek: ['monday'],
          exercises: [
            {
              id: 'se1',
              exerciseId: 'exercise-1',
              name: 'Press banca',
              plannedSets: [{ id: 'ps1', repetitions: 8, weightKg: 20, order: 1 }],
            },
          ],
        }}
        onAddExercise={onAddExercise}
        onUnlinkExercise={onUnlinkExercise}
        onUpdateSet={vi.fn()}
        onDeleteSet={vi.fn()}
      />,
    );

    await user.click(screen.getByRole('button', { name: /agregar ejercicio/i }));
    expect(onAddExercise).toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: /quitar ejercicio/i }));
    expect(onUnlinkExercise).toHaveBeenCalledWith('r1', 's1', 'se1');
  });
});
