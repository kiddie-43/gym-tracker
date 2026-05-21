// Compatibility barrel while concrete API clients live in module-scoped files.
export {
  listMuscles,
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
} from './exercises/exercisesApi';
