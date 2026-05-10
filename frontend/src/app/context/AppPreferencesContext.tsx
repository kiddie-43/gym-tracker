import { createContext, useContext, useMemo, useState } from 'react';

import type { PaletteMode } from '@mui/material';

export type UserProfile = {
  firstName: string;
  lastName: string;
  email: string;
  birthDate: string;
  photoUrl: string;
};

type AppPreferencesContextValue = {
  themeMode: PaletteMode;
  toggleThemeMode: () => void;
  profile: UserProfile;
  updateProfile: (profile: UserProfile) => void;
};

const emptyProfile: UserProfile = {
  firstName: '',
  lastName: '',
  email: '',
  birthDate: '',
  photoUrl: '',
};

const AppPreferencesContext = createContext<AppPreferencesContextValue | undefined>(undefined);

type AppPreferencesProviderProps = {
  children: React.ReactNode;
};

export function AppPreferencesProvider({ children }: AppPreferencesProviderProps) {
  const [themeMode, setThemeMode] = useState<PaletteMode>('light');
  const [profile, setProfile] = useState<UserProfile>(emptyProfile);

  const value = useMemo<AppPreferencesContextValue>(
    () => ({
      themeMode,
      toggleThemeMode: () => setThemeMode((current) => (current === 'light' ? 'dark' : 'light')),
      profile,
      updateProfile: setProfile,
    }),
    [profile, themeMode],
  );

  return <AppPreferencesContext.Provider value={value}>{children}</AppPreferencesContext.Provider>;
}

export function useAppPreferences() {
  const context = useContext(AppPreferencesContext);

  if (!context) {
    throw new Error('useAppPreferences must be used within AppPreferencesProvider');
  }

  return context;
}