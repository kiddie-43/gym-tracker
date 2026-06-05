import { createReducer } from '@reduxjs/toolkit';

import {
  resetRoutines,
  setRoutinesError,
  setRoutinesFilters,
  setRoutinesForm,
  setRoutinesList,
  setRoutinesLoading,
  setRoutinesPopUpCode,
  updateRoutinesForm,
} from '../../actions/routines/routinesActions';
import { routinesInitialState } from '../../states/routines/routinesState';

export const routinesReducer = createReducer(routinesInitialState, (builder) => {
  builder
    .addCase(setRoutinesList, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setRoutinesForm, (state, action) => {
      state.form = action.payload;
    }).addCase(updateRoutinesForm, (state, action) => {
      state.form = { ...state.form, [action.payload.key]: action.payload.value };
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
    .addCase(resetRoutines, () => routinesInitialState);
});
