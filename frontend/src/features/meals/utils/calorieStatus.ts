export type CalorieStatus = 'disabled' | 'unknown' | 'known';

export function resolveCalorieStatus(calorieTrackingEnabled: boolean, totalCalories?: number | null): CalorieStatus {
  if (!calorieTrackingEnabled) {
    return 'disabled';
  }

  return totalCalories == null ? 'unknown' : 'known';
}
