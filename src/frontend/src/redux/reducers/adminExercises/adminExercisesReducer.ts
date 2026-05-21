import { createReducer } from '@reduxjs/toolkit';

import {
  fetchAdminExercises,
  resetAdminExercises,
  setAdminExercisesError,
  setAdminExercisesFilters,
  setAdminExercisesForm,
  setAdminExercisesList,
  setAdminExercisesLoading,
  setAdminExercisesPagination,
  setAdminExercisesPopUpCode,
} from '../../actions/adminExercises/adminExercisesActions';
import { adminExercisesInitialState } from '../../states/adminExercises/adminExercisesState';

export const adminExercisesReducer = createReducer(adminExercisesInitialState, (builder) => {
  builder
    .addCase(setAdminExercisesList, (state, action) => {
      state.list = action.payload;
    })
    .addCase(setAdminExercisesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setAdminExercisesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setAdminExercisesPagination, (state, action) => {
      state.pagination = action.payload;
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
    .addCase(fetchAdminExercises.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(fetchAdminExercises.fulfilled, (state, action) => {
      state.loading = false;
      state.list = action.payload.items;
      state.pagination.totalCount = action.payload.totalCount;
      state.pagination.page = Math.max(0, action.payload.page - 1);
      state.pagination.rowsPerPage = action.payload.pageSize;
    })
    .addCase(fetchAdminExercises.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? action.error.message ?? null;
    })
    .addCase(resetAdminExercises, () => adminExercisesInitialState);
});
