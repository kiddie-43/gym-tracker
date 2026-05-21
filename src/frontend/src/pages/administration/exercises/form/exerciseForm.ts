export type ExerciseFormState = {
  id: string | null;
  name: string;
  code: string;
  description: string;
  category: string;
  difficulty: string;
  measurementTypeIds: string[];
  primaryMuscleIds: string[];
  secondaryMuscleIds: string[];
};

export const defaultExerciseFormState: ExerciseFormState = {
  id: null,
  name: '',
  code: '',
  description: '',
  category: '',
  difficulty: '',
  measurementTypeIds: [],
  primaryMuscleIds: [],
  secondaryMuscleIds: [],
};
