import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { ExerciseList } from '../../../src/pages/routines/list/ExerciseList';

describe('Session exercises catalog', () => {
  it('supports selecting and unlinking an exercise card', async () => {
    const user = userEvent.setup();
    const onExerciseSelect = vi.fn();
    const onLinkExercise = vi.fn();
    const onUnlinkExercise = vi.fn();

    render(
      <ExerciseList
        exercises={[
          {
            id: 'se1',
            exerciseId: 'exercise-1',
            name: 'Press banca',
          },
        ]}
        onExerciseSelect={onExerciseSelect}
        onLinkExercise={onLinkExercise}
        onUnlinkExercise={onUnlinkExercise}
      />,
    );

    expect(screen.getByRole('button', { name: /vincular ejercicio/i })).toBeInTheDocument();

    await user.click(screen.getByText('Press banca'));
    expect(onExerciseSelect).toHaveBeenCalledWith('se1');

    await user.click(screen.getByLabelText(/desvincular press banca/i));
    await user.click(screen.getByRole('button', { name: /^desvincular$/i }));
    expect(onUnlinkExercise).toHaveBeenCalledWith('se1');
  });
});
