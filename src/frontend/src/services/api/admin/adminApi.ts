// Compatibility barrel while concrete API clients live in module-scoped files.
export {
  getMuscleById,
  createMuscle,
  updateMuscle,
  deleteMuscle,
  reactivateMuscle,
  importMusclesCsv,
} from './muscles/musclesApi';

export {
  listMeasurementTypes,
  createMeasurementType,
  updateMeasurementType,
  deleteMeasurementType,
  reactivateMeasurementType,
} from './measurementTypes/measurementTypesApi';

export {
  listExercises,
  createExercise,
  updateExercise,
  deleteExercise,
  reactivateExercise,
  importExercisesCsv,
} from './exercises/exercisesApi';
