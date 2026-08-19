import { PopUpCode } from '../../../enums/popUp/popUp';
import { ICreateTrainingSessionRequest, ITrainingSession } from '../../../interfaces/trainingSessions/trainingSessions';
import type { RootState } from '../../store';

export interface ITrainingSessionsState {
  loading: boolean;
  error: string | null;
  popUpCode: PopUpCode;
  form: ICreateTrainingSessionRequest;
  history: ITrainingSession[];
}

export const trainingSessionsInitialState: ITrainingSessionsState = {
  loading: false,
  error: null,
  popUpCode: PopUpCode.Default,
  form: {
    weekNumber: 0,
    dayNumber: 0,
    exerciseId: '',
    durationMinutes: 0,
    secondaryMetricValue: 0,
    secondaryMetricUnitCode: '',
    tertiaryMetricValue: 0,
    notes: '',
    blocks: [],
  },
  history: [],
};

export const selectTrainingSessionsState = (state: RootState) => state.trainingSessions;
