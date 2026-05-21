export interface IMuscle {
  id?: string;
  name: string;
  code: string;
  description?: string | null;
  active: boolean;
  isDeleted?: boolean;
  muscleGroupIds: string[];
  deletedAt?: string | null;
}

export interface IMusclesFilter {
  includeDeleted?: boolean;
  search?: string;
  code?: string;
  name?: string;
  sortBy?: 'code' | 'name' | 'description';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export interface IMuscles {
  items: IMuscle[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface IUpsertMuscleRequest {
  name: string;
  code: string;
  description?: string | null;
  active: boolean;
  muscleGroupIds: string[];
}

export interface IImportMuscleCsvRowRequest {
  name?: string;
  code?: string;
  description?: string;
}

export interface IImportMusclesRequest {
  rows: IImportMuscleCsvRowRequest[];
}

export interface IImportMuscleCsvRowResult {
  rowNumber: number;
  code?: string;
  imported: boolean;
  reason?: string;
  muscle?: IMuscle;
}

export interface IImportMusclesResult {
  totalRows: number;
  importedRows: number;
  rejectedRows: number;
  results: IImportMuscleCsvRowResult[];
}