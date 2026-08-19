import { createAction, createAsyncThunk } from '@reduxjs/toolkit';

import type {
  ILinkMonthlyPlanExerciseRequest,
  IUpdatePlannedExerciseRequest,
  IUpsertMonthlyPlanRequest,
} from '../../../interfaces/monthlyPlan/IMonthlyPlan';
import * as monthlyPlansApi from '../../../services/api/monthlyPlans/monthlyPlansApi';

export const setMonthlyPlanSelectedWeek = createAction<number>('monthlyPlan/setSelectedWeek');
export const setMonthlyPlanSelectedDay = createAction<number>('monthlyPlan/setSelectedDay');

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}


export const fetchMonthlyPlanAction = createAsyncThunk(
  'monthlyPlan/fetch',
  async (_: void, { rejectWithValue }) => {
    try {
      return await monthlyPlansApi.getMyMonthlyPlan();
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el plan mensual.'));
    }
  },
);

export const saveMonthlyPlanAction = createAsyncThunk(
  'monthlyPlan/save',
  async (request: IUpsertMonthlyPlanRequest, { rejectWithValue }) => {
    try {
      return await monthlyPlansApi.upsertMyMonthlyPlan(request);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el plan mensual.'));
    }
  },
);

export const truncateMonthlyPlanDaysAction = createAsyncThunk(
  'monthlyPlan/truncateDays',
  async (activeDays: number, { rejectWithValue }) => {
    try {
      return await monthlyPlansApi.truncateMonthlyPlanDays({ activeDays, confirmed: true });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo recortar el plan mensual.'));
    }
  },
);

export const linkExerciseToDayAction = createAsyncThunk(
  'monthlyPlan/linkExercise',
  async (request: ILinkMonthlyPlanExerciseRequest, { dispatch, rejectWithValue }) => {
    try {
      await monthlyPlansApi.linkExerciseToMyDay(request);
      await dispatch(fetchMonthlyPlanAction());
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo vincular el ejercicio.'));
    }
  },
);

export const updatePlannedExerciseAction = createAsyncThunk(
  'monthlyPlan/updatePlannedExercise',
  async (
    payload: { id: string; request: IUpdatePlannedExerciseRequest },
    { dispatch, rejectWithValue },
  ) => {
    try {
      await monthlyPlansApi.updatePlannedExercise(payload.id, payload.request);
      await dispatch(fetchMonthlyPlanAction());
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo actualizar el ejercicio.'));
    }
  },
);

export const unlinkPlannedExerciseAction = createAsyncThunk(
  'monthlyPlan/unlinkPlannedExercise',
  async (id: string, { dispatch, rejectWithValue }) => {
    try {
      await monthlyPlansApi.unlinkPlannedExercise(id);
      await dispatch(fetchMonthlyPlanAction());
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el ejercicio.'));
    }
  },
);

