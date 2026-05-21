import { createAsyncThunk } from '@reduxjs/toolkit';

import type { MuscleDto, MusclesPageDto, UpsertMuscleRequest } from '../../../interfaces/admin/muscles/muscles';
import {
  createMuscle as createMuscleApi,
  deleteMuscle as deleteMuscleApi,
  listMusclesPage,
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

// Get muscles paginated
export const getMusclesPaginated = createAsyncThunk<MusclesPageDto, { page: number; limit: number }, AdminMusclesThunkConfig>(
  'muscles/getPaginated',
  async ({ page, limit }, { dispatch, rejectWithValue }) => {
    try {
      dispatch(setAdminMusclesLoading(true));
      const result = await listMusclesPage({ page, pageSize: limit });
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

// Get muscle by ID (resolves from current list in state)
export const getMuscleById = createAsyncThunk<MuscleDto | null, string, AdminMusclesThunkConfig>(
  'muscles/getById',
  async (id, { getState }) => {
    const list = getState().adminMuscles.list;
    return list.find((m) => m.id === id) ?? null;
  },
);

// Create muscle
export const createMuscle = createAsyncThunk<MuscleDto, UpsertMuscleRequest, AdminMusclesThunkConfig>(
  'muscles/create',
  async (data, { dispatch, getState, rejectWithValue }) => {
    try {
      dispatch(setAdminMusclesLoading(true));
      const created = await createMuscleApi(data);
      dispatch(setAdminMusclesList([...getState().adminMuscles.list, created]));
      return created;
    } catch (error) {
      dispatch(setAdminMusclesError(toErrorMessage(error, 'No se pudo crear el músculo.')));
      return rejectWithValue(toErrorMessage(error, 'No se pudo crear el músculo.'));
    } finally {
      dispatch(setAdminMusclesLoading(false));
    }
  },
);

// Edit muscle
export const editMuscle = createAsyncThunk<MuscleDto, { id: string; data: UpsertMuscleRequest }, AdminMusclesThunkConfig>(
  'muscles/edit',
  async ({ id, data }, { dispatch, getState, rejectWithValue }) => {
    try {
      dispatch(setAdminMusclesLoading(true));
      const updated = await updateMuscle(id, data);
      const newList = getState().adminMuscles.list.map((m) => (m.id === id ? updated : m));
      dispatch(setAdminMusclesList(newList));
      return updated;
    } catch (error) {
      dispatch(setAdminMusclesError(toErrorMessage(error, 'No se pudo actualizar el músculo.')));
      return rejectWithValue(toErrorMessage(error, 'No se pudo actualizar el músculo.'));
    } finally {
      dispatch(setAdminMusclesLoading(false));
    }
  },
);

// Delete muscle
export const deleteMuscle = createAsyncThunk<string, string, AdminMusclesThunkConfig>(
  'muscles/delete',
  async (id, { dispatch, getState, rejectWithValue }) => {
    try {
      dispatch(setAdminMusclesLoading(true));
      await deleteMuscleApi(id);
      const newList = getState().adminMuscles.list.filter((m) => m.id !== id);
      dispatch(setAdminMusclesList(newList));
      return id;
    } catch (error) {
      dispatch(setAdminMusclesError(toErrorMessage(error, 'No se pudo eliminar el músculo.')));
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el músculo.'));
    } finally {
      dispatch(setAdminMusclesLoading(false));
    }
  },
);
