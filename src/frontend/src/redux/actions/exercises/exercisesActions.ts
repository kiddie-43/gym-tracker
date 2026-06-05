import { createAction } from '@reduxjs/toolkit';

import type {
  IExercise,
  IExercisesFilter,
} from '../../../interfaces/IExercises/IExercises';
import type { IUnit } from '../../../interfaces/units/IUnit';
import type { IMuscle } from '../../../interfaces/muscles/IMuscles';
import { PopUpCode } from '../../../enums/popUp/popUp';

import type { AppDispatch, RootState } from '../../store';
import { exercisesInitialState } from '../../states/exercises/exercisesState';
import {
  createExercise,
  deleteExercise,
  getExerciseById,
  listExercisesPage,
  reactivateExercise,
  updateExercise,
} from '../../../services/api/exercises/exercisesApi';
import { IPayload } from '../../../interfaces/skeleton/IPayload/IPayload';

// ZONE 1: ACTIONS BASICAS (REDUCER)
export const setExercisesTable = createAction<RootState['exercises']['table']>('adminExercises/setTable');
export const setExercisesFilters = createAction<RootState['exercises']['filters']>('adminExercises/setFilters');
export const setExercisesForm = createAction<IExercise>('adminExercises/setForm');
export const setExercisesCsvResult = createAction<IExercise | null>('adminExercises/setCsvResult');
export const setExercisesReferenceMuscles = createAction<IMuscle[]>('adminExercises/setReferenceMuscles');
export const setExercisesReferenceMeasurementTypes = createAction<IUnit[]>('adminExercises/setReferenceMeasurementTypes');
export const setExercisesLoading = createAction<boolean>('adminExercises/setLoading');
export const setExercisesError = createAction<string | null>('adminExercises/setError');
export const setExercisesPopUpCode = createAction<PopUpCode>('adminExercises/setPopUpCode');
export const resetExercises = createAction('adminExercises/reset');
export const updateExerciseForm = createAction<IPayload>('adminExercises/updateForm');
export const updateExerciseFilter = createAction<IPayload>('adminExercises/updateFilter');
// END ZONE 1

// ZONE 2: ACCIONES CRUD
export const fetchExercisesPageAction = (query?: IExercisesFilter) => async (
  dispatch: AppDispatch,
  getState: () => RootState,
) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    const response = await listExercisesPage(query ?? {});
    const { table } = getState().exercises;

    const normalizedTable: RootState['exercises']['table'] = {
      ...table,
      items: response.items ?? [],
      totalCount: response.totalCount ?? ((response as unknown as { total?: number }).total ?? 0),
      page: response.page ?? table.page,
      pageSize: response.pageSize ?? table.pageSize,
      sortBy: response.sortBy ?? table.sortBy,
      sortDirection: response.sortDirection ?? table.sortDirection,
      selectedIds: table.selectedIds ?? [],
    };

    dispatch(setExercisesTable(normalizedTable));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error loading exercises'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};

export const getExerciseByIdAction = (exerciseId: string) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    const exercise = await getExerciseById(exerciseId);
    dispatch(setExercisesForm(exercise));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error loading exercise detail'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};

export const getExerciseByIdSession = (sessionId: string) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    const exercise = await getExerciseById(sessionId);
    dispatch(setExercisesForm(exercise));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error loading exercise detail'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};



export const AddAndEditExerciseAction = (exercise: IExercise) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));
  try {
    
  
    if (exercise.id) {
      await updateExercise(exercise.id, exercise);
    } else {
      await createExercise(exercise);
    }

    await dispatch(fetchExercisesPageAction() as never);
    dispatch(setExercisePopUpCodeAction(PopUpCode.Default));
    dispatch(setExercisesForm(exercisesInitialState.form));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error saving exercise'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};

export const submitExerciseFormAction = () => async (
  dispatch: AppDispatch,
  getState: () => RootState,
) => {
  const { form } = getState().exercises;
  await dispatch(AddAndEditExerciseAction(form) as never);
};

export const deleteExerciseAction = (exerciseId: string) => async (
  dispatch: AppDispatch,
  getState: () => RootState,
) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    await deleteExercise(exerciseId);

    const { table } = getState().exercises;
    dispatch(setExercisesTable({
      ...table,
      selectedIds: (table.selectedIds ?? []).filter((id: string) => id !== exerciseId),
    }));

    await dispatch(fetchExercisesPageAction() as never);
    dispatch(setExerciseFormAction());
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error deleting exercise'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};

export const deleteSelectedExercisesAction = (exerciseIds: string[]) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    for (const exerciseId of exerciseIds) {
      await deleteExercise(exerciseId);
    }

    await dispatch(fetchExercisesPageAction() as never);
    dispatch(setExercisePopUpCodeAction(PopUpCode.Default));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error deleting exercises'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};
// END ZONE 2

// ZONE 3: ACCIONES BASICAS
export const updateExerciseFormAction = (payload: IPayload) => (dispatch: AppDispatch) => {
  dispatch(updateExerciseForm(payload));
};
export const updateExerciseFilterAction = (payload: IPayload) => (dispatch: AppDispatch) => {
  dispatch(updateExerciseFilter(payload));
};
export const setRemoveExerciseAction = (exercise: IExercise) => (dispatch: AppDispatch) => {
  dispatch(setExerciseDialogStateAction(PopUpCode.Delete, exercise));
};

export const setExerciseFormAction = (exercise?: IExercise) => (dispatch: AppDispatch) => {
  dispatch(setExercisesForm(exercise ?? exercisesInitialState.form));
};

export const setExerciseDialogStateAction = (code: PopUpCode, exercise?: IExercise) => (dispatch: AppDispatch) => {
  if ((code === PopUpCode.Update || code === PopUpCode.Delete) && exercise) {
    dispatch(setExercisesForm(exercise));
    dispatch(setExercisesError(null));
    dispatch(setExercisesPopUpCode(code));
    return;
  }

  if (code === PopUpCode.Create || code === PopUpCode.Default) {
    dispatch(setExerciseFormAction());
    dispatch(setExercisesError(null));
    dispatch(setExercisesPopUpCode(code));
    return;
  }

  dispatch(setExercisesPopUpCode(code));
};

export const setExercisePopUpCodeAction = (code: PopUpCode) => (dispatch: AppDispatch) => {
  dispatch(setExerciseDialogStateAction(code));
};
// END ZONE 3

export const reactivateAdminExercise = (exerciseId: string) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
    await reactivateExercise(exerciseId);
    await dispatch(fetchExercisesPageAction() as never);
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error reactivating exercise'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};

export const importAdminExercisesCsv = (rows: IExercise[]) => async (dispatch: AppDispatch) => {
  dispatch(setExercisesLoading(true));
  dispatch(setExercisesError(null));

  try {
   // const result = await importExercisesCsv({ rows });
   // dispatch(setExercisesCsvResult(result));
  } catch (error) {
    dispatch(setExercisesError(error instanceof Error ? error.message : 'Error importing CSV'));
  } finally {
    dispatch(setExercisesLoading(false));
  }
};
