import type { IMonthlyPlan } from '../../../interfaces/monthlyPlan/IMonthlyPlan';
import type { RootState } from '../../store';

export interface IMonthlyPlanState {
  plan: IMonthlyPlan | null;
  lastConfirmedPlan: IMonthlyPlan | null;
  selectedWeek: number;
  selectedDay: number;
  loading: boolean;
  saving: boolean;
  error: string | null;
  degradedMode: boolean;
}

export const monthlyPlanInitialState: IMonthlyPlanState = {
  plan: null,
  lastConfirmedPlan: null,
  selectedWeek: 1,
  selectedDay: 1,
  loading: false,
  saving: false,
  error: null,
  degradedMode: false,
};

export const selectMonthlyPlanState = (state: RootState) => state.monthlyPlan;
