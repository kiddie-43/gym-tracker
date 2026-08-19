import { createReducer } from '@reduxjs/toolkit';
import { PopUpCode } from '../../../enums/popUp/popUp';

import {
  deleteTrainingMetricLogByGroupId,
  fetchTrainingMetricLogs,
  resetTrainingMetricLogs,
  setTrainingMetricLogsError,
  setTrainingMetricLogsFilters,
  setTrainingMetricLogsForm,
  setTrainingMetricLogsLoading,
  setTrainingMetricLogsMessage,
  setTrainingMetricLogsPopUpCode,
  setTrainingMetricLogsRestTimerSeconds,
  setTrainingMetricLogsTable,
  submitTrainingMetricLogForm,
  updateTrainingMetricLogValue,
  updateTrainingMetricLogsForm,
  updateTrainingMetricLogsUnitValue,
} from '../../actions/trainingMetricLogs/trainingMetricLogsActions';
import { trainingMetricLogsInitialState } from '../../states/trainingMetricLogs/trainingMetricLogsState';

export const trainingMetricLogsReducer = createReducer(trainingMetricLogsInitialState, (builder) => {
  builder
    .addCase(setTrainingMetricLogsTable, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setTrainingMetricLogsForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(updateTrainingMetricLogsForm, (state, action) => {
          state.form = { ...state.form, [action.payload.key]: action.payload.value };
    })
    .addCase(updateTrainingMetricLogsUnitValue, (state, action) => {
      state.form.unitValues = { ...state.form.unitValues, [action.payload.unitName]: action.payload.value };
    })
    .addCase(setTrainingMetricLogsFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setTrainingMetricLogsLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setTrainingMetricLogsError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setTrainingMetricLogsMessage, (state, action) => {
      state.message = action.payload;
    })
    .addCase(setTrainingMetricLogsRestTimerSeconds, (state, action) => {
      state.restTimerSeconds = action.payload;
    })
    .addCase(setTrainingMetricLogsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(fetchTrainingMetricLogs.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(fetchTrainingMetricLogs.fulfilled, (state, action) => {
      state.loading = false;
      state.table = action.payload;
    })
    .addCase(fetchTrainingMetricLogs.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudieron cargar los logs.';
    })
    .addCase(submitTrainingMetricLogForm.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(submitTrainingMetricLogForm.fulfilled, (state) => {
      state.loading = false;
      state.popUpCode = PopUpCode.Default;
      state.message = 'Log guardado correctamente.';
    })
    .addCase(submitTrainingMetricLogForm.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudo guardar el log.';
    })
    .addCase(updateTrainingMetricLogValue.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(updateTrainingMetricLogValue.fulfilled, (state) => {
      state.loading = false;
      state.popUpCode = PopUpCode.Default;
      state.message = 'Log actualizado correctamente.';
    })
    .addCase(updateTrainingMetricLogValue.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudo actualizar el log.';
    })
    .addCase(deleteTrainingMetricLogByGroupId.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(deleteTrainingMetricLogByGroupId.fulfilled, (state) => {
      state.loading = false;
      state.message = 'Log eliminado correctamente.';
    })
    .addCase(deleteTrainingMetricLogByGroupId.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudo eliminar el log.';
    })
    .addCase(resetTrainingMetricLogs, () => trainingMetricLogsInitialState);
});