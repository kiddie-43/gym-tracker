import { createAsyncThunk } from '@reduxjs/toolkit';

import type { IImportMusclesRequest, IImportMusclesResult, IMuscle, IMuscles, IMusclesFilter, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';
import {
  createMuscle as createMuscleApi,
  deleteMuscle as deleteMuscleApi,
  getMuscleById as getMuscleByIdApi,
  importMusclesCsv as importMusclesCsvApi,
  listMusclesPage,
  reactivateMuscle as reactivateMuscleApi,
  updateMuscle,
} from '../../../services/api/admin/muscles/musclesApi';
import {
  setAdminMusclesError,
  setAdminMusclesList,
  setAdminMusclesLoading,
} from '../adminMuscles/adminMusclesActions';
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

// Get muscles paginated — manages loading/error in Redux
export const getMusclesPaginated = createAsyncThunk<IMuscles, IMusclesFilter, AdminMusclesThunkConfig>(
  'muscles/getPaginated',
  async (query, { dispatch, rejectWithValue }) => {
    try {
      dispatch(setAdminMusclesLoading(true));
      dispatch(setAdminMusclesError(null));
      const result = await listMusclesPage(query);
      dispatch(setAdminMusclesList(result.items));
      return result;
    } catch (error) {
      dispatch(setAdminMusclesError(toErrorMessage(error, 'No se pudo cargar el listado.')));
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el listado.'));
    } finally {
      dispatch(setAdminMusclesLoading(false));
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

// Create muscle
export const createMuscle = createAsyncThunk<IMuscle, IUpsertMuscleRequest, AdminMusclesThunkConfig>(
  'muscles/create',
  async (data, { rejectWithValue }) => {
    try {
      return await createMuscleApi(data);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo crear el músculo.'));
    }
  },
);

// Edit muscle
export const editMuscle = createAsyncThunk<IMuscle, { id: string; data: IUpsertMuscleRequest }, AdminMusclesThunkConfig>(
  'muscles/edit',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      return await updateMuscle(id, data);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo actualizar el músculo.'));
    }
  },
);

// Delete muscle
export const deleteMuscle = createAsyncThunk<string, string, AdminMusclesThunkConfig>(
  'muscles/delete',
  async (id, { rejectWithValue }) => {
    try {
      await deleteMuscleApi(id);
      return id;
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el músculo.'));
    }
  },
);

// Reactivate muscle
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

// Import muscles CSV
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
