import type { AdminEntityBase } from '../common/common';

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
  measurementTypeCode?: string;
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
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
