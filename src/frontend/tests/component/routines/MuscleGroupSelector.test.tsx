import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { MuscleGroupSelector } from '../../../src/features/routines/components/MuscleGroupSelector';

describe('MuscleGroupSelector', () => {
  it('emits selected values', async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(
      <MuscleGroupSelector
        value={[]}
        options={[
          { id: 'chest', name: 'Chest' },
          { id: 'legs', name: 'Legs' },
        ]}
        onChange={onChange}
      />,
    );

    await user.click(screen.getByLabelText(/muscle groups/i));
    await user.click(screen.getByRole('option', { name: 'Chest' }));

    expect(onChange).toHaveBeenCalled();
  });
});
