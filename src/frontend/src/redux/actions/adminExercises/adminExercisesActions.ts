import { createAction } from '@reduxjs/toolkit';

import type {
  IExercise,
  IImportExercisesResult,
} from '../../../interfaces/admin/exercises/exercises';
import type { IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { IMuscle } from '../../../interfaces/muscles/IMuscles';
import type {
  AdminExercisesFilters,
  AdminExercisesTable,
} from '../../states/adminExercises/adminExercisesState';

export const setAdminExercisesTable = createAction<AdminExercisesTable>('adminExercises/setTable');
export const setAdminExercisesFilters = createAction<AdminExercisesFilters>('adminExercises/setFilters');
export const setAdminExercisesForm = createAction<IExercise>('adminExercises/setForm');
export const setAdminExercisesCsvResult = createAction<IImportExercisesResult | null>('adminExercises/setCsvResult');
export const setAdminExercisesReferenceMuscles = createAction<IMuscle[]>('adminExercises/setReferenceMuscles');
export const setAdminExercisesReferenceMeasurementTypes = createAction<IMeasurementType[]>('adminExercises/setReferenceMeasurementTypes');
export const setAdminExercisesLoading = createAction<boolean>('adminExercises/setLoading');
export const setAdminExercisesError = createAction<string | null>('adminExercises/setError');
export const setAdminExercisesPopUpCode = createAction<string | null>('adminExercises/setPopUpCode');
export const resetAdminExercises = createAction('adminExercises/reset');
