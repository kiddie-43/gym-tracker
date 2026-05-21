import type { AdminEntityBase } from '../common/common';

export type MuscleDto = AdminEntityBase & {
  muscleGroupIds: string[];
  deletedAt?: string | null;
};

export type UpsertMuscleRequest = {
  name: string;
  code: string;
  description?: string | null;
  active: boolean;
  muscleGroupIds: string[];
};

export type ImportMuscleCsvRowRequest = {
  code?: string;
  description?: string;
};

export type ImportMusclesRequest = {
  rows: ImportMuscleCsvRowRequest[];
};

export type ImportMuscleCsvRowResult = {
  rowNumber: number;
  code?: string;
  imported: boolean;
  reason?: string;
  muscle?: MuscleDto;
};

export type ImportMusclesResult = {
  totalRows: number;
  importedRows: number;
  rejectedRows: number;
  results: ImportMuscleCsvRowResult[];
};
