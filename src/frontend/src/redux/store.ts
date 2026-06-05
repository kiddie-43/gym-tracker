import { configureStore } from '@reduxjs/toolkit';

import { adminExercisesReducer } from './reducers/exercises/exercisesReducer';
import { exerciceLogsReducer } from './reducers/exerciceLogs/exerciceLogsReducer';
import { unitsReducer } from './reducers/units/units';
import { adminMusclesReducer } from './reducers/Muscles/musclesReducer';
import { preferencesReducer } from './reducers/preferences/preferencesReducer';
import { routinesReducer } from './reducers/routines/routinesReducer';
import { sessionsReducer } from './reducers/sessions/sessionReducer';
import { trainingMetricLogsReducer } from './reducers/trainingMetricLogs/trainingMetricLogsReducer';
import { workoutsReducer } from './states/workouts/workoutsState';

export const store = configureStore({
  reducer: {
    preferences: preferencesReducer,
    adminMuscles: adminMusclesReducer,
    units: unitsReducer,
    exercises: adminExercisesReducer,
    routines: routinesReducer,
    sessions: sessionsReducer,
    workouts: workoutsReducer,
    exerciceLogs: exerciceLogsReducer,
    trainingMetricLogs: trainingMetricLogsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
