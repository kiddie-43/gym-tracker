export interface IPaginated<T> {
  items: T[];
  totalCount: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  selectedIds?: string[];
}

export interface ITableFilters {
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}