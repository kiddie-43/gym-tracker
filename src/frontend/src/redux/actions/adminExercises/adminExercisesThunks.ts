import { createAsyncThunk } from '@reduxjs/toolkit';

import type {
  IExercise,
  IExercises,
  IImportExerciseCsvRowRequest,
  IImportExercisesResult,
} from '../../../interfaces/admin/exercises/exercises';
import type { IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { IMuscle } from '../../../interfaces/muscles/IMuscles';
import {
  createExercise,
  deleteExercise,
  importExercisesCsv,
  listExercisesPage,
  reactivateExercise,
  updateExercise,
} from '../../../services/api/admin/exercises/exercisesApi';
import { listMeasurementTypesPage } from '../../../services/api/admin/measurementTypes/measurementTypesApi';
import { listMusclesPage } from '../../../services/api/admin/muscles/musclesApi';
import type { RootState } from '../../store';

type AdminExercisesThunkConfig = {
  state: RootState;
  rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

export const fetchAdminExercises = createAsyncThunk<IExercises, void, AdminExercisesThunkConfig>(
  'adminExercises/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    try {
      const { table, filters } = getState().adminExercises;

      return await listExercisesPage({
        includeDeleted: filters.includeDeleted,
        search: filters.search,
        difficulties: filters.difficulties,
        measurementTypeIds: filters.measurementTypeIds,
        primaryMuscleIds: filters.primaryMuscleIds,
        secondaryMuscleIds: filters.secondaryMuscleIds,
        sortBy: table.sortBy,
        sortDirection: table.sortDirection,
        page: table.page + 1,
        pageSize: table.rowsPerPage,
      });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el listado.'));
    }
  },
);

export const submitAdminExerciseForm = createAsyncThunk<IExercise, void, AdminExercisesThunkConfig>(
  'adminExercises/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    try {
      const form = getState().adminExercises.form;

      const body = {
        name: form.name.trim(),
        code: form.code?.trim() ?? '',
        description: form.description?.trim() || null,
        category: form.category ?? '',
        difficulty: form.difficulty ?? '',
        measurementTypeIds: form.measurementTypeIds ?? [],
        primaryMuscleIds: form.primaryMuscles.map((m) => m.id!),
        secondaryMuscleIds: form.secondaryMuscles.map((m) => m.id!),
        images: form.images ?? [],
        videos: form.videos ?? [],
      };

      if (form.id) {
        return await updateExercise(form.id, body);
      }
      return await createExercise(body);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el registro.'));
    }
  },
);

export const deleteAdminExercises = createAsyncThunk<
  { failedCount: number; total: number },
  string[],
  AdminExercisesThunkConfig
>(
  'adminExercises/deleteMany',
  async (ids, { rejectWithValue }) => {
    try {
      const results = await Promise.allSettled(ids.map((id) => deleteExercise(id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;
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

export const importAdminExercisesCsv = createAsyncThunk<
  IImportExercisesResult,
  IImportExerciseCsvRowRequest[],
  AdminExercisesThunkConfig
>(
  'adminExercises/importCsv',
  async (rows, { rejectWithValue }) => {
    try {
      return await importExercisesCsv({ rows });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo procesar el CSV.'));
    }
  },
);

export const loadAdminExercisesReferenceData = createAsyncThunk<
  { muscles: IMuscle[]; measurementTypes: IMeasurementType[] },
  void,
  AdminExercisesThunkConfig
>(
  'adminExercises/loadReferenceData',
  async (_arg, { rejectWithValue }) => {
    try {
      const [musclesPage, measurementTypesPage] = await Promise.all([
        listMusclesPage({ page: 1, pageSize: 500 }),
        listMeasurementTypesPage({ page: 1, pageSize: 500 }),
      ]);
      return {
        muscles: musclesPage.items,
        measurementTypes: measurementTypesPage.items,
      };
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar los datos de referencia.'));
    }
  },
);
