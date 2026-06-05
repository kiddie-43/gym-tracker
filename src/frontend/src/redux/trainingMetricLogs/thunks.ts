import type { TrainingMetricLogsPageResponse } from '../../interfaces/routines/trainingMetricLogs/TrainingMetricLog';
import {
  createTrainingMetricGroup,
  deleteTrainingMetricGroup,
  getAvailableTrainingMetrics,
  getCurrentDayTrainingMetricLogs,
  getTrainingMetricLogsPage,
  updateTrainingMetricValue,
} from '../../services/trainingMetricLogs/trainingMetricLogsService';
import type { AppDispatch } from '../store';
import type { TrainingMetricLogsFilters } from '../states/trainingMetricLogs/trainingMetricLogsState';
import {
  setTrainingMetricLogsError,
  setTrainingMetricLogsList,
  setTrainingMetricLogsLoading,
  setTrainingMetricLogsMessage,
  setTrainingMetricLogsMetrics,
  setTrainingMetricLogsMetricsDegraded,
  setTrainingMetricLogsTable,
} from '../actions/trainingMetricLogs/trainingMetricLogsActions';

export const fetchTrainingMetricLogsPageAction =
  (query: TrainingMetricLogsFilters & { page?: number; pageSize?: number }) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      const result = await getTrainingMetricLogsPage(query);
      dispatch(setTrainingMetricLogsPageResultAction(result));
    } catch (error) {
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error loading training metric logs page',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

export const fetchCurrentDayTrainingMetricLogsAction =
  (filters: Pick<TrainingMetricLogsFilters, 'routineId' | 'sessionId' | 'trainingId' | 'exerciseId'>) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      const result = await getCurrentDayTrainingMetricLogs(
        filters.routineId,
        filters.sessionId,
        filters.trainingId,
        filters.exerciseId,
      );

      dispatch(setTrainingMetricLogsList(result));
      dispatch(setTrainingMetricLogsMessage(result.length === 0 ? 'No logs for current day.' : null));
    } catch (error) {
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error loading current-day training metric logs',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

export const fetchAvailableTrainingMetricsAction =
  (exerciseId: string) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      const result = await getAvailableTrainingMetrics(exerciseId);
      dispatch(setTrainingMetricLogsMetrics(result.metrics));
      dispatch(setTrainingMetricLogsMetricsDegraded(result.degraded));
    } catch (error) {
      dispatch(setTrainingMetricLogsMetrics([]));
      dispatch(setTrainingMetricLogsMetricsDegraded(true));
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error loading available metrics',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

export const createTrainingMetricGroupAction =
  (
    request: {
      routineId: string;
      sessionId: string;
      trainingId: string;
      exerciseId: string;
      metricId: string;
      value: number;
      date: string;
    },
    filtersToRefresh: Pick<TrainingMetricLogsFilters, 'routineId' | 'sessionId' | 'trainingId' | 'exerciseId'>,
  ) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      await createTrainingMetricGroup({
        routineId: request.routineId,
        sessionId: request.sessionId,
        trainingId: request.trainingId,
        exerciseId: request.exerciseId,
        date: request.date,
        metrics: [{ metricId: request.metricId, value: request.value }],
      });

      dispatch(setTrainingMetricLogsMessage('Log creado correctamente.'));
      await dispatch(fetchCurrentDayTrainingMetricLogsAction(filtersToRefresh));
    } catch (error) {
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error creating training metric log',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

export const updateTrainingMetricValueAction =
  (
    request: {
      groupId: string;
      metricId: string;
      value: number;
    },
    filtersToRefresh: Pick<TrainingMetricLogsFilters, 'routineId' | 'sessionId' | 'trainingId' | 'exerciseId'>,
  ) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      await updateTrainingMetricValue(request.groupId, request.metricId, { value: request.value });
      dispatch(setTrainingMetricLogsMessage('Log actualizado correctamente.'));
      await dispatch(fetchCurrentDayTrainingMetricLogsAction(filtersToRefresh));
    } catch (error) {
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error updating training metric log',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

export const deleteTrainingMetricGroupAction =
  (
    groupId: string,
    filtersToRefresh: Pick<TrainingMetricLogsFilters, 'routineId' | 'sessionId' | 'trainingId' | 'exerciseId'>,
  ) =>
  async (dispatch: AppDispatch) => {
    dispatch(setTrainingMetricLogsLoading(true));
    dispatch(setTrainingMetricLogsError(null));

    try {
      await deleteTrainingMetricGroup(groupId);
      dispatch(setTrainingMetricLogsMessage('Log eliminado correctamente.'));
      await dispatch(fetchCurrentDayTrainingMetricLogsAction(filtersToRefresh));
    } catch (error) {
      dispatch(
        setTrainingMetricLogsError(
          error instanceof Error ? error.message : 'Error deleting training metric log',
        ),
      );
    } finally {
      dispatch(setTrainingMetricLogsLoading(false));
    }
  };

function setTrainingMetricLogsPageResultAction(result: TrainingMetricLogsPageResponse) {
  return setTrainingMetricLogsTable({
    list: result.items,
    totalCount: result.total,
    page: result.page,
    rowsPerPage: result.pageSize,
    sortBy: 'createdAt',
    sortDirection: 'desc',
    selectedIds: [],
  });
}