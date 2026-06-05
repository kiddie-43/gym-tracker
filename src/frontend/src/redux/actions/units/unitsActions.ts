import { createAction, createAsyncThunk } from '@reduxjs/toolkit';

import { PopUpCode } from '../../../enums/popUp/popUp';
import type { RootState } from '../../store';
import { IUnit, IUnitFilters } from '../../../interfaces/admin/units/IUnit';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import {
  createUnit,
  deleteUnit,
  getUnitsById,
  importUnitsCsv as importUnitsCsvApi,
  listUnitsPage,
  reactivateUnit as reactivateUnitApi,
  updateUnit,
} from '../../../services/api/units/unitsApi';

type AdminUnitsThunkConfig = {
  state: RootState;
  rejectValue: string;
};

export const setUnitsTable = createAction<IPaginated<IUnit>>('units/setTable');
export const setUnitsFilters = createAction<IUnitFilters>('units/setFilters');
export const setUnitsForm = createAction<IUnit>('units/setForm');
export const setUnitsCsvResult = createAction<unknown | null>('units/setCsvResult');
export const setUnitsLoading = createAction<boolean>('units/setLoading');
export const setUnitsError = createAction<string | null>('units/setError');
export const setUnitsPopUpCode = createAction<PopUpCode>('units/setPopUpCode');
export const resetUnits = createAction('units/reset');

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

// Fetch paginated list — reads filters + pagination from Redux state
export const fetchUnits = createAsyncThunk<IPaginated<IUnit>, void, AdminUnitsThunkConfig>(
  'units/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    const { filters, table } = getState().units;
    try {
      const query: IUnitFilters = {
        code: filters.code,
        name: filters.name,
        description: filters.description,
        sortBy: table.sortBy,
        sortDirection: table.sortDirection,
        page: table.page,
        pageSize: table.pageSize,
      }

      return await listUnitsPage(query);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el listado.'));
    }
  },
);

// Submit form (create or edit) — reads form from Redux state
export const submitUnitForm = createAsyncThunk<void, void, AdminUnitsThunkConfig>(
  'units/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    const { form } = getState().units;
    const request: IUnit = {
      name: form.name.trim(),
      code: form.code.trim(),
      description: form.description?.trim() || null,
    };
    try {
      if (form.id) {
        await updateUnit(form.id, request);
      } else {
        await createUnit(request);
      }
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar la unidad.'));
    }
  },
);

// Delete multiple units by ID
export const deleteUnits = createAsyncThunk<{ failedCount: number; total: number }, string[], AdminUnitsThunkConfig>(
  'units/deleteMany',
  async (ids, { rejectWithValue }) => {
    try {
      const results = await Promise.allSettled(ids.map((id) => deleteUnit(id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;
      return { failedCount, total: ids.length };
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar la unidad.'));
    }
  },
);

// Reactivate a single unit
export const reactivateUnit = createAsyncThunk<IUnit, string, AdminUnitsThunkConfig>(
  'units/reactivate',
  async (id, { rejectWithValue }) => {
    try {
      return await reactivateUnitApi(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo reactivar la unidad.'));
    }
  },
);

// Import units from CSV
export const importUnitsCsv = createAsyncThunk<IUnit, string, AdminUnitsThunkConfig>(
  'units/importCsv',
  async (request, { rejectWithValue }) => {
    try {
      return await importUnitsCsvApi(request);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo importar el CSV.'));
    }
  },
);

// Get unit by ID
export const getUnitById = createAsyncThunk<IUnit, string, AdminUnitsThunkConfig>(
  'units/getById',
  async (id, { rejectWithValue }) => {
    try {
      return await getUnitsById(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo obtener la unidad.'));
    }
  },
);

