import { createReducer } from '@reduxjs/toolkit';

import {
  resetTrainingMetricLogs,
  setTrainingMetricLogsError,
  setTrainingMetricLogsFilters,
  setTrainingMetricLogsForm,
  setTrainingMetricLogsList,
  setTrainingMetricLogsLoading,
  setTrainingMetricLogsMessage,
  setTrainingMetricLogsMetrics,
  setTrainingMetricLogsMetricsDegraded,
  setTrainingMetricLogsPopUpCode,
  setTrainingMetricLogsTable,
} from '../../actions/trainingMetricLogs/trainingMetricLogsActions';
import { trainingMetricLogsInitialState } from '../../states/trainingMetricLogs/trainingMetricLogsState';

export const trainingMetricLogsReducer = createReducer(trainingMetricLogsInitialState, (builder) => {
  builder
    .addCase(setTrainingMetricLogsTable, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setTrainingMetricLogsFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setTrainingMetricLogsForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setTrainingMetricLogsList, (state, action) => {
      state.table.list = action.payload;
      state.table.totalCount = action.payload.length;
    })
    .addCase(setTrainingMetricLogsMetrics, (state, action) => {
      state.metrics = action.payload;
    })
    .addCase(setTrainingMetricLogsMetricsDegraded, (state, action) => {
      state.metricsDegraded = action.payload;
    })
    .addCase(setTrainingMetricLogsLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setTrainingMetricLogsError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setTrainingMetricLogsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
      state.error = null;
    })
    .addCase(setTrainingMetricLogsMessage, (state, action) => {
      state.message = action.payload;
    })
    .addCase(resetTrainingMetricLogs, () => trainingMetricLogsInitialState);
});