import { createAction, createAsyncThunk } from '@reduxjs/toolkit';
import { PopUpCode } from '../../../enums/popUp/popUp';

import {
    IBlockTemplateItem,
    ICreateTrainingSessionRequest,
    ITrainingSession,
} from '../../../interfaces/trainingSessions/trainingSessions';
import {
    createTrainingSession,
    getBlockTemplates,
    getTrainingSessionHistory,
} from '../../../services/api/trainingSessions/trainingSessionsApi';
import type { RootState } from '../../store';

export const setTrainingSessionsForm = createAction<ICreateTrainingSessionRequest>('trainingSessions/setForm');
export const setTrainingSessionsPopUpCode = createAction<PopUpCode>('trainingSessions/setPopUpCode');
export const resetTrainingSessions = createAction('trainingSessions/reset');

type TrainingSessionsThunkConfig = {
    state: RootState;
    rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
    return error instanceof Error && error.message.trim().length > 0
        ? error.message
        : fallback;
}

export const fetchTrainingSessionHistory = createAsyncThunk<ITrainingSession[], void, TrainingSessionsThunkConfig>(
    'trainingSessions/fetchHistory',
    async (_arg, { getState, rejectWithValue }) => {
        const { form } = getState().trainingSessions;

        if (!form.weekNumber || !form.dayNumber || !form.exerciseId) {
            return [];
        }

        try {
            return await getTrainingSessionHistory(form.weekNumber, form.dayNumber, form.exerciseId);
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el historial de entrenamientos.'));
        }
    },
);

export const fetchBlockTemplates = createAsyncThunk<IBlockTemplateItem[], string, TrainingSessionsThunkConfig>(
    'trainingSessions/fetchBlockTemplates',
    async (exerciseType, { rejectWithValue }) => {
        try {
            return await getBlockTemplates(exerciseType);
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo cargar la plantilla de bloques.'));
        }
    },
);

export const submitTrainingSession = createAsyncThunk<void, void, TrainingSessionsThunkConfig>(
    'trainingSessions/submit',
    async (_arg, { getState, dispatch, rejectWithValue }) => {
        const { form } = getState().trainingSessions;

        if (!form.weekNumber || !form.dayNumber || !form.exerciseId) {
            return rejectWithValue('No hay contexto de semana, dia o ejercicio para guardar el entrenamiento.');
        }

        try {
            await createTrainingSession(form);
            await dispatch(fetchTrainingSessionHistory());
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el entrenamiento.'));
        }
    },
);
