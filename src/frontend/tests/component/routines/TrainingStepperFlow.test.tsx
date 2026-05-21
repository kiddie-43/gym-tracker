import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { TrainingStepper } from '../../../src/features/routines/components/TrainingStepper';
import { canNavigateOutsideTraining } from '../../../src/features/routines/guards/trainingNavigationGuard';

describe('Training stepper flow', () => {
  it('shows current step and blocks external navigation when locked', async () => {
    const user = userEvent.setup();
    const onNext = vi.fn();
    const onBack = vi.fn();

    render(
      <TrainingStepper
        state={{
          isLocked: true,
          routineId: 'r1',
          sessionId: null,
          exerciseId: null,
          stepNode: 'session',
          lastUpdatedAt: new Date().toISOString(),
        }}
        onNext={onNext}
        onBack={onBack}
      />,
    );

    expect(screen.getByText(/paso: session/i)).toBeInTheDocument();
    expect(canNavigateOutsideTraining({
      isLocked: true,
      routineId: 'r1',
      sessionId: null,
      exerciseId: null,
      stepNode: 'session',
      lastUpdatedAt: new Date().toISOString(),
    })).toBe(false);

    await user.click(screen.getByRole('button', { name: /siguiente/i }));
    await user.click(screen.getByRole('button', { name: /atras/i }));

    expect(onNext).toHaveBeenCalled();
    expect(onBack).toHaveBeenCalled();
  });
});
