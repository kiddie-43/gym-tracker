import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { ExerciseSearchStep } from '../../../src/pages/WorkoutNew/components/ExerciseSearchStep';

const options = [
  {
    id: 'ex-1',
    name: 'Press banca',
    muscleGroupIds: ['upper-body'],
    imageUrl: null,
    muscleGroupLabel: 'Tren superior',
  },
  {
    id: 'ex-2',
    name: 'Sentadilla',
    muscleGroupIds: ['lower-body'],
    imageUrl: null,
    muscleGroupLabel: 'Tren inferior',
  },
];

describe('AdminExerciseSelector', () => {
  it('filters and selects exercise options', async () => {
    const user = userEvent.setup();
    const onSelectExercise = vi.fn();
    const onToggleFavorite = vi.fn();
    const onQueryChange = vi.fn();

    render(
      <ExerciseSearchStep
        allOptions={options}
        selectedOption={null}
        query=""
        recentOptions={options}
        favoriteOptions={[options[0]]}
        filteredOptions={options}
        favoriteIds={['ex-1']}
        onQueryChange={onQueryChange}
        onSelectExercise={onSelectExercise}
        onToggleFavorite={onToggleFavorite}
      />,
    );

    expect(screen.getByText('Recientes')).toBeInTheDocument();
    expect(screen.getByText('Favoritos')).toBeInTheDocument();

    await user.click(screen.getAllByText('Press banca')[0]);
    expect(onSelectExercise).toHaveBeenCalledWith('ex-1');
  });
});
