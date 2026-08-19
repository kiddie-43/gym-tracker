import type { TFunction } from 'i18next';

const weekDayTranslationKeyByCode: Record<number, string> = {
  1: 'monday',
  2: 'tuesday',
  3: 'wednesday',
  4: 'thursday',
  5: 'friday',
  6: 'saturday',
  7: 'sunday',
};

export function getWeekDayTranslationKey(dayCode: number): string {
  return weekDayTranslationKeyByCode[dayCode] ?? '';
}

export function translateWeekDayCode(dayCode: number, t: TFunction): string {
  const key = getWeekDayTranslationKey(dayCode);
  if (!key) {
    return String(dayCode);
  }

  return t(`common.daysOfWeek.${key}`);
}
