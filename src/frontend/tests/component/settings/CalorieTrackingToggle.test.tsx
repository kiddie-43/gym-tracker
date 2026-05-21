import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { CalorieTrackingToggle } from '../../../src/features/settings/components/CalorieTrackingToggle';

describe('CalorieTrackingToggle', () => {
  it('updates toggle and goal values', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(
      <CalorieTrackingToggle
        preferences={{ calorieTrackingEnabled: false, dailyCalorieGoal: null }}
        onChange={onChange}
      />,
    );

    await user.click(screen.getByRole('checkbox', { name: /enable calorie tracking/i }));
    await user.type(screen.getByLabelText(/daily calorie goal/i), '2200');

    expect(onChange).toHaveBeenCalled();
  });
});
