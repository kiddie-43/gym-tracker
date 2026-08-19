import type { UserProfile } from '../../../interfaces/preferences/preferences';
import type { RootState } from '../../store';

export type PreferencesState = {
  themeMode: 'light' | 'dark';
  profile: UserProfile;
  headerTitle: string;
  headerShowBackButton: boolean;
  headerBackRequestToken: number;
};

function getInitialThemeMode(): 'light' | 'dark' {
  if (typeof window === 'undefined' || !window.matchMedia) return 'light';
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

export const preferencesInitialState: PreferencesState = {
  themeMode: getInitialThemeMode(),
  profile: { firstName: '', lastName: '', email: '', photoUrl: '' },
  headerTitle: '',
  headerShowBackButton: false,
  headerBackRequestToken: 0,
};

export const selectPreferencesState = (state: RootState) => state.preferences;
