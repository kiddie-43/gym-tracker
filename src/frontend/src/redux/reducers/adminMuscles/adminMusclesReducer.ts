import { createReducer } from '@reduxjs/toolkit';

import {
  deleteAdminMuscles,
  fetchAdminMuscles,
  importAdminMusclesCsv,
  reactivateAdminMuscle,
  submitAdminMuscleForm,
} from '../../actions/muscles/muscles';
import {
  resetAdminMuscles,
  setAdminMusclesCsvResult,
  setAdminMusclesError,
  setAdminMusclesFilters,
  setAdminMusclesForm,
  setAdminMusclesLoading,
  setAdminMusclesPopUpCode,
  setAdminMusclesTable,
} from '../../actions/adminMuscles/adminMusclesActions';
import { adminMusclesInitialState } from '../../states/adminMuscles/adminMusclesState';

export const adminMusclesReducer = createReducer(adminMusclesInitialState, (builder) => {
  builder
    // Sync actions
    .addCase(setAdminMusclesTable, (state, action) => { state.table = action.payload; })
    .addCase(setAdminMusclesFilters, (state, action) => { state.filters = action.payload; })
    .addCase(setAdminMusclesForm, (state, action) => { state.form = action.payload; })
    .addCase(setAdminMusclesCsvResult, (state, action) => { state.csvResult = action.payload; })
    .addCase(setAdminMusclesLoading, (state, action) => { state.loading = action.payload; })
    .addCase(setAdminMusclesError, (state, action) => { state.error = action.payload; })
    .addCase(setAdminMusclesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
      state.error = null;
    })
    .addCase(resetAdminMuscles, () => adminMusclesInitialState)

    // fetchAdminMuscles
    .addCase(fetchAdminMuscles.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(fetchAdminMuscles.fulfilled, (state, action) => {
      state.loading = false;
      state.table.list = action.payload.items;
      state.table.totalCount = action.payload.totalCount;
      state.table.page = Math.max(0, action.payload.page - 1);
      state.table.rowsPerPage = action.payload.pageSize;
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

    // importAdminMusclesCsv
    .addCase(importAdminMusclesCsv.pending, (state) => { state.loading = true; state.error = null; state.csvResult = null; })
    .addCase(importAdminMusclesCsv.fulfilled, (state, action) => {
      state.loading = false;
      state.csvResult = action.payload;
    })
    .addCase(importAdminMusclesCsv.rejected, (state, action) => {
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


