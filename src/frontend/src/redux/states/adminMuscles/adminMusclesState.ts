import { PopUpCode } from '../../../enums/popUp/popUp';
import type {  IMuscle, IMusclesFilter } from '../../../interfaces/IMuscles/IMuscles';
import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';


export const musclesInitialState: IReduxState<IMusclesFilter, IMuscle> = {
  table: {
    items: [],
    page: 0,
    totalCount: 0,
    sortBy: 'name',
    sortDirection: 'asc',
    selectedIds: [],
    pageSize: 10,
  },
  filters: {
    code: '',
    name: '',
  },
  form: {
    id: undefined,
    name: '',
    code: '',
    description: null,
    muscleGroupIds: [],
  },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null
};

export const selectMusclesState = (state: RootState) => state.adminMuscles;

