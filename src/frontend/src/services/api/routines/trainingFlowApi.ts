import type {
  CreateExerciseTrainingLogRequest,
  ExerciseTrainingLog,
  ProgressComparison,
  StartTrainingFlowRequest,
  TrainingFlowState,
  UpdateTrainingFlowRequest,
} from '../../../interfaces/routines/routines';
import { apiFetch } from '../../httpClient';

export function startTrainingFlow(request: StartTrainingFlowRequest) {
  return apiFetch<TrainingFlowState>('/api/training-flow/start', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function getTrainingFlowActive() {
  return apiFetch<TrainingFlowState | null>('/api/training-flow/active');
}

export function updateTrainingFlowActive(request: UpdateTrainingFlowRequest) {
  return apiFetch<TrainingFlowState>('/api/training-flow/active', {
    method: 'PUT',
    body: JSON.stringify(request),
  });
}

export function cancelTrainingFlow() {
  return apiFetch<void>('/api/training-flow/cancel', {
    method: 'POST',
  });
}

export function createExerciseTrainingLog(request: CreateExerciseTrainingLogRequest) {
  return apiFetch<ExerciseTrainingLog>('/api/exercise-training-logs', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export function getExerciseTrainingLog(logId: string) {
  return apiFetch<ExerciseTrainingLog>(`/api/exercise-training-logs/${logId}`);
}

export function getExerciseProgressComparison(exerciseId: string) {
  const params = new URLSearchParams({ exerciseId });
  return apiFetch<ProgressComparison>(`/api/exercise-training-logs/progress-comparison?${params.toString()}`);
}
