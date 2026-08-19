export type MuscleGroup = {
  id: string;
  name: string;
};

export type Exercise = {
  id: string;
  name: string;
  muscleGroupIds: string[];
  imageUrl?: string | null;
  formTypeId?: string;
  formTypeCode?: string;
};

export type Food = {
  id: string;
  name: string;
  calories?: number | null;
};

export type CatalogAvailability = {
  isStale: boolean;
  lastUpdatedAt: string;
};
