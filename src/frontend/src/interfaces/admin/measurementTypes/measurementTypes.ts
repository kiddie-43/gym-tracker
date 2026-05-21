export type MeasurementTypeDto = {
  id: string;
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  description?: string | null;
  active: boolean;
  isDeleted: boolean;
  deletedAt?: string | null;
};

export type MeasurementTypesListQuery = {
  includeInactive?: boolean;
  search?: string;
  code?: string;
  sortBy?: 'name' | 'category' | 'key' | 'description';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
};

export type MeasurementTypesPageDto = {
  items: MeasurementTypeDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type AssignableMeasurementTypeDto = {
  id: string;
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  metrics: string[];
};

export type ImportMeasurementTypeCsvRowRequest = {
  key: string;
  name: string;
  unit: string;
  dataType: string;
  category: string;
  description?: string | null;
};

export type ImportMeasurementTypesRequest = {
  rows: ImportMeasurementTypeCsvRowRequest[];
};

export type ImportMeasurementTypeCsvRowResult = {
  rowNumber: number;
  key?: string | null;
  created: boolean;
  reason?: string | null;
  measurementType?: MeasurementTypeDto | null;
};

export type ImportMeasurementTypesResult = {
  totalRows: number;
  createdRows: number;
  rejectedRows: number;
  rows: ImportMeasurementTypeCsvRowResult[];
};

export type UpsertMeasurementTypeRequest = {
  name: string;
  key?: string;
  unit?: string;
  dataType?: string;
  category?: string;
  description?: string | null;
};
