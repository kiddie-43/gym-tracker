import type { IImportMeasurementTypesResult, IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { RootState } from '../../store';

export type AdminMeasurementTypesFilters = {
  search: string;
  code: string;
  includeDeleted: boolean;
};

export type AdminMeasurementTypesTable = {
  list: IMeasurementType[];
  page: number;
  rowsPerPage: number;
  totalCount: number;
  sortBy: 'code' | 'name' | 'description';
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];
};

export type AdminMeasurementTypesState = {
  table: AdminMeasurementTypesTable;
  filters: AdminMeasurementTypesFilters;
  form: IMeasurementType;
  csvResult: IImportMeasurementTypesResult | null;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminMeasurementTypesInitialState: AdminMeasurementTypesState = {
  table: {
    list: [],
    page: 0,
    rowsPerPage: 10,
    totalCount: 0,
    sortBy: 'code',
    sortDirection: 'asc',
    selectedIds: [],
  },
  filters: { search: '', code: '', includeDeleted: false },
  form: {
    id: undefined,
    code: '',
    name: '',
    description: null,
  },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminMeasurementTypesState = (state: RootState) => state.adminMeasurementTypes;
