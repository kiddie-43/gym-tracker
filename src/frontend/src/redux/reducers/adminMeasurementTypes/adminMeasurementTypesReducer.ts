import { createReducer } from '@reduxjs/toolkit';

import {
  deleteAdminMeasurementTypes,
  fetchAdminMeasurementTypes,
  importAdminMeasurementTypesCsv,
  reactivateAdminMeasurementType,
  submitAdminMeasurementTypeForm,
} from '../../actions/adminMeasurementTypes/adminMeasurementTypesThunks';
import {
  resetAdminMeasurementTypes,
  setAdminMeasurementTypesCsvResult,
  setAdminMeasurementTypesError,
  setAdminMeasurementTypesFilters,
  setAdminMeasurementTypesForm,
  setAdminMeasurementTypesLoading,
  setAdminMeasurementTypesPopUpCode,
  setAdminMeasurementTypesTable,
} from '../../actions/adminMeasurementTypes/adminMeasurementTypesActions';
import { adminMeasurementTypesInitialState } from '../../states/adminMeasurementTypes/adminMeasurementTypesState';

export const adminMeasurementTypesReducer = createReducer(adminMeasurementTypesInitialState, (builder) => {
  builder
    // Sync actions
    .addCase(setAdminMeasurementTypesTable, (state, action) => { state.table = action.payload; })
    .addCase(setAdminMeasurementTypesFilters, (state, action) => { state.filters = action.payload; })
    .addCase(setAdminMeasurementTypesForm, (state, action) => { state.form = action.payload; })
    .addCase(setAdminMeasurementTypesCsvResult, (state, action) => { state.csvResult = action.payload; })
    .addCase(setAdminMeasurementTypesLoading, (state, action) => { state.loading = action.payload; })
    .addCase(setAdminMeasurementTypesError, (state, action) => { state.error = action.payload; })
    .addCase(setAdminMeasurementTypesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
      state.error = null;
    })
    .addCase(resetAdminMeasurementTypes, () => adminMeasurementTypesInitialState)

    // fetchAdminMeasurementTypes
    .addCase(fetchAdminMeasurementTypes.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(fetchAdminMeasurementTypes.fulfilled, (state, action) => {
      state.loading = false;
      state.table.list = action.payload.items;
      state.table.totalCount = action.payload.totalCount;
      state.table.page = Math.max(0, action.payload.page - 1);
      state.table.rowsPerPage = action.payload.pageSize;
    })
    .addCase(fetchAdminMeasurementTypes.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // submitAdminMeasurementTypeForm
    .addCase(submitAdminMeasurementTypeForm.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(submitAdminMeasurementTypeForm.fulfilled, (state) => { state.loading = false; })
    .addCase(submitAdminMeasurementTypeForm.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // deleteAdminMeasurementTypes
    .addCase(deleteAdminMeasurementTypes.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(deleteAdminMeasurementTypes.fulfilled, (state, action) => {
      state.loading = false;
      if (action.payload.failedCount > 0) {
        state.error = `No se pudieron eliminar ${String(action.payload.failedCount)} de ${String(action.payload.total)} registros.`;
      }
    })
    .addCase(deleteAdminMeasurementTypes.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // importAdminMeasurementTypesCsv
    .addCase(importAdminMeasurementTypesCsv.pending, (state) => { state.loading = true; state.error = null; state.csvResult = null; })
    .addCase(importAdminMeasurementTypesCsv.fulfilled, (state, action) => {
      state.loading = false;
      state.csvResult = action.payload;
    })
    .addCase(importAdminMeasurementTypesCsv.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    // reactivateAdminMeasurementType
    .addCase(reactivateAdminMeasurementType.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(reactivateAdminMeasurementType.fulfilled, (state) => { state.loading = false; })
    .addCase(reactivateAdminMeasurementType.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    });
});

