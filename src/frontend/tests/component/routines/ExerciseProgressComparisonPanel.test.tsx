import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import { ExerciseProgressComparisonPanel } from '../../../src/features/routines/components/ExerciseProgressComparisonPanel';

vi.mock('../../../src/services/api/routines/trainingFlowApi', () => ({
  getExerciseProgressComparison: vi.fn(async () => ({
    exerciseId: 'exercise-1',
    current: {
      label: 'lastSession',
      volumeTotal: 240,
      loadTotal: 30,
      repetitionsTotal: 8,
    },
    baselines: [
      { label: 'lastSession', volumeTotal: 220, loadTotal: 28, repetitionsTotal: 8 },
      { label: 'averageLast4', volumeTotal: 210, loadTotal: 26, repetitionsTotal: 8 },
      { label: 'bestRecent', volumeTotal: 260, loadTotal: 32, repetitionsTotal: 8 },
    ],
    variationPercent: 14.28,
    trend: 'improves',
  })),
}));

describe('ExerciseProgressComparisonPanel', () => {
  it('shows progress comparison values and trend', async () => {
    render(<ExerciseProgressComparisonPanel exerciseId="exercise-1" />);

    await waitFor(() => {
      expect(screen.getByText(/comparativa de progreso/i)).toBeInTheDocument();
    });

    expect(screen.getByText(/tendencia: mejora/i)).toBeInTheDocument();
    expect(screen.getByText(/variacion: 14.28%/i)).toBeInTheDocument();
  });
});
