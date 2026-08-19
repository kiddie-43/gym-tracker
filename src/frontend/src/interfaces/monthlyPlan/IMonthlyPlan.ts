import type { IWeekPlan } from './IWeekPlan';

export interface IMonthlyPlan {
  id: string;
  activeDays: number;
  weeks: IWeekPlan[];
}

export interface IMonthlyPlanExerciseRequest {
  exerciseId: string;
  orderIndex: number;
}

export interface IMonthlyPlanDayRequest {
  weekNumber: number;
  dayNumber: number;
  exercises: IMonthlyPlanExerciseRequest[];
}

export interface IUpsertMonthlyPlanRequest {
  activeDays: number;
  days: IMonthlyPlanDayRequest[];
}

export interface ILinkMonthlyPlanExerciseRequest {
  weekId: number;
  dayId: number;
  exerciseId: string;
}

export interface ILinkMonthlyPlanExerciseResponse {
  id: string;
  message: string;
}

export interface IUpdatePlannedExerciseRequest {
  exerciseId: string;
}
