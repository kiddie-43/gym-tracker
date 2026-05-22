export interface IMeasurementType {
  id?: string;
  code?: string;
  name: string;
  unit?: string;
  dataType?: string;
  category?: string;
  description?: string | null;
  active: boolean;
  isDeleted?: boolean;
  deletedAt?: string | null;
}

export interface IMeasurementTypesFilter {
  includeDeleted?: boolean;
  search?: string;
  code?: string;
  sortBy?: 'name' | 'category' | 'key' | 'description';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export interface IMeasurementTypes {
  items: IMeasurementType[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface IAssignableMeasurementType {
  id: string;
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  metrics: string[];
}

export interface IImportMeasurementTypeCsvRowRequest {
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  description?: string | null;
}

export interface IImportMeasurementTypesRequest {
  rows: IImportMeasurementTypeCsvRowRequest[];
}

export interface IImportMeasurementTypesResult {
  totalRows: number;
  createdRows: number;
  rejectedRows: number;
  rows: Array<{
    rowNumber: number;
    key?: string | null;
    created: boolean;
    reason?: string | null;
    measurementType?: IMeasurementType | null;
  }>;
}

