import { createAction } from '@reduxjs/toolkit';

import type { IImportMusclesResult, IMuscle } from '../../../interfaces/muscles/IMuscles';
import type {
  AdminMusclesFilters,
  AdminMusclesTable,
} from '../../states/adminMuscles/adminMusclesState';

export const setAdminMusclesTable = createAction<AdminMusclesTable>('adminMuscles/setTable');
export const setAdminMusclesFilters = createAction<AdminMusclesFilters>('adminMuscles/setFilters');
export const setAdminMusclesForm = createAction<IMuscle>('adminMuscles/setForm');
export const setAdminMusclesCsvResult = createAction<IImportMusclesResult | null>('adminMuscles/setCsvResult');
export const setAdminMusclesLoading = createAction<boolean>('adminMuscles/setLoading');
export const setAdminMusclesError = createAction<string | null>('adminMuscles/setError');
export const setAdminMusclesPopUpCode = createAction<string | null>('adminMuscles/setPopUpCode');
export const resetAdminMuscles = createAction('adminMuscles/reset');


