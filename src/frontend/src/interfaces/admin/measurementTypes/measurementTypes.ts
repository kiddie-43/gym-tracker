export interface IMeasurementType {
  id?: string;
  code: string;
  name: string;
  description?: string | null;
  isDeleted?: boolean;
}

export interface IMeasurementTypesFilter {
  includeDeleted?: boolean;
  search?: string;
  code?: string;
  sortBy?: 'code' | 'name' | 'description';
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
  code: string;
  name: string;
  description?: string | null;
}

export interface IImportMeasurementTypeCsvRowRequest {
  code?: string;
  name?: string;
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
    code?: string | null;
    created: boolean;
    reason?: string | null;
    measurementType?: IMeasurementType | null;
  }>;
}

