import { ITableFilters } from "../skeleton/IPaginated/IPaginated";

export interface IMuscle {
  id?: string;
  name: string;
  code: string;
  description?: string | null;
  muscleGroupIds?: string[];
}

export interface IMusclesFilter extends ITableFilters {
  code?: string;
  name?: string;
}