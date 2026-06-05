export interface IRoutine {
  id?: string;
  name?: string;
  createdAt?: string;
  description: string;
  isDeleted?: boolean;
  totalSessions?: number;
  exerciseCount?: number;
}

export interface IRoutineFilter {
  description?: string;
  search?: string;
  includeDeleted?: boolean;
  sortBy?: 'createdAt' | 'description' | 'exerciseCount';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export type TrainingAttachmentType = 'photo' | 'video';

export interface TrainingAttachment {
  type: TrainingAttachmentType;
  url: string;
}

export interface PerformedSet {
  repetitions: number;
  weightKg: number;
  order: number;
}

export interface CreateExerciseTrainingLogRequest {
  routineId: string;
  sessionId: string;
  exerciseId: string;
  performedSets: PerformedSet[];
  notes?: string;
  attachments?: TrainingAttachment[];
}

export interface ExerciseTrainingLog extends CreateExerciseTrainingLogRequest {
  id: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export type ProgressBaselineLabel = 'lastSession' | 'averageLast4' | 'bestRecent';
export type ProgressTrend = 'improves' | 'stable' | 'declines';

export interface ProgressBaseline {
  label: ProgressBaselineLabel;
  volumeTotal: number;
  loadTotal: number;
  repetitionsTotal: number;
}

export interface ProgressComparison {
  exerciseId: string;
  current: ProgressBaseline;
  baselines: ProgressBaseline[];
  variationPercent: number;
  trend: ProgressTrend;
}

export type TrainingFlowStepNode = 'routine' | 'session' | 'exercise' | 'exerciseData';

export interface StartTrainingFlowRequest {
  routineId: string;
}

export interface UpdateTrainingFlowRequest {
  sessionId?: string | null;
  exerciseId?: string | null;
  stepNode: TrainingFlowStepNode;
}

export interface TrainingFlowState {
  isLocked: boolean;
  routineId: string;
  sessionId?: string | null;
  exerciseId?: string | null;
  stepNode: TrainingFlowStepNode;
  lastUpdatedAt: string;
}

