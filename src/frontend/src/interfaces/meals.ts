import type { MealItemInput } from './diets';

export type CreateMealLogRequest = {
  loggedDate: string;
  slotType: string;
  items: MealItemInput[];
};

export type MealLog = {
  id: string;
  loggedDate: string;
  slotType: string;
  items: MealItemInput[];
  totalCalories?: number | null;
};
