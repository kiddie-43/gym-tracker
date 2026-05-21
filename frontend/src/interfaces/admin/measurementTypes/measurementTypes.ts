export type MeasurementFieldDto = {
  name: string;
};

export type MeasurementTypeDto = {
  id: string;
  name: string;
  fields: MeasurementFieldDto[];
  active: boolean;
  isDeleted: boolean;
  deletedAt?: string | null;
};

export type UpsertMeasurementTypeRequest = {
  name: string;
  fields: MeasurementFieldDto[];
};
