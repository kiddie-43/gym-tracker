import { createReducer } from '@reduxjs/toolkit';

import {
  resetRoutines,
  setRoutinesError,
  setRoutinesFilters,
  setRoutinesForm,
  setRoutinesList,
  setRoutinesLoading,
  setRoutinesPopUpCode,
  setSelectedRoutine,
  setTrainingFlowState,
  setCatalogAvailability,
} from '../../actions/routines/routinesActions';
import { routinesInitialState } from '../../states/routines/routinesState';

export const routinesReducer = createReducer(routinesInitialState, (builder) => {
  builder
    .addCase(setRoutinesList, (state, action) => {
      state.list = action.payload;
    })
    .addCase(setRoutinesForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setRoutinesFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setRoutinesLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setRoutinesError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setRoutinesPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(setSelectedRoutine, (state, action) => {
      state.selectedRoutine = action.payload;
    })
    .addCase(setTrainingFlowState, (state, action) => {
      state.trainingFlowState = action.payload;
    })
    .addCase(setCatalogAvailability, (state, action) => {
      state.catalogAvailability = action.payload;
    })
    .addCase(resetRoutines, () => routinesInitialState);
});
