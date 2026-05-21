import type { IMuscle, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';
import type { RootState } from '../../store';

export type AdminMusclesFilters = {
  search: string;
  includeDeleted: boolean;
};

export type AdminMusclesState = {
  list: IMuscle[];
  filters: AdminMusclesFilters;
  form: IUpsertMuscleRequest;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminMusclesInitialState: AdminMusclesState = {
  list: [],
  filters: {
    search: '',
    includeDeleted: false,
  },
  form: {
    name: '',
    code: '',
    description: null,
    muscleGroupIds: [],
    active: true,
  },
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminMusclesState = (state: RootState) => state.adminMuscles;
