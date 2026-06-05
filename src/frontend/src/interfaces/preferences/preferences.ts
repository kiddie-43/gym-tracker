export interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  photoUrl: string;
}

export interface AppPreferences {
  profile: UserProfile;
  themeMode: 'light' | 'dark';
  headerTitle: string;
}
