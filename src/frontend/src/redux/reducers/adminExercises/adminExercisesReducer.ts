import { createReducer } from '@reduxjs/toolkit';

import {
  resetAdminExercises,
  setAdminExercisesCsvResult,
  setAdminExercisesError,
  setAdminExercisesFilters,
  setAdminExercisesForm,
  setAdminExercisesLoading,
  setAdminExercisesPopUpCode,
  setAdminExercisesReferenceMeasurementTypes,
  setAdminExercisesReferenceMuscles,
  setAdminExercisesTable,
} from '../../actions/adminExercises/adminExercisesActions';
import {
  deleteAdminExercises,
  fetchAdminExercises,
  importAdminExercisesCsv,
  loadAdminExercisesReferenceData,
  reactivateAdminExercise,
  submitAdminExerciseForm,
} from '../../actions/adminExercises/adminExercisesThunks';
import { adminExercisesInitialState } from '../../states/adminExercises/adminExercisesState';

export const adminExercisesReducer = createReducer(adminExercisesInitialState, (builder) => {
  builder
    .addCase(setAdminExercisesTable, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setAdminExercisesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setAdminExercisesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setAdminExercisesCsvResult, (state, action) => {
      state.csvResult = action.payload;
    })
    .addCase(setAdminExercisesReferenceMuscles, (state, action) => {
      state.referenceMuscles = action.payload;
    })
    .addCase(setAdminExercisesReferenceMeasurementTypes, (state, action) => {
      state.referenceMeasurementTypes = action.payload;
    })
    .addCase(setAdminExercisesLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setAdminExercisesError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setAdminExercisesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    // fetchAdminExercises
    .addCase(fetchAdminExercises.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(fetchAdminExercises.fulfilled, (state, action) => {
      state.loading = false;
      state.table.list = action.payload.items;
      state.table.totalCount = action.payload.totalCount;
      state.table.page = Math.max(0, action.payload.page - 1);
      state.table.rowsPerPage = action.payload.pageSize;
    })
    .addCase(fetchAdminExercises.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    // submitAdminExerciseForm
    .addCase(submitAdminExerciseForm.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(submitAdminExerciseForm.fulfilled, (state) => {
      state.loading = false;
    })
    .addCase(submitAdminExerciseForm.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    // deleteAdminExercises
    .addCase(deleteAdminExercises.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(deleteAdminExercises.fulfilled, (state) => {
      state.loading = false;
    })
    .addCase(deleteAdminExercises.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    // reactivateAdminExercise
    .addCase(reactivateAdminExercise.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(reactivateAdminExercise.fulfilled, (state) => {
      state.loading = false;
    })
    .addCase(reactivateAdminExercise.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    // importAdminExercisesCsv
    .addCase(importAdminExercisesCsv.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(importAdminExercisesCsv.fulfilled, (state, action) => {
      state.loading = false;
      state.csvResult = action.payload;
    })
    .addCase(importAdminExercisesCsv.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    // loadAdminExercisesReferenceData
    .addCase(loadAdminExercisesReferenceData.fulfilled, (state, action) => {
      state.referenceMuscles = action.payload.muscles;
      state.referenceMeasurementTypes = action.payload.measurementTypes;
    })
    .addCase(loadAdminExercisesReferenceData.rejected, (state, action) => {
      state.error = action.payload ?? action.error.message ?? null;
    })
    .addCase(resetAdminExercises, () => adminExercisesInitialState);
});
