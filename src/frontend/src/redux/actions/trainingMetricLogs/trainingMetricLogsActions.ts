import { createAction, createAsyncThunk } from '@reduxjs/toolkit';
import { PopUpCode } from '../../../enums/popUp/popUp';

import type { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import {
    createTrainingLogGroup,
    deleteTrainingLogGroup,
    getTrainingLogsByExercise,
    updateTrainingLogGroup,
} from '../../../services/api/trainingLogs/trainingLogs';
import type { AppDispatch, RootState } from '../../store';
import { ITrainingLogMetricFilter, ITrainingMetricLog } from '../../../interfaces/ITrainingMetricLogs/ITrainingMetricLog';
import { IPayload } from '../../../interfaces/skeleton/IPayload/IPayload';

export const setTrainingMetricLogsFilters = createAction<ITrainingLogMetricFilter>('trainingMetricLogs/setFilters');
export const setTrainingMetricLogsTable = createAction<IPaginated<ITrainingMetricLog>>('trainingMetricLogs/setTable');
export const setTrainingMetricLogsForm = createAction<ITrainingMetricLog>('trainingMetricLogs/setForm');
export const setTrainingMetricLogsLoading = createAction<boolean>('trainingMetricLogs/setLoading');
export const setTrainingMetricLogsError = createAction<string | null>('trainingMetricLogs/setError');
export const setTrainingMetricLogsPopUpCode = createAction<PopUpCode>('trainingMetricLogs/setPopUpCode');
export const setTrainingMetricLogsMessage = createAction<string | null>('trainingMetricLogs/setMessage');
export const setTrainingMetricLogsRestTimerSeconds = createAction<number>('trainingMetricLogs/setRestTimerSeconds');
export const resetTrainingMetricLogs = createAction('trainingMetricLogs/reset');
export const updateTrainingMetricLogsForm = createAction<IPayload>('trainingMetricLogs/updateForm');
export const updateTrainingMetricLogsUnitValue = createAction<{ unitName: string; value: number }>('trainingMetricLogs/updateUnitValue');

type TrainingLogsThunkConfig = {
    state: RootState;
    rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
    return error instanceof Error && error.message.trim().length > 0
        ? error.message
        : fallback;
}

function buildMetricsFromUnitValues(unitValues?: Record<string, number>): NonNullable<ITrainingMetricLog['metrics']> {
    if (!unitValues) {
        return [];
    }

    return Object.entries(unitValues)
        .filter(([, value]) => Number.isFinite(value))
        .map(([code, value]) => ({ code, value }));
}

export const fetchTrainingMetricLogs = createAsyncThunk<IPaginated<ITrainingMetricLog>, void, TrainingLogsThunkConfig>(
    'trainingMetricLogs/fetch',
    async (_arg, { getState, rejectWithValue }) => {
        const { filters, table } = getState().trainingMetricLogs;
        const weekNumber = filters.weekNumber;
        const dayNumber = filters.dayNumber;
        const exerciseCode = filters.exerciseCode ?? filters.exerciseId;

        if (!weekNumber || !dayNumber || !exerciseCode) {
            return {
                items: [],
                totalCount: 0,
                page: table.page,
                pageSize: table.pageSize,
                sortBy: table.sortBy,
                sortDirection: table.sortDirection,
                selectedIds: table.selectedIds,
            };
        }

        try {
            const items = await getTrainingLogsByExercise(weekNumber, dayNumber, exerciseCode);

            return {
                items,
                totalCount: items.length,
                page: table.page,
                pageSize: table.pageSize,
                sortBy: table.sortBy,
                sortDirection: table.sortDirection,
                selectedIds: table.selectedIds,
            };
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudieron cargar los logs.'));
        }
    },
);

export const submitTrainingMetricLogForm = createAsyncThunk<void, void, TrainingLogsThunkConfig>(
    'trainingMetricLogs/submitForm',
    async (_arg, { getState, dispatch, rejectWithValue }) => {
        const { filters, form } = getState().trainingMetricLogs;
        const exerciseCode = filters.exerciseCode ?? filters.exerciseId;

        if (!filters.weekNumber || !filters.dayNumber || !exerciseCode) {
            return rejectWithValue('No hay contexto de semana, dia o ejercicio para crear el log.');
        }

        const metrics = buildMetricsFromUnitValues(form.unitValues);
        if (metrics.length === 0) {
            return rejectWithValue('Debes ingresar al menos una metrica para guardar el log.');
        }

        try {
            await createTrainingLogGroup({
                weekNumber: filters.weekNumber,
                dayNumber: filters.dayNumber,
                exerciseCode,
                metrics,
            });

            void dispatch(fetchTrainingMetricLogs());
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo crear el log.'));
        }
    },
);

export const updateTrainingMetricLogValue = createAsyncThunk<void, { groupId?: string; weekNumber?: number; dayNumber?: number; exerciseCode?: string; metrics?: NonNullable<ITrainingMetricLog['metrics']> }, TrainingLogsThunkConfig>(
    'trainingMetricLogs/updateValue',
    async ({ groupId, weekNumber, dayNumber, exerciseCode, metrics }, { dispatch, rejectWithValue }) => {
        if (!groupId || !weekNumber || !dayNumber || !exerciseCode || !metrics || metrics.length === 0) {
            return rejectWithValue('No se pudo actualizar el log por datos invalidos.');
        }

        const normalizedMetrics = metrics.map((metric) => ({
            code: metric.code ?? metric.unitCode ?? '',
            value: metric.value,
        })).filter((metric) => metric.code);

        if (normalizedMetrics.length === 0) {
            return rejectWithValue('No se pudo actualizar el log por datos invalidos.');
        }

        try {
            await updateTrainingLogGroup(groupId, {
                weekNumber,
                dayNumber,
                exerciseCode,
                metrics: normalizedMetrics,
            });
            await dispatch(fetchTrainingMetricLogs());
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo actualizar el log.'));
        }
    },
);

export const deleteTrainingMetricLogByGroupId = createAsyncThunk<void, string | string[], TrainingLogsThunkConfig>(
    'trainingMetricLogs/deleteByGroupId',
    async (groupIds, { dispatch, rejectWithValue }) => {
        const ids = Array.isArray(groupIds) ? groupIds.filter(Boolean) : [groupIds].filter(Boolean);

        if (ids.length === 0) {
            return rejectWithValue('No se pudo eliminar el log porque no tiene groupId.');
        }

        try {
            await Promise.all(ids.map((groupId) => deleteTrainingLogGroup(groupId)));
            await dispatch(fetchTrainingMetricLogs());
        } catch (error) {
            return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el log.'));
        }
    },
);
export const updateTrainingMetricLogsFormAction = (payload: IPayload) => (dispatch: AppDispatch) => {
    dispatch(updateTrainingMetricLogsForm(payload));
};

export const updateTrainingMetricLogsUnitValueAction = (unitName: string, value: number) => (dispatch: AppDispatch) => {
    dispatch(updateTrainingMetricLogsUnitValue({ unitName, value }));
};
