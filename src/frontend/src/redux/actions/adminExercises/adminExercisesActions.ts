import { createAction, createAsyncThunk } from '@reduxjs/toolkit';

import type {
  ExerciseDto,
  ExercisesPageDto,
  ImportExerciseCsvRowRequest,
  ImportExercisesResult,
  UpsertExerciseRequest,
} from '../../../interfaces/admin/exercises/exercises';
import {
  createExercise,
  deleteExercise,
  importExercisesCsv,
  listExercisesPage,
  reactivateExercise,
  updateExercise,
} from '../../../services/api/admin/exercises/exercisesApi';
import type { RootState } from '../../store';
import type {
  AdminExercisesFilters,
  AdminExercisesFormState,
  AdminExercisesPagination,
} from '../../states/adminExercises/adminExercisesState';

export const setAdminExercisesList = createAction<ExerciseDto[]>('adminExercises/setList');
export const setAdminExercisesForm = createAction<AdminExercisesFormState>('adminExercises/setForm');
export const setAdminExercisesFilters = createAction<AdminExercisesFilters>('adminExercises/setFilters');
export const setAdminExercisesPagination = createAction<AdminExercisesPagination>('adminExercises/setPagination');
export const setAdminExercisesLoading = createAction<boolean>('adminExercises/setLoading');
export const setAdminExercisesError = createAction<string | null>('adminExercises/setError');
export const setAdminExercisesPopUpCode = createAction<string | null>('adminExercises/setPopUpCode');
export const resetAdminExercises = createAction('adminExercises/reset');

type AdminExercisesThunkConfig = {
  state: RootState;
  rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

export const fetchAdminExercises = createAsyncThunk<ExercisesPageDto, void, AdminExercisesThunkConfig>(
  'adminExercises/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    try {
      const state = getState().adminExercises;

      return await listExercisesPage({
        includeDeleted: state.filters.includeDeleted,
        search: state.filters.search,
        difficulties: state.filters.difficulties,
        measurementTypeIds: state.filters.measurementTypeIds,
        primaryMuscleIds: state.filters.primaryMuscleIds,
        secondaryMuscleIds: state.filters.secondaryMuscleIds,
        sortBy: state.filters.sortBy,
        sortDirection: state.filters.sortDirection,
        page: state.pagination.page + 1,
        pageSize: state.pagination.rowsPerPage,
      });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el listado.'));
    }
  },
);

export const submitAdminExerciseForm = createAsyncThunk<void, void, AdminExercisesThunkConfig>(
  'adminExercises/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    try {
      const form = getState().adminExercises.form;

      const request: UpsertExerciseRequest = {
        name: form.name.trim(),
        code: form.code.trim(),
        description: form.description.trim() || null,
        category: form.category,
        difficulty: form.difficulty,
        measurementTypeId: form.measurementTypeIds[0] ?? '',
        primaryMuscleIds: form.primaryMuscleIds,
        secondaryMuscleIds: form.secondaryMuscleIds,
        images: [],
        videos: [],
      };

      if (form.id) {
        await updateExercise(form.id, request);
      } else {
        await createExercise(request);
      }
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el registro.'));
    }
  },
);

export const deleteAdminExercises = createAsyncThunk<{ failedCount: number; total: number }, string[], AdminExercisesThunkConfig>(
  'adminExercises/deleteMany',
  async (ids, { rejectWithValue }) => {
    try {
      const results = await Promise.allSettled(ids.map((id) => deleteExercise(id)));
      const failedCount = results.filter((result) => result.status === 'rejected').length;
      return { failedCount, total: ids.length };
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el registro.'));
    }
  },
);

export const reactivateAdminExercise = createAsyncThunk<void, string, AdminExercisesThunkConfig>(
  'adminExercises/reactivate',
  async (id, { rejectWithValue }) => {
    try {
      await reactivateExercise(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo reactivar el registro.'));
    }
  },
);

export const importAdminExercisesCsv = createAsyncThunk<ImportExercisesResult, ImportExerciseCsvRowRequest[], AdminExercisesThunkConfig>(
  'adminExercises/importCsv',
  async (rows, { rejectWithValue }) => {
    try {
      return await importExercisesCsv({ rows });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo procesar el CSV.'));
    }
  },
);
