import { createReducer } from '@reduxjs/toolkit';

import {
  deleteAdminMuscles,
  fetchAdminMuscles,
  reactivateAdminMuscle,
  submitAdminMuscleForm,
  setMusclesTable,
  setMusclesFilters,
  setMusclesForm,
  setMusclesCsvResult,
  setMusclesLoading,
  setMusclesError,
  setMusclesPopUpCode,
  resetMuscles,
  importMuscleCsv
} from '../../actions/muscles/musclesActions';

import { musclesInitialState } from '../../states/adminMuscles/adminMusclesState';

export const adminMusclesReducer = createReducer(musclesInitialState, (builder) => {
  builder
    // Sync actions
    .addCase(setMusclesTable, (state, action) => { state.table = action.payload; })
    .addCase(setMusclesFilters, (state, action) => { state.filters = action.payload; })
    .addCase(setMusclesForm, (state, action) => { state.form = action.payload; })
    .addCase(setMusclesCsvResult, (state, action) => { state.csvResult = action.payload; })
    .addCase(setMusclesLoading, (state, action) => { state.loading = action.payload; })
    .addCase(setMusclesError, (state, action) => { state.error = action.payload; })
    .addCase(setMusclesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
      state.error = null;
    })
    .addCase(resetMuscles, () => musclesInitialState)

    // fetchAdminMuscles
    .addCase(fetchAdminMuscles.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(fetchAdminMuscles.fulfilled, (state, action) => {
      state.loading = false;
      state.table.items = action.payload.items;
      state.table.totalCount = action.payload.totalCount;
    })
    .addCase(fetchAdminMuscles.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // submitAdminMuscleForm
    .addCase(submitAdminMuscleForm.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(submitAdminMuscleForm.fulfilled, (state) => { state.loading = false; })
    .addCase(submitAdminMuscleForm.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // deleteAdminMuscles
    .addCase(deleteAdminMuscles.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(deleteAdminMuscles.fulfilled, (state, action) => {
      state.loading = false;
      if (action.payload.failedCount > 0) {
        state.error = `No se pudieron eliminar ${String(action.payload.failedCount)} de ${String(action.payload.total)} registros.`;
      }
    })
    .addCase(deleteAdminMuscles.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // importMuscleCsv
    .addCase(importMuscleCsv.pending, (state) => { state.loading = true; state.error = null; state.csvResult = null; })
    .addCase(importMuscleCsv.fulfilled, (state, action) => {
      state.loading = false;
      state.csvResult = action.payload;
    })
    .addCase(importMuscleCsv.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // reactivateAdminMuscle
    .addCase(reactivateAdminMuscle.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(reactivateAdminMuscle.fulfilled, (state) => { state.loading = false; })
    .addCase(reactivateAdminMuscle.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    });
});


