import { createAction } from '@reduxjs/toolkit';

import type { UserProfile } from '../../../interfaces/preferences/preferences';

export const setPreferencesThemeMode = createAction<'light' | 'dark'>('preferences/setThemeMode');
export const setPreferencesProfile = createAction<UserProfile>('preferences/setProfile');
export const setPreferencesHeaderTitle = createAction<string>('preferences/setHeaderTitle');
