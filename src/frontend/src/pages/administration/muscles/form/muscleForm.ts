export type MuscleFormState = {
  id: string | null;
  name: string;
  code: string;
  description: string;
};

export const defaultMuscleFormState: MuscleFormState = {
  id: null,
  name: '',
  code: '',
  description: '',
};
