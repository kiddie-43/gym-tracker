import { PopUpCode } from '../../../enums/popUp/popUp';
import { IRoutine, IRoutineFilter } from '../../../interfaces/routines/IRoutines';
import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';

export const routinesInitialState: IReduxState<IRoutineFilter, IRoutine> = {
  table: {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    sortBy: '',
    sortDirection: 'asc',
  },

  filters: {

  },
  form: {
    name: '',
    createdAt: '',
    description: '',
    isDeleted: false
  },
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null
};

export const selectRoutinesState = (state: RootState) => state.routines;
export const selectRoutinesForm = (state: RootState) => state.routines.form;
