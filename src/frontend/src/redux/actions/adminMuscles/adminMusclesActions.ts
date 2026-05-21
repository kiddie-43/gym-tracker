import { createAction } from '@reduxjs/toolkit';

import type { IMuscle, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';
import type { AdminMusclesFilters } from '../../states/adminMuscles/adminMusclesState';

export const setAdminMusclesList = createAction<IMuscle[]>('adminMuscles/setList');
export const setAdminMusclesForm = createAction<IUpsertMuscleRequest>('adminMuscles/setForm');
export const setAdminMusclesFilters = createAction<AdminMusclesFilters>('adminMuscles/setFilters');
export const setAdminMusclesLoading = createAction<boolean>('adminMuscles/setLoading');
export const setAdminMusclesError = createAction<string | null>('adminMuscles/setError');
export const setAdminMusclesPopUpCode = createAction<string | null>('adminMuscles/setPopUpCode');
export const resetAdminMuscles = createAction('adminMuscles/reset');
