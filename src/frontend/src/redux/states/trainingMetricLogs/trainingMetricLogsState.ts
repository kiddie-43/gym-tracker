import { PopUpCode } from '../../../enums/popUp/popUp';
import type {
  CreateTrainingMetricGroupRequest,
  MetricDefinition,
  TrainingMetricLog,
} from '../../../interfaces/routines/trainingMetricLogs/TrainingMetricLog';
import type { RootState } from '../../store';

export type TrainingMetricLogsFilters = {
  routineId: string;
  sessionId: string;
  trainingId: string;
  exerciseId: string;
  metricId: string;
};

export type TrainingMetricLogsTable = {
  list: TrainingMetricLog[];
  page: number;
  rowsPerPage: number;
  totalCount: number;
  sortBy: 'date' | 'createdAt' | 'updatedAt' | 'value';
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];
};

export type TrainingMetricLogsState = {
  table: TrainingMetricLogsTable;
  filters: TrainingMetricLogsFilters;
  form: CreateTrainingMetricGroupRequest;
  metrics: MetricDefinition[];
  metricsDegraded: boolean;
  error: string | null;
  loading: boolean;
  popUpCode: PopUpCode;
  message: string | null;
};

export const trainingMetricLogsInitialState: TrainingMetricLogsState = {
  table: {
    list: [],
    page: 0,
    rowsPerPage: 20,
    totalCount: 0,
    sortBy: 'createdAt',
    sortDirection: 'desc',
    selectedIds: [],
  },
  filters: {
    routineId: '',
    sessionId: '',
    trainingId: '',
    exerciseId: '',
    metricId: '',
  },
  form: {
    routineId: '',
    sessionId: '',
    trainingId: '',
    exerciseId: '',
    date: '',
    metrics: [],
  },
  metrics: [],
  metricsDegraded: false,
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null,
};

export const selectTrainingMetricLogsState = (state: RootState) => state.trainingMetricLogs;