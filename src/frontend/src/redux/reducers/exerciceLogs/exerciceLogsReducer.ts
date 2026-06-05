import { createReducer } from '@reduxjs/toolkit';

import {
  resetExerciceLogs,
  setCurrentDayExerciceLog,
  setExerciceLogsError,
  setExerciceLogsFilters,
  setExerciceLogsForm,
  setExerciceLogsList,
  setExerciceLogsLoading,
  setExerciceLogsPopUpCode,
  updateExerciceLogsForm,
} from '../../actions/exerciceLogs/exerciceLogsActions';
import { exerciceLogsInitialState } from '../../states/exerciceLogs/exerciceLogsState';

export const exerciceLogsReducer = createReducer(exerciceLogsInitialState, (builder) => {
  builder
    .addCase(setExerciceLogsList, (state, action) => {
      state.table.items = action.payload;
      state.table.totalCount = action.payload.length;
    })
    .addCase(setExerciceLogsForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setExerciceLogsFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setExerciceLogsLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setExerciceLogsError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setExerciceLogsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(setCurrentDayExerciceLog, (state, action) => {
      state.currentDayLog = action.payload;
    })
    .addCase(updateExerciceLogsForm, (state, action) => {
      const { key, value } = action.payload;
      state.form = { ...state.form, [key]: value };
    })
    .addCase(resetExerciceLogs, () => exerciceLogsInitialState);
});
