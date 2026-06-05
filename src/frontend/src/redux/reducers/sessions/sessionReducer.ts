import { createReducer } from '@reduxjs/toolkit';

import {
  resetSessions,
  setSessionsError,
  setSessionsFilters,
  setSessionsForm,
  setSessionsList,
  setSessionsLoading,
  setSessionsPopUpCode,
  updateSessionsForm,
} from '../../actions/sessions/sessionsAction';
import { sessionsInitialState } from '../../states/session/session';

export const sessionsReducer = createReducer(sessionsInitialState, (builder) => {
  builder
    .addCase(setSessionsList, (state, action) => {
      state.table = action.payload;
    })
    .addCase(setSessionsForm, (state, action) => {
      state.form = action.payload;
    }).addCase(updateSessionsForm, (state, action) => {
      state.form = { ...state.form, [action.payload.key]: action.payload.value };
    })
    .addCase(setSessionsFilters, (state, action) => {
      state.filters = action.payload;
    })
    .addCase(setSessionsLoading, (state, action) => {
      state.loading = action.payload;
    })
    .addCase(setSessionsError, (state, action) => {
      state.error = action.payload;
    })
    .addCase(setSessionsPopUpCode, (state, action) => {
      state.popUpCode = action.payload;
    })
    .addCase(resetSessions, () => sessionsInitialState);
});
