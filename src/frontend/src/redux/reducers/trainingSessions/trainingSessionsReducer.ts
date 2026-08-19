import { createReducer } from '@reduxjs/toolkit';
import { PopUpCode } from '../../../enums/popUp/popUp';

import {
    fetchBlockTemplates,
    fetchTrainingSessionHistory,
    resetTrainingSessions,
    setTrainingSessionsForm,
    setTrainingSessionsPopUpCode,
    submitTrainingSession,
} from '../../actions/trainingSessions/trainingSessionsActions';
import { trainingSessionsInitialState } from '../../states/trainingSessions/trainingSessionsState';

export const trainingSessionsReducer = createReducer(trainingSessionsInitialState, (builder) => {
  builder
    .addCase(setTrainingSessionsForm, (state, action) => {
      state.form = action.payload;
    })
    .addCase(setTrainingSessionsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(fetchBlockTemplates.rejected, (state, action) => {
      state.error = action.payload ?? 'No se pudo cargar la plantilla de bloques.';
    })
    .addCase(fetchTrainingSessionHistory.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(fetchTrainingSessionHistory.fulfilled, (state, action) => {
      state.loading = false;
      state.history = action.payload;
    })
    .addCase(fetchTrainingSessionHistory.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudo cargar el historial de entrenamientos.';
    })
    .addCase(submitTrainingSession.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(submitTrainingSession.fulfilled, (state) => {
      state.loading = false;
      state.popUpCode = PopUpCode.Default;
    })
    .addCase(submitTrainingSession.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload ?? 'No se pudo guardar el entrenamiento.';
    })
    .addCase(resetTrainingSessions, () => trainingSessionsInitialState);
});
