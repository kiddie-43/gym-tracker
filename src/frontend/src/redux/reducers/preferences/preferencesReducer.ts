import { createReducer } from '@reduxjs/toolkit';

import {
  requestPreferencesHeaderBack,
  setPreferencesHeaderShowBackButton,
  setPreferencesHeaderTitle,
  setPreferencesProfile,
  setPreferencesThemeMode,
} from '../../actions/preferences/preferencesActions';
import { preferencesInitialState } from '../../states/preferences/preferencesState';

export const preferencesReducer = createReducer(preferencesInitialState, (builder) => {
  builder
    .addCase(setPreferencesThemeMode, (state, action) => {
      state.themeMode = action.payload;
    })
    .addCase(setPreferencesProfile, (state, action) => {
      state.profile = action.payload;
    })
    .addCase(setPreferencesHeaderTitle, (state, action) => {
      state.headerTitle = action.payload;
    })
    .addCase(setPreferencesHeaderShowBackButton, (state, action) => {
      state.headerShowBackButton = action.payload;
    })
    .addCase(requestPreferencesHeaderBack, (state) => {
      state.headerBackRequestToken += 1;
    });
});
