import { createReducer } from '@reduxjs/toolkit';

import {
  resetAdminMeasurementTypes,
  setAdminMeasurementTypesError,
  setAdminMeasurementTypesFilters,
  setAdminMeasurementTypesForm,
  setAdminMeasurementTypesList,
  setAdminMeasurementTypesLoading,
  setAdminMeasurementTypesPopUpCode,
} from '../../actions/adminMeasurementTypes/adminMeasurementTypesActions';
import { adminMeasurementTypesInitialState } from '../../states/adminMeasurementTypes/adminMeasurementTypesState';

export const adminMeasurementTypesReducer = createReducer(adminMeasurementTypesInitialState, (builder) => {
  builder
    .addCase(setAdminMeasurementTypesList, (state, action) => {
      state.list = action.payload;
    })
    .addCase(setAdminMeasurementTypesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setAdminMeasurementTypesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setAdminMeasurementTypesLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setAdminMeasurementTypesError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setAdminMeasurementTypesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(resetAdminMeasurementTypes, () => adminMeasurementTypesInitialState);
});
