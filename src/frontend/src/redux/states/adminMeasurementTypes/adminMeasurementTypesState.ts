import type { MeasurementTypeDto, UpsertMeasurementTypeRequest } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { RootState } from '../../store';

export type AdminMeasurementTypesFilters = {
  search: string;
  includeDeleted: boolean;
};

export type AdminMeasurementTypesState = {
  list: MeasurementTypeDto[];
  filters: AdminMeasurementTypesFilters;
  form: UpsertMeasurementTypeRequest;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminMeasurementTypesInitialState: AdminMeasurementTypesState = {
  list: [],
  filters: {
    search: '',
    includeDeleted: false,
  },
  form: {
    key: '',
    name: '',
    unit: '',
    dataType: 'integer',
    category: 'general',
    description: null,
  },
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminMeasurementTypesState = (state: RootState) => state.adminMeasurementTypes;
