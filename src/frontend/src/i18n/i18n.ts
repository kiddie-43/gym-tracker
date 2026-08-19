import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import { resources } from './resources';

void i18n.use(initReactI18next).init({
  resources,
  supportedLngs: ['es', 'en'],
  lng: 'es',
  fallbackLng: 'es',
  interpolation: {
    escapeValue: false,
  },
});

export { i18n };