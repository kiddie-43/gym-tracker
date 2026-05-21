import type { TrainingFlowState } from '../../interfaces/routines/routines';

export function canNavigateOutsideTraining(state: TrainingFlowState | null): boolean {
  return !state?.isLocked;
}
