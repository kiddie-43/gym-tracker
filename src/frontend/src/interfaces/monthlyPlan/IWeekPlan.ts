export interface IMonthlyPlanExercise {
  id: string;
  exerciseId: string;
  orderIndex: number;
  exerciseName?: string | null;
  exerciseType?: string | null;
  primaryMuscles?: string[] | null;
}

export interface IDayPlan {
  weekNumber: number;
  dayNumber: number;
  status: string;
  isTruncated: boolean;
  exercises: IMonthlyPlanExercise[];
}

export interface IWeekPlan {
  weekNumber: number;
  days: IDayPlan[];
}
