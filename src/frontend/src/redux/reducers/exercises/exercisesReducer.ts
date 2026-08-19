import { createReducer } from '@reduxjs/toolkit';

import {
  resetExercises,
  setExercisesCsvResult,
  setExercisesError,
  setExercisesFilters,
  setExercisesForm,
  setExercisesLoading,
  setExercisesPopUpCode,
  setExercisesReferenceMeasurementTypes,
  setExercisesReferenceMuscles,
  setExercisesTable,
  updateExerciseFilter,
  updateExerciseForm,
} from '../../actions/exercises/exercisesActions';
import { exercisesInitialState } from '../../states/exercises/exercisesState';

export const adminExercisesReducer = createReducer(exercisesInitialState, (builder) => {
  builder
    .addCase(setExercisesTable, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setExercisesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setExercisesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setExercisesCsvResult, (state, action) => {
      state.csvResult = action.payload;
    })
    .addCase(setExercisesReferenceMuscles, (state, action) => {
      state.referenceMuscles = action.payload;
    })
    .addCase(setExercisesReferenceMeasurementTypes, (state, action) => {
      state.referenceMeasurementTypes = action.payload;
    })
    .addCase(setExercisesLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setExercisesError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setExercisesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    }).addCase(updateExerciseForm, (state, action) => {
      const { key, value } = action.payload;
      state.form = { ...state.form, [key]: value };
    }).addCase(updateExerciseFilter, (state, action) => {
      const { key, value } = action.payload;
      state.filters = { ...state.filters, [key]: value };
    })
    .addCase(resetExercises, () => exercisesInitialState);
});
