import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import { ProgressSummaryPanel } from '../../../src/features/progress/components/ProgressSummaryPanel';
import type { WorkoutProgress } from '../../../src/shared/types/workouts';

describe('ProgressSummaryPanel', () => {
  it('renders regression alerts and comparison percentages', () => {
    const progress: WorkoutProgress = {
      exerciseId: 'bench-press',
      trend: 'Declining',
      calculatedAt: '2026-05-05T10:05:00Z',
      current: { volume: 700, load: 87.5, repetitions: 8 },
      lastSessionComparison: {
        reference: { volume: 800, load: 100, repetitions: 8 },
        volume: { current: 700, reference: 800, percentageChange: -12.5 },
        load: { current: 87.5, reference: 100, percentageChange: -12.5 },
        repetitions: { current: 8, reference: 8, percentageChange: 0 },
      },
      rollingAverageComparison: null,
      bestRecentComparison: null,
    };

    render(<ProgressSummaryPanel progress={progress} />);

    expect(screen.getByRole('alert')).toHaveTextContent(/regression detected/i);
    expect(screen.getAllByText(/-12.50%/i)).toHaveLength(2);
    expect(screen.getByText(/current session/i)).toBeInTheDocument();
  });

  it('renders a no-reference empty comparison state', () => {
    const progress: WorkoutProgress = {
      exerciseId: 'bench-press',
      trend: 'NoReference',
      calculatedAt: '2026-05-05T10:05:00Z',
      current: { volume: 800, load: 100, repetitions: 8 },
      lastSessionComparison: null,
      rollingAverageComparison: null,
      bestRecentComparison: null,
    };

    render(<ProgressSummaryPanel progress={progress} />);

    expect(screen.getByText(/no reference yet/i)).toBeInTheDocument();
    expect(screen.queryByText(/last session/i)).not.toBeInTheDocument();
  });
});
