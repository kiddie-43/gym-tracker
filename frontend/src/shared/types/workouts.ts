export type WorkoutSummary = {
  id: string;
  performedAt: string;
  status: 'completed' | 'incomplete';
  exerciseName?: string;
  exerciseEntries?: Array<{
    externalExerciseId?: string;
    exerciseName?: string;
    exerciseNameSnapshot?: string;
    imageUrl?: string | null;
    sets?: Array<{
      repetitions: number;
      weight?: number | null;
      restSeconds?: number | null;
      completed: boolean;
    }>;
  }>;
};

export type WorkoutSetInput = {
  repetitions: number;
  weight?: number | null;
  restSeconds?: number | null;
  completed: boolean;
};

export type ExerciseEntryInput = {
  externalExerciseId: string;
  exerciseName: string;
  muscleGroupIds: string[];
  sets: WorkoutSetInput[];
  notes?: string;
  imageUrl?: string | null;
};

export type CreateWorkoutRequest = {
  performedAt: string;
  status: 'completed' | 'incomplete';
  routineId?: string | null;
  notes?: string;
  exerciseEntries: ExerciseEntryInput[];
};

export type MetricChange = {
  current: number;
  reference: number;
  percentageChange: number;
};

export type ProgressMetrics = {
  volume: number;
  load: number;
  repetitions: number;
};

export type MetricDelta = {
  reference: ProgressMetrics;
  volume: MetricChange;
  load: MetricChange;
  repetitions: MetricChange;
};

export type WorkoutProgress = {
  exerciseId: string;
  trend: 'Improving' | 'Stable' | 'Declining' | 'NoReference' | 'IncompleteSession';
  current: ProgressMetrics;
  lastSessionComparison?: MetricDelta | null;
  rollingAverageComparison?: MetricDelta | null;
  bestRecentComparison?: MetricDelta | null;
  calculatedAt: string;
};

