import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { RoutineSessionsSection } from '../../../src/features/routines/components/RoutineSessionsSection';

describe('RoutineSessionsSection', () => {
  it('creates a session when name and day are set', async () => {
    const onCreateSession = vi.fn();
    const user = userEvent.setup();

    render(
      <RoutineSessionsSection
        routine={{
          id: 'r1',
          title: 'A',
          createdAt: new Date().toISOString(),
          isDeleted: false,
          exerciseCount: 0,
          sessions: [],
        }}
        onCreateSession={onCreateSession}
      />,
    );

    await user.click(screen.getByText('monday'));
    await user.type(screen.getByLabelText(/nombre de sesion/i), 'Dia A');
    await user.click(screen.getByRole('button', { name: /agregar sesion/i }));

    expect(onCreateSession).toHaveBeenCalledWith('Dia A', ['monday']);
  });
});
