import type { AdminEntityBase } from '../common/common';
import type { MuscleDto } from '../muscles/muscles';

export type ExerciseMediaType = 0 | 1;

export type ExerciseMediaDto = {
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
};

export type ExerciseDto = AdminEntityBase & {
  category: string;
  difficulty: string;
  measurementTypeId: string;
  measurementTypeName: string;
  measurementTypeCode?: string;
  primaryMuscles: MuscleDto[];
  secondaryMuscles: MuscleDto[];
  images: string[];
  videos: string[];
  deletedAt?: string | null;
};

export type UpsertExerciseRequest = {
  name: string;
  code: string;
  description?: string | null;
  category: string;
  difficulty: string;
  measurementTypeId: string;
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
  images?: string[];
  videos?: string[];
};

export type ImportExerciseCsvRowRequest = {
  code?: string;
  name?: string;
  description?: string | null;
  category?: string;
  difficulty?: string;
  measurementTypeId?: string;
  primaryMuscleIds?: string[];
  secondaryMuscleIds?: string[];
};

export type ImportExercisesRequest = {
  rows: ImportExerciseCsvRowRequest[];
};

export type ImportExerciseCsvRowResult = {
  rowNumber: number;
  code?: string;
  created: boolean;
  reason?: string;
  exercise?: ExerciseDto | null;
};

export type ImportExercisesResult = {
  totalRows: number;
  createdRows: number;
  rejectedRows: number;
  rows: ImportExerciseCsvRowResult[];
};

export type ExercisesListQuery = {
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
};

export type ExercisesPageDto = {
  items: ExerciseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type RequestUploadUrlRequest = {
  mediaType: ExerciseMediaType;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  title?: string | null;
};

export type MediaUploadTicket = {
  exerciseId: string;
  mediaId: string;
  storagePath: string;
  uploadUrl: string;
  expiresAt: string;
  contentType: string;
  maxSizeBytes: number;
};

export type ConfirmExerciseMediaRequest = {
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
};

export type ReorderExerciseMediaRequest = {
  items: { mediaId: string; sortOrder: number }[];
};

export type SetPrimaryExerciseMediaRequest = {
  mediaId: string;
};

export type UpdateExerciseMediaRequest = {
  title?: string | null;
  active?: boolean | null;
};
