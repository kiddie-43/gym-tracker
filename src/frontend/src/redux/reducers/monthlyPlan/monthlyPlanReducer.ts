import { createReducer } from '@reduxjs/toolkit';

import {
  fetchMonthlyPlanAction,
  linkExerciseToDayAction,
  saveMonthlyPlanAction,
  setMonthlyPlanSelectedDay,
  setMonthlyPlanSelectedWeek,
  truncateMonthlyPlanDaysAction,
  unlinkPlannedExerciseAction,
  updatePlannedExerciseAction,
} from '../../actions/monthlyPlan/monthlyPlanActions';
import { monthlyPlanInitialState } from '../../states/monthlyPlan/monthlyPlanState';

export const monthlyPlanReducer = createReducer(monthlyPlanInitialState, (builder) => {
  builder
    .addCase(setMonthlyPlanSelectedWeek, (state, action) => {
      state.selectedWeek = action.payload;
    })
    .addCase(setMonthlyPlanSelectedDay, (state, action) => {
      state.selectedDay = action.payload;
    })
    .addCase(fetchMonthlyPlanAction.pending, (state) => {
      state.loading = true;
      state.error = null;
    })
    .addCase(fetchMonthlyPlanAction.fulfilled, (state, action) => {
      state.loading = false;
      state.plan = action.payload;
      state.lastConfirmedPlan = action.payload;
      state.error = null;
    })
    .addCase(fetchMonthlyPlanAction.rejected, (state, action) => {
      state.loading = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
      if (state.lastConfirmedPlan) {
        state.plan = state.lastConfirmedPlan;
      }
    })
    .addCase(saveMonthlyPlanAction.pending, (state) => {
      state.saving = true;
      state.error = null;
    })
    .addCase(saveMonthlyPlanAction.fulfilled, (state, action) => {
      state.saving = false;
      state.plan = action.payload;
      state.lastConfirmedPlan = action.payload;
      state.error = null;
    })
    .addCase(saveMonthlyPlanAction.rejected, (state, action) => {
      state.saving = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
      if (state.lastConfirmedPlan) {
        state.plan = state.lastConfirmedPlan;
      }
    })
    .addCase(linkExerciseToDayAction.pending, (state) => {
      state.saving = true;
      state.error = null;
    })
    .addCase(linkExerciseToDayAction.fulfilled, (state) => {
      state.saving = false;
      state.error = null;
    })
    .addCase(linkExerciseToDayAction.rejected, (state, action) => {
      state.saving = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
      if (state.lastConfirmedPlan) {
        state.plan = state.lastConfirmedPlan;
      }
    })
    .addCase(updatePlannedExerciseAction.pending, (state) => {
      state.saving = true;
      state.error = null;
    })
    .addCase(updatePlannedExerciseAction.fulfilled, (state) => {
      state.saving = false;
      state.error = null;
    })
    .addCase(updatePlannedExerciseAction.rejected, (state, action) => {
      state.saving = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
      if (state.lastConfirmedPlan) {
        state.plan = state.lastConfirmedPlan;
      }
    })
    .addCase(unlinkPlannedExerciseAction.pending, (state) => {
      state.saving = true;
      state.error = null;
    })
    .addCase(unlinkPlannedExerciseAction.fulfilled, (state) => {
      state.saving = false;
      state.error = null;
    })
    .addCase(unlinkPlannedExerciseAction.rejected, (state, action) => {
      state.saving = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
      if (state.lastConfirmedPlan) {
        state.plan = state.lastConfirmedPlan;
      }
    })
    .addCase(truncateMonthlyPlanDaysAction.pending, (state) => {
      state.saving = true;
      state.error = null;
    })
    .addCase(truncateMonthlyPlanDaysAction.fulfilled, (state, action) => {
      state.saving = false;
      state.plan = action.payload;
      state.lastConfirmedPlan = action.payload;
      state.error = null;
      if (state.selectedDay > action.payload.activeDays) {
        state.selectedDay = action.payload.activeDays;
      }
    })
    .addCase(truncateMonthlyPlanDaysAction.rejected, (state, action) => {
      state.saving = false;
      state.error = typeof action.payload === 'string' ? action.payload : action.error.message ?? null;
    });
});
