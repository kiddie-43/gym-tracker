export type MeasurementTypeFormState = {
  id: string | null;
  code: string;
  name: string;
  category: string;
  description: string;
};

export const defaultMeasurementTypeFormState: MeasurementTypeFormState = {
  id: null,
  code: '',
  name: '',
  category: 'general',
  description: '',
};
