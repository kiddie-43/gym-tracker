import { createAsyncThunk } from '@reduxjs/toolkit';

import type { IImportMusclesRequest, IImportMusclesResult, IMuscle, IMuscles, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';
import {
  createMuscle as createMuscleApi,
  deleteMuscle as deleteMuscleApi,
  getMuscleById as getMuscleByIdApi,
  importMusclesCsv as importMusclesCsvApi,
  listMusclesPage,
  reactivateMuscle as reactivateMuscleApi,
  updateMuscle,
} from '../../../services/api/admin/muscles/musclesApi';
import type { RootState } from '../../store';

type AdminMusclesThunkConfig = {
  state: RootState;
  rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

// Fetch paginated list — reads filters + pagination from Redux state
export const fetchAdminMuscles = createAsyncThunk<IMuscles, void, AdminMusclesThunkConfig>(
  'muscles/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    const { filters, table } = getState().adminMuscles;
    try {
      return await listMusclesPage({
        includeDeleted: filters.includeDeleted,
        search: filters.search || undefined,
        code: filters.code || undefined,
        name: filters.name || undefined,
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

// Submit form (create or edit) — reads form from Redux state
export const submitAdminMuscleForm = createAsyncThunk<void, void, AdminMusclesThunkConfig>(
  'muscles/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    const { form } = getState().adminMuscles;
    const request: IUpsertMuscleRequest = {
      name: form.name.trim(),
      code: form.code.trim(),
      description: form.description?.trim() || null,
      active: form.active,
      muscleGroupIds: form.muscleGroupIds,
    };
    try {
      if (form.id) {
        await updateMuscle(form.id, request);
      } else {
        await createMuscleApi(request);
      }
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el músculo.'));
    }
  },
);

// Delete multiple muscles by ID
export const deleteAdminMuscles = createAsyncThunk<{ failedCount: number; total: number }, string[], AdminMusclesThunkConfig>(
  'muscles/deleteMany',
  async (ids, { rejectWithValue }) => {
    try {
      const results = await Promise.allSettled(ids.map((id) => deleteMuscleApi(id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;
      return { failedCount, total: ids.length };
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el músculo.'));
    }
  },
);

// Reactivate a single muscle
export const reactivateAdminMuscle = createAsyncThunk<IMuscle, string, AdminMusclesThunkConfig>(
  'muscles/reactivate',
  async (id, { rejectWithValue }) => {
    try {
      return await reactivateMuscleApi(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo reactivar el músculo.'));
    }
  },
);

// Import muscles from CSV
export const importAdminMusclesCsv = createAsyncThunk<IImportMusclesResult, IImportMusclesRequest, AdminMusclesThunkConfig>(
  'muscles/importCsv',
  async (request, { rejectWithValue }) => {
    try {
      return await importMusclesCsvApi(request);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo importar el CSV.'));
    }
  },
);

// Get muscle by ID
export const getMuscleById = createAsyncThunk<IMuscle | null, string, AdminMusclesThunkConfig>(
  'muscles/getById',
  async (id, { rejectWithValue }) => {
    try {
      return await getMuscleByIdApi(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo obtener el músculo.'));
    }
  },
);

