export type RoutineSummary = {
  id: string;
  name: string;
  tags: string[];
  dayCount: number;
};

export type PlannedExerciseInput = {
  externalExerciseId: string;
  exerciseName: string;
  muscleGroupIds: string[];
  targetSets: number;
  targetRepetitions: number;
  targetRestSeconds?: number | null;
  notes?: string;
  imageUrl?: string | null;
};

export type RoutineDayInput = {
  dayLabel: string;
  exercises: PlannedExerciseInput[];
};

export type CreateRoutineRequest = {
  name: string;
  description?: string;
  tags: string[];
  days: RoutineDayInput[];
};

export type Routine = {
  id: string;
  name: string;
  description?: string;
  tags: string[];
  days: RoutineDayInput[];
};
