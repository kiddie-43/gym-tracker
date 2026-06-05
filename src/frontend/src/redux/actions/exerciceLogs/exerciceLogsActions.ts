import { createAction } from '@reduxjs/toolkit';

import { PopUpCode } from '../../../enums/popUp/popUp';
import type {
  CreateExerciseTrainingLogRequest,
  ExerciseTrainingLog,
} from '../../../interfaces/routines/IRoutines';
import type { IPayload } from '../../../interfaces/skeleton/IPayload/IPayload';
import * as trainingFlowApi from '../../../services/api/routines/trainingFlowApi';
import type { AppDispatch } from '../../store';
import {
  exerciceLogsInitialState,
  type ExerciseTrainingLogFilters,
} from '../../states/exerciceLogs/exerciceLogsState';

export const setExerciceLogsList = createAction<ExerciseTrainingLog[]>('exerciceLogs/setList');
export const setExerciceLogsForm = createAction<CreateExerciseTrainingLogRequest>('exerciceLogs/setForm');
export const setExerciceLogsFilters = createAction<ExerciseTrainingLogFilters>('exerciceLogs/setFilters');
export const setExerciceLogsLoading = createAction<boolean>('exerciceLogs/setLoading');
export const setExerciceLogsError = createAction<string | null>('exerciceLogs/setError');
export const setExerciceLogsPopUpCode = createAction<PopUpCode>('exerciceLogs/setPopUpCode');
export const setCurrentDayExerciceLog = createAction<ExerciseTrainingLog | null>('exerciceLogs/setCurrentDayLog');
export const updateExerciceLogsForm = createAction<IPayload>('exerciceLogs/updateForm');
export const resetExerciceLogs = createAction('exerciceLogs/reset');

export const setExerciceLogContextAction = (context: ExerciseTrainingLogFilters) => (dispatch: AppDispatch) => {
  dispatch(setExerciceLogsFilters(context));
  dispatch(
    setExerciceLogsForm({
      ...exerciceLogsInitialState.form,
      ...context,
    }),
  );
};

export const fetchCurrentDayExerciceLogAction = (context: ExerciseTrainingLogFilters) => async (dispatch: AppDispatch) => {
  dispatch(setExerciceLogsLoading(true));
  dispatch(setExerciceLogsError(null));
  dispatch(setExerciceLogContextAction(context));

  try {
    const currentDayLog = await trainingFlowApi.getCurrentDayExerciseTrainingLog(
      context.routineId,
      context.sessionId,
      context.exerciseId,
    );

    dispatch(setCurrentDayExerciceLog(currentDayLog));
    dispatch(setExerciceLogsList(currentDayLog ? [currentDayLog] : []));

    if (currentDayLog) {
      dispatch(
        setExerciceLogsForm({
          routineId: currentDayLog.routineId,
          sessionId: currentDayLog.sessionId,
          exerciseId: currentDayLog.exerciseId,
          performedSets: currentDayLog.performedSets,
          notes: currentDayLog.notes ?? '',
          attachments: currentDayLog.attachments ?? [],
        }),
      );
    }
  } catch (error) {
    dispatch(setCurrentDayExerciceLog(null));
    dispatch(setExerciceLogsList([]));
    dispatch(setExerciceLogsError(error instanceof Error ? error.message : 'Error loading exercise training log'));
  } finally {
    dispatch(setExerciceLogsLoading(false));
  }
};

export const openExerciceLogDialogAction = (code: PopUpCode, log?: ExerciseTrainingLog | null) => (dispatch: AppDispatch) => {
  if (log) {
    dispatch(
      setExerciceLogsForm({
        routineId: log.routineId,
        sessionId: log.sessionId,
        exerciseId: log.exerciseId,
        performedSets: log.performedSets,
        notes: log.notes ?? '',
        attachments: log.attachments ?? [],
      }),
    );
  }

  dispatch(setExerciceLogsPopUpCode(code));
};

export const closeExerciceLogDialogAction = () => (dispatch: AppDispatch) => {
  dispatch(setExerciceLogsPopUpCode(PopUpCode.Default));
};
