import { configureStore } from '@reduxjs/toolkit';

import { adminExercisesReducer } from './reducers/adminExercises/adminExercisesReducer';
import { adminMeasurementTypesReducer } from './reducers/adminMeasurementTypes/adminMeasurementTypesReducer';
import { adminMusclesReducer } from './reducers/adminMuscles/adminMusclesReducer';
import { preferencesReducer } from './reducers/preferences/preferencesReducer';
import { routinesReducer } from './reducers/routines/routinesReducer';
import { workoutsReducer } from './states/workouts/workoutsState';

export const store = configureStore({
  reducer: {
    preferences: preferencesReducer,
    adminMuscles: adminMusclesReducer,
    adminMeasurementTypes: adminMeasurementTypesReducer,
    adminExercises: adminExercisesReducer,
    routines: routinesReducer,
    workouts: workoutsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
