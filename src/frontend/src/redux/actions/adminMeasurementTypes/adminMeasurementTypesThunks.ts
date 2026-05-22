import { createAsyncThunk } from '@reduxjs/toolkit';

import type {
  IImportMeasurementTypesRequest,
  IImportMeasurementTypesResult,
  IMeasurementType,
  IMeasurementTypes,
} from '../../../interfaces/admin/measurementTypes/measurementTypes';
import {
  createMeasurementType,
  deleteMeasurementType,
  getMeasurementTypeById as getMeasurementTypeByIdApi,
  importMeasurementTypesCsv as importMeasurementTypesCsvApi,
  listMeasurementTypesPage,
  reactivateMeasurementType,
  updateMeasurementType,
} from '../../../services/api/admin/measurementTypes/measurementTypesApi';
import type { RootState } from '../../store';

type AdminMeasurementTypesThunkConfig = {
  state: RootState;
  rejectValue: string;
};

function toErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

// Fetch paginated list — reads filters + table from Redux state
export const fetchAdminMeasurementTypes = createAsyncThunk<IMeasurementTypes, void, AdminMeasurementTypesThunkConfig>(
  'adminMeasurementTypes/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    const { filters, table } = getState().adminMeasurementTypes;
    try {
      return await listMeasurementTypesPage({
        includeDeleted: filters.includeDeleted,
        search: filters.search || undefined,
        code: filters.code || undefined,
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
export const submitAdminMeasurementTypeForm = createAsyncThunk<void, void, AdminMeasurementTypesThunkConfig>(
  'adminMeasurementTypes/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    const { form } = getState().adminMeasurementTypes;
    const request: IMeasurementType = {
      code: form.code.trim(),
      name: form.name.trim(),
      description: form.description?.trim() || null,
    };
    try {
      if (form.id) {
        await updateMeasurementType(form.id, request);
      } else {
        await createMeasurementType(request);
      }
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar el tipo de medición.'));
    }
  },
);

// Delete multiple measurement types by ID
export const deleteAdminMeasurementTypes = createAsyncThunk<
  { failedCount: number; total: number },
  string[],
  AdminMeasurementTypesThunkConfig
>(
  'adminMeasurementTypes/deleteMany',
  async (ids, { rejectWithValue }) => {
    try {
      const results = await Promise.allSettled(ids.map((id) => deleteMeasurementType(id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;
      return { failedCount, total: ids.length };
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo eliminar el tipo de medición.'));
    }
  },
);

// Reactivate a single measurement type
export const reactivateAdminMeasurementType = createAsyncThunk<IMeasurementType, string, AdminMeasurementTypesThunkConfig>(
  'adminMeasurementTypes/reactivate',
  async (id, { rejectWithValue }) => {
    try {
      return await reactivateMeasurementType(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo reactivar el tipo de medición.'));
    }
  },
);

// Import measurement types from CSV
export const importAdminMeasurementTypesCsv = createAsyncThunk<
  IImportMeasurementTypesResult,
  IImportMeasurementTypesRequest,
  AdminMeasurementTypesThunkConfig
>(
  'adminMeasurementTypes/importCsv',
  async (request, { rejectWithValue }) => {
    try {
      return await importMeasurementTypesCsvApi(request);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo importar el CSV.'));
    }
  },
);

// Get measurement type by ID
export const getMeasurementTypeById = createAsyncThunk<IMeasurementType | null, string, AdminMeasurementTypesThunkConfig>(
  'adminMeasurementTypes/getById',
  async (id, { rejectWithValue }) => {
    try {
      return await getMeasurementTypeByIdApi(id);
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo obtener el tipo de medición.'));
    }
  },
);
