import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { WorkoutEntryForm } from '../../../src/features/workouts/components/WorkoutEntryForm';

describe('WorkoutEntryForm', () => {
  it('submits a normalized workout payload', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(<WorkoutEntryForm onSubmit={onSubmit} />);

    await user.clear(screen.getByLabelText(/exercise id/i));
    await user.type(screen.getByLabelText(/exercise id/i), 'squat');
    await user.clear(screen.getByLabelText(/exercise name/i));
    await user.type(screen.getByLabelText(/exercise name/i), 'Back squat');
    await user.clear(screen.getByLabelText(/weight/i));
    await user.type(screen.getByLabelText(/weight/i), '120');
    await user.clear(screen.getByLabelText(/repetitions/i));
    await user.type(screen.getByLabelText(/repetitions/i), '5');
    await user.click(screen.getByRole('button', { name: /save workout/i }));

    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        status: 'completed',
        exerciseEntries: [
          expect.objectContaining({
            externalExerciseId: 'squat',
            exerciseName: 'Back squat',
            sets: [expect.objectContaining({ weight: 120, repetitions: 5 })],
          }),
        ],
      }),
    );
  });
});
