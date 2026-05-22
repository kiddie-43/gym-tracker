import { createAction } from '@reduxjs/toolkit';

import type { IImportMeasurementTypesResult, IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { AdminMeasurementTypesFilters, AdminMeasurementTypesTable } from '../../states/adminMeasurementTypes/adminMeasurementTypesState';

export const setAdminMeasurementTypesTable = createAction<AdminMeasurementTypesTable>('adminMeasurementTypes/setTable');
export const setAdminMeasurementTypesFilters = createAction<AdminMeasurementTypesFilters>('adminMeasurementTypes/setFilters');
export const setAdminMeasurementTypesForm = createAction<IMeasurementType>('adminMeasurementTypes/setForm');
export const setAdminMeasurementTypesCsvResult = createAction<IImportMeasurementTypesResult | null>('adminMeasurementTypes/setCsvResult');
export const setAdminMeasurementTypesLoading = createAction<boolean>('adminMeasurementTypes/setLoading');
export const setAdminMeasurementTypesError = createAction<string | null>('adminMeasurementTypes/setError');
export const setAdminMeasurementTypesPopUpCode = createAction<string | null>('adminMeasurementTypes/setPopUpCode');
export const resetAdminMeasurementTypes = createAction('adminMeasurementTypes/reset');
