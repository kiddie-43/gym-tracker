import { createAction } from '@reduxjs/toolkit';

import type {
    ISession,
    ISessionFilter,
} from '../../../interfaces/ISession/ISession';
import { PopUpCode } from '../../../enums/popUp/popUp';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { IPayload } from '../../../interfaces/skeleton/IPayload/IPayload';
import * as sessionsApi from '../../../services/api/sessions/sessions';
import type { AppDispatch, RootState } from '../../store';
import { sessionsInitialState } from '../../states/session/session';
import { setExercisePopUpCodeAction, setExercisesError, setExercisesLoading, setExercisesTable } from '../exercises/exercisesActions';

// ZONE 1: ACTIONS BASICAS (REDUCER)
export const setSessionsList = createAction<IPaginated<ISession>>('sessions/setList');
export const setSessionsForm = createAction<ISession>('sessions/setForm');
export const setSessionsFilters = createAction<ISessionFilter>('sessions/setFilters');
export const setSessionsLoading = createAction<boolean>('sessions/setLoading');
export const setSessionsError = createAction<string | null>('sessions/setError');
export const setSessionsPopUpCode = createAction<PopUpCode>('sessions/setPopUpCode');
export const setSelectedSession = createAction<ISession | null>('sessions/setSelectedSession');
export const resetSessions = createAction('sessions/reset');
export const updateSessionsForm = createAction<IPayload>('sessions/updateForm');
// END ZONE 1

// ZONE 2: ACCIONES CRUD
export const fetchSessionsByRoutineAction = (routineId: string) => async (dispatch: AppDispatch) => {
    dispatch(setSessionsLoading(true));
    dispatch(setSessionsError(null));

    try {
        const page = await sessionsApi.listSessionsByRoutineId(routineId);
        dispatch(setSessionsList(page));
    } catch (error) {
        dispatch(setSessionsError(error instanceof Error ? error.message : 'Error loading sessions'));
    } finally {
        dispatch(setSessionsLoading(false));
    }
};

export const AddAndEditSessionAction = (routineId: string, session: ISession) => async (dispatch: AppDispatch) => {
    dispatch(setSessionsLoading(true));
    dispatch(setSessionsError(null));

    try {
        if (session.id?.trim()) {
            await sessionsApi.updateSession(routineId, session.id, session);
        } else {
            await sessionsApi.createSession(routineId, session);
        }

        await dispatch(fetchSessionsByRoutineAction(routineId));
        dispatch(setSessionPopUpCodeAction(PopUpCode.Default));
    } catch (error) {
        dispatch(setSessionsError(error instanceof Error ? error.message : 'Error saving session'));
    } finally {
        dispatch(setSessionsLoading(false));
    }
};

export const deleteSessionAction = (routineId: string, sessionId: string) => async (dispatch: AppDispatch) => {
    dispatch(setSessionsLoading(true));
    dispatch(setSessionsError(null));

    try {
        await sessionsApi.deleteSession(routineId, sessionId);
        await dispatch(fetchSessionsByRoutineAction(routineId));
        dispatch(resetSessionFormAction());
    } catch (error) {
        dispatch(setSessionsError(error instanceof Error ? error.message : 'Error deleting session'));
    } finally {
        dispatch(setSessionsLoading(false));
    }
};

export const getExerciseByIdSession = (routineId: string, sessionId: string) => async (
    dispatch: AppDispatch,
    getState: () => RootState,
) => {
    dispatch(setExercisesLoading(true));
    dispatch(setExercisesError(null));

    try {
        const exercises = await sessionsApi.getSessionExercises(routineId, sessionId);
        const { table } = getState().exercises;

        dispatch(setExercisesTable({
            ...table,
            items: exercises,
            totalCount: exercises.length,
            page: 0,
            selectedIds: [],
        }));
    } catch (error) {
        dispatch(setExercisesError(error instanceof Error ? error.message : 'Error loading exercises'));
    } finally {
        dispatch(setExercisesLoading(false));
    }
};

export const unlinkExerciseFromSessionAction = (routineId: string, sessionId: string, exerciseId: string) => async (
    dispatch: AppDispatch,
) => {
    dispatch(setExercisesLoading(true));
    dispatch(setExercisesError(null));

    try {
        await sessionsApi.unlinkSessionExercise(routineId, sessionId, exerciseId);
        await dispatch(getExerciseByIdSession(routineId, sessionId) as never);
        dispatch(setExercisePopUpCodeAction(PopUpCode.Default));
    } catch (error) {
        dispatch(setExercisesError(error instanceof Error ? error.message : 'Error al quitar ejercicio'));
    } finally {
        dispatch(setExercisesLoading(false));
    }
};

export const linkExerciseToSessionAction = (
    routineId: string,
    sessionId: string,
    exerciseId: string,
    name: string,
) => async (
    dispatch: AppDispatch,
) => {
    dispatch(setExercisesLoading(true));
    dispatch(setExercisesError(null));

    try {
        await sessionsApi.addSessionExercise(routineId, sessionId, { exerciseId, name });
        await dispatch(getExerciseByIdSession(routineId, sessionId) as never);
        dispatch(setExercisePopUpCodeAction(PopUpCode.Default));
    } catch (error) {
        dispatch(setExercisesError(error instanceof Error ? error.message : 'Error al añadir ejercicio'));
    } finally {
        dispatch(setExercisesLoading(false));
    }
};


// END ZONE 2

// ZONE 3: ACCIONES BASICAS
export const updateSessionFormAction = (payload: IPayload) => (dispatch: AppDispatch) => {
    dispatch(updateSessionsForm(payload));
};

export const setRemoveSessionAction = (session: ISession) => (dispatch: AppDispatch) => {
    dispatch(setSessionsForm(session));
    dispatch(setSessionPopUpCodeAction(PopUpCode.Delete));
};

export const resetSessionFormAction = () => (dispatch: AppDispatch) => {
    dispatch(setSessionsForm(sessionsInitialState.form));
};
export const setSessionFormAction = (session?: ISession) => (dispatch: AppDispatch) => {
    if (session) {
        dispatch(setSessionsForm(session));
        return;
    } else {
        dispatch(resetSessionFormAction());
    }

};
export const setSessionPopUpCodeAction = (code: PopUpCode) => (dispatch: AppDispatch) => {
    dispatch(setSessionsPopUpCode(code));
    if (code === PopUpCode.Default) {
        dispatch(resetSessionFormAction());
    }
};
// END ZONE 3

// ZONE 5: ALIASES DE COMPATIBILIDAD
// Compatibility aliases used by current sessions UI.
export const fetchSessions = (routineId: string) => fetchSessionsByRoutineAction(routineId);
export const archiveSessionAction = (routineId: string, sessionId: string) => deleteSessionAction(routineId, sessionId);
// END ZONE 5





