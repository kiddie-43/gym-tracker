import { createAction } from '@reduxjs/toolkit';

import { PopUpCode } from '../../../enums/popUp/popUp';
import type {
  CreateTrainingMetricGroupRequest,
  MetricDefinition,
  TrainingMetricLog,
} from '../../../interfaces/routines/trainingMetricLogs/TrainingMetricLog';
import {
} from '../../../services/trainingMetricLogs/trainingMetricLogsService';
import type { TrainingMetricLogsFilters, TrainingMetricLogsTable } from '../../states/trainingMetricLogs/trainingMetricLogsState';

export const setTrainingMetricLogsTable = createAction<TrainingMetricLogsTable>('trainingMetricLogs/setTable');
export const setTrainingMetricLogsFilters = createAction<TrainingMetricLogsFilters>('trainingMetricLogs/setFilters');
export const setTrainingMetricLogsForm = createAction<CreateTrainingMetricGroupRequest>('trainingMetricLogs/setForm');
export const setTrainingMetricLogsList = createAction<TrainingMetricLog[]>('trainingMetricLogs/setList');
export const setTrainingMetricLogsMetrics = createAction<MetricDefinition[]>('trainingMetricLogs/setMetrics');
export const setTrainingMetricLogsMetricsDegraded = createAction<boolean>('trainingMetricLogs/setMetricsDegraded');
export const setTrainingMetricLogsLoading = createAction<boolean>('trainingMetricLogs/setLoading');
export const setTrainingMetricLogsError = createAction<string | null>('trainingMetricLogs/setError');
export const setTrainingMetricLogsPopUpCode = createAction<PopUpCode>('trainingMetricLogs/setPopUpCode');
export const setTrainingMetricLogsMessage = createAction<string | null>('trainingMetricLogs/setMessage');
export const resetTrainingMetricLogs = createAction('trainingMetricLogs/reset');
