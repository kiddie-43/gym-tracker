import { PopUpCode } from '../../../enums/popUp/popUp';
import { ITrainingLogMetricFilter, ITrainingMetricLog } from '../../../interfaces/ITrainingMetricLogs/ITrainingMetricLog';

import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';

export const trainingMetricLogsInitialState: IReduxState<ITrainingLogMetricFilter, ITrainingMetricLog> = {
  table: {
    items: [],
    page: 0,
    pageSize: 10,
    sortBy: 'timestamp',
    sortDirection: 'desc',
    selectedIds: [],
    totalCount: 0,
  },
  filters: {
    weekNumber: undefined,
    dayNumber: undefined,
    exerciseId: '',
    exerciseCode: '',
  },
  form: {
    id: '',
    groupId: '',
    weekNumber: undefined,
    dayNumber: undefined,
    exerciseId: '',
    exerciseCode: '',
    timestamp: '',
    unitValues: {},
    metrics: [],
  },
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null,
  restTimerSeconds: 0,
};

export const selectTrainingMetricLogsState = (state: RootState) => state.trainingMetricLogs;