import type { ExerciseDto } from '../../../interfaces/admin/exercises/exercises';
import type { RootState } from '../../store';

export type AdminExercisesFilters = {
  search: string;
  includeDeleted: boolean;
  difficulties: string[];
  measurementTypeIds: string[];
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
  sortBy: 'code' | 'name' | 'category' | 'difficulty';
  sortDirection: 'asc' | 'desc';
};

export type AdminExercisesPagination = {
  page: number;
  rowsPerPage: number;
  totalCount: number;
};

export type AdminExercisesFormState = {
  id: string | null;
  name: string;
  code: string;
  description: string;
  category: string;
  difficulty: string;
  measurementTypeIds: string[];
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
};

export type AdminExercisesState = {
  list: ExerciseDto[];
  filters: AdminExercisesFilters;
  pagination: AdminExercisesPagination;
  form: AdminExercisesFormState;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminExercisesInitialState: AdminExercisesState = {
  list: [],
  filters: {
    search: '',
    includeDeleted: false,
    difficulties: [],
    measurementTypeIds: [],
    primaryMuscleIds: [],
    secondaryMuscleIds: [],
    sortBy: 'name',
    sortDirection: 'asc',
  },
  pagination: {
    page: 0,
    rowsPerPage: 10,
    totalCount: 0,
  },
  form: {
    id: null,
    name: '',
    code: '',
    description: '',
    category: '',
    difficulty: '',
    measurementTypeIds: [],
    primaryMuscleIds: [],
    secondaryMuscleIds: [],
  },
  error: null,
  loading: false,
  popUpCode: null,
};

export const selectAdminExercisesState = (state: RootState) => state.adminExercises;
