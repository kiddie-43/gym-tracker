import { PopUpCode } from '../../../enums/popUp/popUp';
import type { IUnit, IUnitFilters } from '../../../interfaces/units/IUnit';
import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';





export const unitsInitialState: IReduxState<IUnitFilters,IUnit> = {
  table: {
    items: [],
    totalCount: 0,
    page: 0,
    pageSize: 0,
    sortBy: '',
    sortDirection: 'asc'
  },
  filters: { code: '', name: '', description: '' },
  form: {
    id: undefined,
    code: '',
    name: '',
    description: null,
  },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null
};

export const selectUnitState = (state: RootState) => state.units;
