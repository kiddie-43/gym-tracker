import { createAction } from '@reduxjs/toolkit';

import type { MeasurementTypeDto, UpsertMeasurementTypeRequest } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { AdminMeasurementTypesFilters } from '../../states/adminMeasurementTypes/adminMeasurementTypesState';

export const setAdminMeasurementTypesList = createAction<MeasurementTypeDto[]>('adminMeasurementTypes/setList');
export const setAdminMeasurementTypesForm = createAction<UpsertMeasurementTypeRequest>('adminMeasurementTypes/setForm');
export const setAdminMeasurementTypesFilters = createAction<AdminMeasurementTypesFilters>('adminMeasurementTypes/setFilters');
export const setAdminMeasurementTypesLoading = createAction<boolean>('adminMeasurementTypes/setLoading');
export const setAdminMeasurementTypesError = createAction<string | null>('adminMeasurementTypes/setError');
export const setAdminMeasurementTypesPopUpCode = createAction<string | null>('adminMeasurementTypes/setPopUpCode');
export const resetAdminMeasurementTypes = createAction('adminMeasurementTypes/reset');
