import { PopUpCode } from '../../../enums/popUp/popUp';
import type {
  CreateExerciseTrainingLogRequest,
  ExerciseTrainingLog,
} from '../../../interfaces/routines/IRoutines';
import type { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';

export type ExerciseTrainingLogFilters = {
  routineId: string;
  sessionId: string;
  exerciseId: string;
};

export type ExerciceLogsState = IReduxState<ExerciseTrainingLogFilters, CreateExerciseTrainingLogRequest> & {
  currentDayLog: ExerciseTrainingLog | null;
};

export const exerciceLogsInitialState: ExerciceLogsState = {
  table: {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    sortBy: '',
    sortDirection: 'asc',
  },
  filters: {
    routineId: '',
    sessionId: '',
    exerciseId: '',
  },
  form: {
    routineId: '',
    sessionId: '',
    exerciseId: '',
    performedSets: [],
    notes: '',
    attachments: [],
  },
  currentDayLog: null,
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null,
};

export const selectExerciceLogsState = (state: RootState) => state.exerciceLogs;
