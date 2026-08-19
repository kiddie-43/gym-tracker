import { en } from './lenguage/en';
import { es } from './lenguage/es';

export const resources = {
  es,
  en,
} as const;

export type SupportedLanguage = keyof typeof resources;