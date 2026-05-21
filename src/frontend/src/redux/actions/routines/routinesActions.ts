import { createAction } from '@reduxjs/toolkit';

import type {
  CreateExerciseTrainingLogRequest,
  RoutineCard,
  RoutineDetail,
  TrainingFlowState,
  CatalogAvailability,
} from '../../../interfaces/routines/routines';
import * as routinesApi from '../../../services/api/routines/routinesApi';
import * as trainingFlowApi from '../../../services/api/routines/trainingFlowApi';
import type { AppDispatch } from '../../store';
import type { RoutineFormState, RoutinesFilters } from '../../states/routines/routinesState';

export const setRoutinesList = createAction<RoutineCard[]>('routines/setList');
export const setRoutinesForm = createAction<RoutineFormState>('routines/setForm');
export const setRoutinesFilters = createAction<RoutinesFilters>('routines/setFilters');
export const setRoutinesLoading = createAction<boolean>('routines/setLoading');
export const setRoutinesError = createAction<string | null>('routines/setError');
export const setRoutinesPopUpCode = createAction<string | null>('routines/setPopUpCode');
export const setSelectedRoutine = createAction<RoutineDetail | null>('routines/setSelectedRoutine');
export const setTrainingFlowState = createAction<TrainingFlowState | null>('routines/setTrainingFlowState');
export const setCatalogAvailability = createAction<CatalogAvailability | null>('routines/setCatalogAvailability');
export const resetRoutines = createAction('routines/reset');

export const fetchRoutines = (includeDeleted = false) => async (dispatch: AppDispatch) => {
  dispatch(setRoutinesLoading(true));
  dispatch(setRoutinesError(null));

  try {
    const list = await routinesApi.listRoutines(includeDeleted);
    dispatch(setRoutinesList(list));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error loading routines'));
  } finally {
    dispatch(setRoutinesLoading(false));
  }
};

export const createRoutineAction = (title: string, goal?: string, includeDeleted = false) => async (dispatch: AppDispatch) => {
  dispatch(setRoutinesLoading(true));
  dispatch(setRoutinesError(null));

  try {
    await routinesApi.createRoutine({ title, goal: goal || null });
    const list = await routinesApi.listRoutines(includeDeleted);
    dispatch(setRoutinesList(list));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error creating routine'));
  } finally {
    dispatch(setRoutinesLoading(false));
  }
};

export const archiveRoutineAction = (routineId: string, includeDeleted = false) => async (dispatch: AppDispatch) => {
  dispatch(setRoutinesLoading(true));
  dispatch(setRoutinesError(null));

  try {
    await routinesApi.archiveRoutine(routineId);
    const list = await routinesApi.listRoutines(includeDeleted);
    dispatch(setRoutinesList(list));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error archiving routine'));
  } finally {
    dispatch(setRoutinesLoading(false));
  }
};

export const reactivateRoutineAction = (routineId: string, includeDeleted = false) => async (dispatch: AppDispatch) => {
  dispatch(setRoutinesLoading(true));
  dispatch(setRoutinesError(null));

  try {
    await routinesApi.reactivateRoutine(routineId);
    const list = await routinesApi.listRoutines(includeDeleted);
    dispatch(setRoutinesList(list));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error reactivating routine'));
  } finally {
    dispatch(setRoutinesLoading(false));
  }
};

export const loadRoutineDetail = (routineId: string) => async (dispatch: AppDispatch) => {
  const routine = await routinesApi.getRoutineById(routineId);
  dispatch(setSelectedRoutine(routine));
};

export const createSessionAction = (routineId: string, name: string, daysOfWeek: string[]) => async (dispatch: AppDispatch) => {
  await routinesApi.createRoutineSession(routineId, { name, daysOfWeek });
  dispatch(loadRoutineDetail(routineId));
};

export const addSessionExerciseAction = (routineId: string, sessionId: string, exerciseId: string, name: string) => async (dispatch: AppDispatch) => {
  await routinesApi.addSessionExercise(routineId, sessionId, { exerciseId, name });
  dispatch(loadRoutineDetail(routineId));
};

