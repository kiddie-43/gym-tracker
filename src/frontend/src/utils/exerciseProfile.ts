export type ExerciseTrainingProfile = 'strength' | 'cardio';

const STRENGTH_EXERCISE_TYPES = ['STRENGTH', 'BODYWEIGHT', 'PLYOMETRIC', 'REHABILITATION'];
const CARDIO_EXERCISE_TYPES = ['CARDIO', 'MOBILITY', 'STRETCHING', 'SPORTS'];

/**
 * Resuelve el perfil (fuerza/cardio) de un ExerciseType, equivalente al
 * ExerciseTypeProfileMapper del backend.
 */
export function resolveExerciseProfile(exerciseType?: string | null): ExerciseTrainingProfile {
  if (exerciseType && CARDIO_EXERCISE_TYPES.includes(exerciseType)) {
    return 'cardio';
  }

  if (exerciseType && STRENGTH_EXERCISE_TYPES.includes(exerciseType)) {
    return 'strength';
  }

  return 'strength';
}
