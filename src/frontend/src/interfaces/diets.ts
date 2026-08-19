export type DietSummary = {
  id: string;
  name: string;
  dayCount: number;
};

export type MealItemInput = {
  externalFoodId: string;
  quantity: number;
  unit: string;
  calories?: number | null;
};

export type MealSlotInput = {
  slotType: string;
  items: MealItemInput[];
};

export type DietDayInput = {
  dayKey: string;
  mealSlots: MealSlotInput[];
};

export type CreateDietRequest = {
  name: string;
  days: DietDayInput[];
};

export type Diet = {
  id: string;
  name: string;
  days: DietDayInput[];
};
