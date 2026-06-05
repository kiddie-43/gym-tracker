import type { TrainingFlowState } from '../../interfaces/routines/IRoutines';

export function canNavigateOutsideTraining(state: TrainingFlowState | null): boolean {
  return !state?.isLocked;
}
