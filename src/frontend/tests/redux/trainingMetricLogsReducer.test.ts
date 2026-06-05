import { describe, expect, it, vi } from 'vitest';

import { trainingMetricLogsReducer } from '../../src/redux/reducers/trainingMetricLogs/trainingMetricLogsReducer';
import {
  setTrainingMetricLogsList,
  setTrainingMetricLogsTable,
} from '../../src/redux/actions/trainingMetricLogs/trainingMetricLogsActions';
import { trainingMetricLogsInitialState } from '../../src/redux/states/trainingMetricLogs/trainingMetricLogsState';
import {
  fetchCurrentDayTrainingMetricLogsAction,
  fetchTrainingMetricLogsPageAction,
} from '../../src/redux/trainingMetricLogs/thunks';
import * as trainingMetricLogsService from '../../src/services/trainingMetricLogs/trainingMetricLogsService';

describe('trainingMetricLogs reducer', () => {
  it('sets list and total count', () => {
    const state = trainingMetricLogsReducer(
      trainingMetricLogsInitialState,
      setTrainingMetricLogsList([
        {
          logId: 'log-1',
          userId: 'user-1',
          routineId: 'r-1',
          sessionId: 's-1',
          trainingId: 't-1',
          exerciseId: 'e-1',
          metricId: 'm-1',
          groupId: 'g-1',
          date: '2026-05-29',
          value: 80,
          createdAt: '2026-05-29T10:00:00Z',
          updatedAt: '2026-05-29T10:00:00Z',
        },
      ]),
    );

    expect(state.table.list).toHaveLength(1);
    expect(state.table.totalCount).toBe(1);
  });

  it('updates pagination table fields', () => {
    const state = trainingMetricLogsReducer(
      trainingMetricLogsInitialState,
      setTrainingMetricLogsTable({
        list: [],
        totalCount: 24,
        page: 2,
        rowsPerPage: 10,
        sortBy: 'createdAt',
        sortDirection: 'desc',
        selectedIds: [],
      }),
    );

    expect(state.table.page).toBe(2);
    expect(state.table.rowsPerPage).toBe(10);
    expect(state.table.totalCount).toBe(24);
  });
});

describe('trainingMetricLogs thunks', () => {
  it('fetchTrainingMetricLogsPageAction dispatches table payload with pagination', async () => {
    const dispatch = vi.fn();
    const serviceSpy = vi.spyOn(trainingMetricLogsService, 'getTrainingMetricLogsPage').mockResolvedValue({
      items: [],
      total: 40,
      page: 1,
      pageSize: 20,
    });

    await fetchTrainingMetricLogsPageAction({
      routineId: 'r',
      sessionId: 's',
      trainingId: 't',
      exerciseId: 'e',
      metricId: '',
      page: 1,
      pageSize: 20,
    })(dispatch as never);

    expect(serviceSpy).toHaveBeenCalledTimes(1);
    const tableAction = dispatch.mock.calls.find(
      (call) => call[0]?.type === 'trainingMetricLogs/setTable',
    )?.[0];
    expect(tableAction).toBeDefined();
    expect(tableAction.payload.page).toBe(1);
    expect(tableAction.payload.rowsPerPage).toBe(20);
    expect(tableAction.payload.totalCount).toBe(40);
  });

  it('fetchCurrentDayTrainingMetricLogsAction dispatches empty message on [] response', async () => {
    const dispatch = vi.fn();
    const serviceSpy = vi.spyOn(trainingMetricLogsService, 'getCurrentDayTrainingMetricLogs').mockResolvedValue([]);

    await fetchCurrentDayTrainingMetricLogsAction({
      routineId: 'r',
      sessionId: 's',
      trainingId: 't',
      exerciseId: 'e',
    })(dispatch as never);

    expect(serviceSpy).toHaveBeenCalledTimes(1);
    const messageAction = dispatch.mock.calls.find(
      (call) => call[0]?.type === 'trainingMetricLogs/setMessage',
    )?.[0];
    expect(messageAction).toBeDefined();
    expect(messageAction.payload).toBe('No logs for current day.');
  });
});