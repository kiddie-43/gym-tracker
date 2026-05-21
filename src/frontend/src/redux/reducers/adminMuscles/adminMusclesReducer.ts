import { createReducer } from '@reduxjs/toolkit';

import {
  resetAdminMuscles,
  setAdminMusclesError,
  setAdminMusclesFilters,
  setAdminMusclesForm,
  setAdminMusclesList,
  setAdminMusclesLoading,
  setAdminMusclesPopUpCode,
} from '../../actions/adminMuscles/adminMusclesActions';
import { adminMusclesInitialState } from '../../states/adminMuscles/adminMusclesState';

export const adminMusclesReducer = createReducer(adminMusclesInitialState, (builder) => {
  builder
    .addCase(setAdminMusclesList, (state, action) => {
      state.list = action.payload;
    })
    .addCase(setAdminMusclesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setAdminMusclesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setAdminMusclesLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setAdminMusclesError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setAdminMusclesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(resetAdminMuscles, () => adminMusclesInitialState);
});
