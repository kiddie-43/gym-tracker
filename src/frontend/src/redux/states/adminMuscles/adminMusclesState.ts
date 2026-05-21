import type { IImportMusclesResult, IMuscle } from '../../../interfaces/muscles/IMuscles';
import type { RootState } from '../../store';

export type AdminMusclesFilters = {
  search: string;
  code: string;
  name: string;
  includeDeleted: boolean;
};

export type AdminMusclesTable = {
  list: IMuscle[];
  page: number;
  rowsPerPage: number;
  totalCount: number;
  sortBy: 'code' | 'name' | 'description';
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];
};


export type AdminMusclesState = {
  table: AdminMusclesTable;
  filters: AdminMusclesFilters;
  form: IMuscle;
  csvResult: IImportMusclesResult | null;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminMusclesInitialState: AdminMusclesState = {
  table: {
    list: [],
    page: 0,
    rowsPerPage: 10,
    totalCount: 0,
    sortBy: 'name',
    sortDirection: 'asc',
    selectedIds: [],
  },
  filters: {
    search: '',
    code: '',
    name: '',
    includeDeleted: false,
  },
  form: {
    id: undefined,
    name: '',
    code: '',
    description: null,
    active: true,
    muscleGroupIds: [],
    isDeleted: false,
    deletedAt: null,
  },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminMusclesState = (state: RootState) => state.adminMuscles;

