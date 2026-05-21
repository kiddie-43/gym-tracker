export type RoutineCard = {
  id: string;
  title: string;
  createdAt: string;
  exerciseCount: number;
  isDeleted: boolean;
};

export type PlannedSet = {
  id: string;
  repetitions: number;
  weightKg: number;
  order: number;
};

export type SessionExercise = {
  id: string;
  exerciseId: string;
  name: string;
  plannedSets: PlannedSet[];
};

export type RoutineSession = {
  id: string;
  name: string;
  daysOfWeek: string[];
  exercises: SessionExercise[];
};

export type RoutineDetail = {
  id: string;
  title: string;
  goal?: string | null;
  createdAt: string;
  isDeleted: boolean;
  sessions: RoutineSession[];
  exerciseCount: number;
};

export type CreateRoutineRequest = {
  title: string;
  goal?: string | null;
};

export type UpdateRoutineRequest = {
  title?: string;
  goal?: string | null;
};

export type CreateRoutineSessionRequest = {
  name: string;
  daysOfWeek: string[];
};

export type AddSessionExerciseRequest = {
  exerciseId: string;
  name: string;
};

export type UpdatePlannedSetRequest = {
  repetitions: number;
  weightKg: number;
};

export type StartTrainingFlowRequest = {
  routineId: string;
};

export type UpdateTrainingFlowRequest = {
  routineId?: string;
  sessionId?: string | null;
  exerciseId?: string | null;
  stepNode: 'routine' | 'session' | 'exercise' | 'exerciseData';
};

export type TrainingFlowState = {
  isLocked: boolean;
  routineId: string;
  sessionId?: string | null;
  exerciseId?: string | null;
  stepNode: 'routine' | 'session' | 'exercise' | 'exerciseData';
  lastUpdatedAt: string;
};

export type PerformedSet = {
  repetitions: number;
  weightKg: number;
  order: number;
};

export type TrainingAttachment = {
  type: 'photo' | 'video';
  url: string;
};

export type CreateExerciseTrainingLogRequest = {
  routineId: string;
  sessionId: string;
  exerciseId: string;
  performedSets: PerformedSet[];
  notes?: string;
  attachments?: TrainingAttachment[];
};

export type ExerciseTrainingLog = {
  id: string;
  userId: string;
  routineId: string;
  sessionId: string;
  exerciseId: string;
  performedSets: PerformedSet[];
  notes?: string;
  attachments: TrainingAttachment[];
  createdAt: string;
  updatedAt: string;
};

export type ProgressBaseline = {
  label: 'lastSession' | 'averageLast4' | 'bestRecent';
  volumeTotal: number;
  loadTotal: number;
  repetitionsTotal: number;
};

export type ProgressComparison = {
  exerciseId: string;
  current: ProgressBaseline;
  baselines: ProgressBaseline[];
  variationPercent: number;
  trend: 'improves' | 'stable' | 'declines';
};

export type CatalogAvailability = {
  isStale: boolean;
  lastUpdatedAt: string;
};
