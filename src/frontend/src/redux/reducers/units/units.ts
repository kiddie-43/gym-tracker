import { createReducer } from '@reduxjs/toolkit';

import {
  deleteUnits,
  fetchUnits,
  importUnitsCsv,
  reactivateUnit,
  resetUnits,
  setUnitsCsvResult,
  setUnitsError,
  setUnitsFilters,
  setUnitsForm,
  setUnitsLoading,
  setUnitsPopUpCode,
  setUnitsTable,
  submitUnitForm,
} from '../../actions/units/unitsActions';
import { unitsInitialState } from '../../states/units/unitsState';

export const unitsReducer = createReducer(unitsInitialState, (builder) => {
  builder
    .addCase(setUnitsTable, (state, action) => { state.table = action.payload; })
    .addCase(setUnitsFilters, (state, action) => { state.filters = action.payload; })
    .addCase(setUnitsForm, (state, action) => { state.form = action.payload; })
    .addCase(setUnitsCsvResult, (state, action) => { state.csvResult = action.payload; })
    .addCase(setUnitsLoading, (state, action) => { state.loading = action.payload; })
    .addCase(setUnitsError, (state, action) => { state.error = action.payload; })
    .addCase(setUnitsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
      state.error = null;
    })
    .addCase(resetUnits, () => unitsInitialState)

    .addCase(fetchUnits.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(fetchUnits.fulfilled, (state, action) => {
      state.loading = false;
      state.table.items = action.payload.items;
      state.table.totalCount = action.payload.totalCount;
      state.table.page = action.payload.page;
      state.table.pageSize = action.payload.pageSize;
    })
    .addCase(fetchUnits.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    .addCase(submitUnitForm.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(submitUnitForm.fulfilled, (state) => { state.loading = false; })
    .addCase(submitUnitForm.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    .addCase(deleteUnits.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(deleteUnits.fulfilled, (state, action) => {
      state.loading = false;
      if (action.payload.failedCount > 0) {
        state.error = `No se pudieron eliminar ${String(action.payload.failedCount)} de ${String(action.payload.total)} registros.`;
      }
    })
    .addCase(deleteUnits.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    .addCase(importUnitsCsv.pending, (state) => { state.loading = true; state.error = null; state.csvResult = null; })
    .addCase(importUnitsCsv.fulfilled, (state, action) => {
      state.loading = false;
      state.csvResult = action.payload;
    })
    .addCase(importUnitsCsv.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })

    .addCase(reactivateUnit.pending, (state) => { state.loading = true; state.error = null; })
    .addCase(reactivateUnit.fulfilled, (state) => { state.loading = false; })
    .addCase(reactivateUnit.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    });
});

