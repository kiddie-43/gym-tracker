import { ITableFilters } from "../skeleton/IPaginated/IPaginated";

export interface ITrainingMetricLog {
  id?: string;
  weekNumber?: number;
  dayNumber?: number;
  exerciseId?: string;
  exerciseCode?: string;
  metricId?: string;
  groupId?: string;
  timestamp?: string;
  value?: number;
  unitValues?: Record<string, number>;
  metrics?: Array<{
    id?: string;
    code?: string;
    unitCode?: string;
    value: number;
  }>;
}

export interface ITrainingLogMetricFilter extends ITableFilters {
weekNumber?: number;
dayNumber?: number;
exerciseId?: string;
exerciseCode?: string;
}