export const unlinkSessionExerciseAction = (routineId: string, sessionId: string, exerciseId: string) => async (dispatch: AppDispatch) => {
  await routinesApi.unlinkSessionExercise(routineId, sessionId, exerciseId);
  dispatch(loadRoutineDetail(routineId));
};

export const updatePlannedSetAction = (
  routineId: string,
  sessionId: string,
  exerciseId: string,
  setId: string,
  repetitions: number,
  weightKg: number,
) => async (dispatch: AppDispatch) => {
  await routinesApi.updatePlannedSet(routineId, sessionId, exerciseId, setId, { repetitions, weightKg });
  dispatch(loadRoutineDetail(routineId));
};

export const deletePlannedSetAction = (
  routineId: string,
  sessionId: string,
  exerciseId: string,
  setId: string,
) => async (dispatch: AppDispatch) => {
  await routinesApi.deletePlannedSet(routineId, sessionId, exerciseId, setId);
  dispatch(loadRoutineDetail(routineId));
};

export const saveExerciseTrainingLogAction = (request: CreateExerciseTrainingLogRequest) => async () => {
  await trainingFlowApi.createExerciseTrainingLog(request);
};

export const startTrainingFlowAction = (routineId: string) => async (dispatch: AppDispatch) => {
  const state = await trainingFlowApi.startTrainingFlow({ routineId });
  dispatch(setTrainingFlowState(state));
};

export const cancelTrainingFlowAction = () => async (dispatch: AppDispatch) => {
  await trainingFlowApi.cancelTrainingFlow();
  dispatch(setTrainingFlowState(null));
};

export const restoreTrainingFlowAction = () => async (dispatch: AppDispatch) => {
  const state = await trainingFlowApi.getTrainingFlowActive();
  dispatch(setTrainingFlowState(state));
};

export const fetchCatalogAvailabilityAction = () => async (dispatch: AppDispatch) => {
  try {
    const availability = await routinesApi.getCatalogAvailability();
    dispatch(setCatalogAvailability(availability));
  } catch {
    dispatch(setCatalogAvailability(null));
  }
};

export const updateTrainingFlowStepAction = (sessionId?: string, exerciseId?: string) => async (dispatch: AppDispatch) => {
  try {
    const currentState = await trainingFlowApi.getTrainingFlowActive();
    if (!currentState) return;

    const nextStepOrder: TrainingFlowState['stepNode'][] = ['routine', 'session', 'exercise', 'exerciseData'];
    const currentIndex = nextStepOrder.indexOf(currentState.stepNode);
    const nextIndex = Math.min(currentIndex + 1, nextStepOrder.length - 1);

    const updatedState = await trainingFlowApi.updateTrainingFlowActive({
      routineId: currentState.routineId,
      sessionId: sessionId ?? currentState.sessionId,
      exerciseId: exerciseId ?? currentState.exerciseId,
      stepNode: nextStepOrder[nextIndex],
    });

    dispatch(setTrainingFlowState(updatedState));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error updating training flow'));
  }
};

export const previousTrainingFlowStepAction = () => async (dispatch: AppDispatch) => {
  try {
    const currentState = await trainingFlowApi.getTrainingFlowActive();
    if (!currentState) return;

    const previousStepOrder: TrainingFlowState['stepNode'][] = ['routine', 'session', 'exercise', 'exerciseData'];
    const currentIndex = previousStepOrder.indexOf(currentState.stepNode);
    const previousIndex = Math.max(currentIndex - 1, 0);

    let sessionId = currentState.sessionId;
    let exerciseId = currentState.exerciseId;

    // Clear IDs when going back to avoid confusion
    if (previousIndex < currentIndex) {
      if (previousIndex < 1) {
        sessionId = null;
        exerciseId = null;
      } else if (previousIndex < 2) {
        exerciseId = null;
      }
    }

    const updatedState = await trainingFlowApi.updateTrainingFlowActive({
      routineId: currentState.routineId,
      sessionId,
      exerciseId,
      stepNode: previousStepOrder[previousIndex],
    });

    dispatch(setTrainingFlowState(updatedState));
  } catch (error) {
    dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error updating training flow'));
  }
};
