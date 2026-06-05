import { createAction } from '@reduxjs/toolkit';

import type {
	IRoutine,
	IRoutineFilter,
} from '../../../interfaces/routines/IRoutines';
import { PopUpCode } from '../../../enums/popUp/popUp';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { IPayload } from '../../../interfaces/skeleton/IPayload/IPayload';
import * as routinesApi from '../../../services/api/routines/routinesApi';
import * as trainingFlowApi from '../../../services/api/routines/trainingFlowApi';
import type { AppDispatch } from '../../store';
import { routinesInitialState } from '../../states/routines/routinesState';

// ZONE 1: ACTIONS BASICAS (REDUCER)
export const setRoutinesList = createAction<IPaginated<IRoutine>>('routines/setList');
export const setRoutinesForm = createAction<IRoutine>('routines/setForm');
export const setRoutinesFilters = createAction<IRoutineFilter>('routines/setFilters');
export const setRoutinesLoading = createAction<boolean>('routines/setLoading');
export const setRoutinesError = createAction<string | null>('routines/setError');
export const setRoutinesPopUpCode = createAction<PopUpCode>('routines/setPopUpCode');
export const setSelectedRoutine = createAction<IRoutine | null>('routines/setSelectedRoutine');
export const setTrainingFlowState = createAction<Awaited<ReturnType<typeof trainingFlowApi.getTrainingFlowActive>>>('routines/setTrainingFlowState');
export const setCatalogAvailability = createAction<unknown | null>('routines/setCatalogAvailability');
export const resetRoutines = createAction('routines/reset');
export const updateRoutinesForm = createAction<IPayload>('routines/updateForm');
// END ZONE 1

// ZONE 2: ACCIONES CRUD
export const fetchRoutinesPage = (query: IRoutineFilter = {}) => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		const page = await routinesApi.listRoutinesPage(query);
		dispatch(setRoutinesList(page));
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error loading routines'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};

export const getRoutineByIdAction = (id: string) => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		const routine = await routinesApi.getRoutineById(id);
		dispatch(setSelectedRoutine(routine));
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error loading routine detail'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};

export const AddAndEditRoutineAction = (routine: IRoutine) => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		if (routine.id) {
			await routinesApi.updateRoutine(routine.id, routine);
		} else {
			await routinesApi.createRoutine(routine);
		}
		await dispatch(fetchRoutines());
		dispatch(setRoutinePopUpCodeAction(PopUpCode.Default));
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error saving routine'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};


export const deleteRoutineAction = (id: string) => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		await routinesApi.deleteRoutine(id);
		await dispatch(fetchRoutinesPage());
		dispatch(resetRoutineFormAction());
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error deleting routine'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};

export const reactivateRoutineAction = (routineId: string, includeDeleted = false) => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		await routinesApi.reactivateRoutine(routineId);
		await dispatch(fetchRoutines(includeDeleted));
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error reactivating routine'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};
// END ZONE 2

// ZONE 3: ACCIONES BASICAS
export const updateRoutineFormAction = (payload: IPayload) => (dispatch: AppDispatch) => {
	dispatch(updateRoutinesForm(payload));
};

export const setRemoveRoutineAction = (routine: IRoutine) => (dispatch: AppDispatch) => {
	dispatch(setRoutinesForm(routine));
	dispatch(setRoutinePopUpCodeAction(PopUpCode.Delete));
};

export const resetRoutineFormAction = () => (dispatch: AppDispatch) => {
	dispatch(setRoutinesForm(routinesInitialState.form));
};
export const setRoutineFormAction = (routine?: IRoutine) => (dispatch: AppDispatch) => {
	if (routine) {
		dispatch(setRoutinesForm(routine));
		return;
	} else {
		dispatch(resetRoutineFormAction());
	}

};
export const setRoutinePopUpCodeAction = (code: PopUpCode) => (dispatch: AppDispatch) => {
	dispatch(setRoutinesPopUpCode(code));
	if (code === PopUpCode.Default) {
		dispatch(resetRoutineFormAction());
	}
};
// END ZONE 3


// ZONE 4: ACCIONES TRAINING FLOW
export const restoreTrainingFlowAction = () => async (dispatch: AppDispatch) => {
	dispatch(setRoutinesLoading(true));
	dispatch(setRoutinesError(null));

	try {
		const trainingFlow = await trainingFlowApi.getTrainingFlowActive();
		dispatch(setTrainingFlowState(trainingFlow));
		dispatch(setCatalogAvailability(null));
	} catch (error) {
		dispatch(setRoutinesError(error instanceof Error ? error.message : 'Error restoring training flow'));
	} finally {
		dispatch(setRoutinesLoading(false));
	}
};
// END ZONE 4

// ZONE 5: ALIASES DE COMPATIBILIDAD
// Compatibility aliases used by current routines UI.
export const fetchRoutines = (includeDeleted = false) => fetchRoutinesPage({ includeDeleted } as IRoutineFilter);
export const loadRoutineDetail = (routineId: string) => getRoutineByIdAction(routineId);
export const archiveRoutineAction = (routineId: string) => deleteRoutineAction(routineId);
// END ZONE 5





