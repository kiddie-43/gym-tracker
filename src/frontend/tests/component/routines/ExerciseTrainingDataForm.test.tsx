import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { ExerciseTrainingDataForm } from '../../../src/features/routines/components/ExerciseTrainingDataForm';

describe('ExerciseTrainingDataForm', () => {
  it('limits attachments to 5 in UI', async () => {
    const onSubmit = vi.fn();
    const user = userEvent.setup();

    render(
      <ExerciseTrainingDataForm
        routineId="r1"
        sessionId="s1"
        exerciseId="e1"
        onSubmit={onSubmit}
      />,
    );

    const addButton = screen.getByRole('button', { name: /agregar adjunto/i });
    for (let index = 0; index < 6; index += 1) {
      await user.click(addButton);
    }

    expect(screen.getByText(/adjuntos: 5\/5/i)).toBeInTheDocument();
  });
});
