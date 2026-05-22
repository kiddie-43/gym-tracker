import type { IMuscle } from '../../muscles/IMuscles';

export type ExerciseMediaType = 0 | 1;

export interface IExerciseMedia {
  mediaId: string;
  mediaType: ExerciseMediaType;
  title: string;
  storagePath: string;
  thumbnailPath?: string | null;
  contentType: string;
  fileName: string;
  sizeBytes: number;
  sortOrder: number;
  isPrimary: boolean;
  active: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
}

export interface IExercise {
  id?: string;
  name: string;
  code?: string;
  description?: string | null;
  category?: string;
  difficulty?: string;
  measurementTypeId?: string;
  measurementTypeName?: string;
  primaryMuscles: IMuscle[];
  secondaryMuscles: IMuscle[];
  images?: string[];
  videos?: string[];
  active?: boolean;
  isDeleted?: boolean;
  deletedAt?: string | null;
}

export interface IExercisesFilter {
  includeDeleted?: boolean;
  search?: string;
  difficulties?: string[];
  measurementTypeIds?: string[];
  primaryMuscleIds?: string[];
  secondaryMuscleIds?: string[];
  sortBy?: 'code' | 'name' | 'category' | 'difficulty';
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export interface IExercises {
  items: IExercise[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface IImportExerciseCsvRowRequest {
  code?: string;
  name?: string;
  description?: string | null;
  category?: string;
  difficulty?: string;
  measurementTypeId?: string;
  primaryMuscleIds?: string[];
  secondaryMuscleIds?: string[];
}

export interface IImportExercisesRequest {
  rows: IImportExerciseCsvRowRequest[];
}

export interface IImportExercisesResult {
  totalRows: number;
  createdRows: number;
  rejectedRows: number;
  rows: Array<{
    rowNumber: number;
    code?: string;
    created: boolean;
    reason?: string;
    exercise?: IExercise | null;
  }>;
}

export interface IRequestUploadUrlRequest {
  mediaType: ExerciseMediaType;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  title?: string | null;
}

export interface IMediaUploadTicket {
  exerciseId: string;
  mediaId: string;
  storagePath: string;
  uploadUrl: string;
  expiresAt: string;
  contentType: string;
  maxSizeBytes: number;
}

export interface IConfirmExerciseMediaRequest {
  mediaId: string;
  mediaType: ExerciseMediaType;
  storagePath: string;
  thumbnailPath?: string | null;
  contentType: string;
  fileName: string;
  sizeBytes: number;
  title?: string | null;
  sortOrder: number;
  isPrimary: boolean;
}

export interface IReorderExerciseMediaRequest {
  items: { mediaId: string; sortOrder: number }[];
}

export interface ISetPrimaryExerciseMediaRequest {
  mediaId: string;
}

export interface IUpdateExerciseMediaRequest {
  title?: string | null;
  active?: boolean | null;
}

