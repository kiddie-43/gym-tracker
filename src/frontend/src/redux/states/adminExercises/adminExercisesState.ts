import type { IExercise, IImportExercisesResult } from '../../../interfaces/admin/exercises/exercises';
import type { IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { IMuscle } from '../../../interfaces/muscles/IMuscles';
import type { RootState } from '../../store';

export type AdminExercisesTable = {
  list: IExercise[];
  page: number;
  rowsPerPage: number;
  totalCount: number;
  sortBy: 'code' | 'name' | 'category' | 'difficulty';
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];
};

export type AdminExercisesFilters = {
  search: string;
  includeDeleted: boolean;
  difficulties: string[];
  measurementTypeIds: string[];
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
};

export type AdminExercisesState = {
  table: AdminExercisesTable;
  filters: AdminExercisesFilters;
  form: IExercise;
  csvResult: IImportExercisesResult | null;
  referenceMuscles: IMuscle[];
  referenceMeasurementTypes: IMeasurementType[];
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminExercisesInitialState: AdminExercisesState = {
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
    includeDeleted: false,
    difficulties: [],
    measurementTypeIds: [],
    primaryMuscleIds: [],
    secondaryMuscleIds: [],
  },
  form: {
    name: '',
    code: '',
    description: null,
    category: '',
    difficulty: '',
    measurementTypeId: undefined,
    measurementTypeName: undefined,
    primaryMuscles: [],
    secondaryMuscles: [],
    images: [],
    videos: [],
    active: true,
  },
  csvResult: null,
  referenceMuscles: [],
  referenceMeasurementTypes: [],
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminExercisesState = (state: RootState) => state.adminExercises;

