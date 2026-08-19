import { PopUpCode } from '../../../enums/popUp/popUp';
import type { IExercise, IExercisesFilter } from '../../../interfaces/IExercises/IExercises';
import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';


export const exercisesInitialState: IReduxState<IExercisesFilter, IExercise> = {
  table: {
    items: [],
    page: 0,
    totalCount: 0,
    pageSize: 10,
    sortBy: '',
    sortDirection: 'asc',
    selectedIds: [],
  },
  filters: {
    code: '',
    name: '',
    primaryMuscleId: [],
      secondaryMuscleId: [],
      unitId: [],

  },
  form: {
    name: '',
    code: '',
    description: '',
    exerciseType: 'STRENGTH',


    units: [],
    primaryMuscles: [],
    secondaryMuscles: [],
    id: ''
  },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: PopUpCode.Default,
  message: null,
};

export const selectExercisesState = (state: RootState) => state.exercises;
export const selectExercicesForm = (state: RootState) => state.exercises.form;