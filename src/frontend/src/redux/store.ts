import { configureStore } from '@reduxjs/toolkit';

import { adminExercisesReducer } from './reducers/exercises/exercisesReducer';
import { unitsReducer } from './reducers/units/units';
import { adminMusclesReducer } from './reducers/Muscles/musclesReducer';
import { preferencesReducer } from './reducers/preferences/preferencesReducer';
import { trainingMetricLogsReducer } from './reducers/trainingMetricLogs/trainingMetricLogsReducer';
import { trainingSessionsReducer } from './reducers/trainingSessions/trainingSessionsReducer';
import { monthlyPlanReducer } from './reducers/monthlyPlan/monthlyPlanReducer';
import { workoutsReducer } from './states/workouts/workoutsState';

export const store = configureStore({
  reducer: {
    preferences: preferencesReducer,
    adminMuscles: adminMusclesReducer,
    units: unitsReducer,
    exercises: adminExercisesReducer,
    monthlyPlan: monthlyPlanReducer,
    workouts: workoutsReducer,
    trainingMetricLogs: trainingMetricLogsReducer,
    trainingSessions: trainingSessionsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
