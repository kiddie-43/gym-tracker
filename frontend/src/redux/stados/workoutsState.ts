import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit';

import type { RootState } from '../globalState';
import type { Exercise } from '../../shared/types/catalog';
import type { CreateWorkoutRequest, WorkoutSummary } from '../../shared/types/workouts';
import {
  createWorkout,
  deleteWorkout,
  getWorkout,
  getWorkoutHistory,
  listCatalogExercises,
  updateWorkout,
} from '../../services/workouts/workoutsApi';

type WorkoutStatus = 'completed' | 'incomplete';

type SetDraft = {
  repetitions: string;
  weight: string;
};

type WorkoutFormDraft = {
  selectedExerciseId: string | null;
  inputValue: string;
  performedAt: string;
  status: WorkoutStatus;
  setEntries: SetDraft[];
};

type WorkoutsState = {
  from: string;
  to: string;
  items: WorkoutSummary[];
  exercises: Exercise[];
  selectedWorkout: WorkoutSummary | null;
  isCreateDialogOpen: boolean;
  isEditDialogOpen: boolean;
  isDeleteDialogOpen: boolean;
  isLoadingHistory: boolean;
  isLoadingWorkout: boolean;
  isDeleting: boolean;
  isSubmitting: boolean;
  historyError: string | null;
  createError: string | null;
  editError: string | null;
  deleteError: string | null;
  formDraft: WorkoutFormDraft;
};

function getDefaultPerformedAt() {
  const now = new Date();
  now.setSeconds(0, 0);
  const localTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
  return localTime.toISOString().slice(0, 16);
}

function createDefaultFormDraft(): WorkoutFormDraft {
  return {
    selectedExerciseId: null,
    inputValue: '',
    performedAt: getDefaultPerformedAt(),
    status: 'completed',
    setEntries: [{ repetitions: '8', weight: '100' }],
  };
}

function createDraftFromWorkout(workout: WorkoutSummary): WorkoutFormDraft {
  const firstEntry = workout.exerciseEntries?.[0];

  return {
    selectedExerciseId: null,
    inputValue: firstEntry?.exerciseNameSnapshot ?? firstEntry?.exerciseName ?? '',
    performedAt: workout.performedAt ? new Date(workout.performedAt).toISOString().slice(0, 16) : getDefaultPerformedAt(),
    status: workout.status,
    setEntries: firstEntry?.sets && firstEntry.sets.length > 0
      ? firstEntry.sets.map((setEntry) => ({
          repetitions: String(setEntry.repetitions),
          weight: String(setEntry.weight ?? 0),
        }))
      : [{ repetitions: '8', weight: '100' }],
  };
}

function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}

const initialState: WorkoutsState = {
  from: '2026-05-01',
  to: '2026-05-31',
  items: [],
  exercises: [],
  selectedWorkout: null,
  isCreateDialogOpen: false,
  isEditDialogOpen: false,
  isDeleteDialogOpen: false,
  isLoadingHistory: false,
  isLoadingWorkout: false,
  isDeleting: false,
  isSubmitting: false,
  historyError: null,
  createError: null,
  editError: null,
  deleteError: null,
  formDraft: createDefaultFormDraft(),
};

export const loadCatalogExercises = createAsyncThunk<Exercise[], void, { rejectValue: string }>(
  'workouts/loadCatalogExercises',
  async (_, { rejectWithValue }) => {
    try {
      return await listCatalogExercises();
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'No se pudieron cargar los ejercicios.'));
    }
  },
);

export const loadWorkoutHistory = createAsyncThunk<
  { items: WorkoutSummary[]; totalCount: number; page: number; pageSize: number },
  void,
  { state: RootState; rejectValue: string }
>('workouts/loadWorkoutHistory', async (_, { getState, rejectWithValue }) => {
  const { from, to } = getState().workouts;

  try {
    return await getWorkoutHistory(1, 20, from, to);
  } catch (error) {
    return rejectWithValue(getErrorMessage(error, 'Error al cargar el historial de entrenamientos.'));
  }
});

export const startEditWorkout = createAsyncThunk<WorkoutSummary, string, { rejectValue: string }>(
  'workouts/startEditWorkout',
  async (workoutId, { rejectWithValue }) => {
    try {
      return await getWorkout(workoutId);
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'Error al cargar el entrenamiento.'));
    }
  },
);

export const submitCreateWorkout = createAsyncThunk<void, CreateWorkoutRequest, { state: RootState; rejectValue: string }>(
  'workouts/submitCreateWorkout',
  async (request, { dispatch, rejectWithValue }) => {
    try {
      await createWorkout(request);
      await dispatch(loadWorkoutHistory()).unwrap();
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'Error al crear el entrenamiento.'));
    }
  },
);

export const submitUpdateWorkout = createAsyncThunk<
  void,
  { workoutId: string; request: CreateWorkoutRequest },
  { state: RootState; rejectValue: string }
>('workouts/submitUpdateWorkout', async ({ workoutId, request }, { dispatch, rejectWithValue }) => {
  try {
    await updateWorkout(workoutId, request);
    await dispatch(loadWorkoutHistory()).unwrap();
  } catch (error) {
    return rejectWithValue(getErrorMessage(error, 'Error al actualizar el entrenamiento.'));
  }
});

export const confirmDeleteWorkout = createAsyncThunk<void, string, { state: RootState; rejectValue: string }>(
  'workouts/confirmDeleteWorkout',
  async (workoutId, { dispatch, rejectWithValue }) => {
    try {
      await deleteWorkout(workoutId);
      await dispatch(loadWorkoutHistory()).unwrap();
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'Error al borrar el entrenamiento.'));
    }
  },
);

const workoutsSlice = createSlice({
  name: 'workouts',
  initialState,
  reducers: {
    setFrom(state, action: PayloadAction<string>) {
      state.from = action.payload;
    },
    setTo(state, action: PayloadAction<string>) {
      state.to = action.payload;
    },
    openCreateDialog(state) {
      state.isCreateDialogOpen = true;
      state.createError = null;
      state.formDraft = createDefaultFormDraft();
    },
    closeCreateDialog(state) {
      state.isCreateDialogOpen = false;
      state.createError = null;
    },
    closeEditDialog(state) {
      state.isEditDialogOpen = false;
      state.editError = null;
      state.selectedWorkout = null;
    },
    openDeleteDialog(state, action: PayloadAction<WorkoutSummary>) {
      state.selectedWorkout = action.payload;
      state.isDeleteDialogOpen = true;
      state.deleteError = null;
    },
    closeDeleteDialog(state) {
      state.isDeleteDialogOpen = false;
      state.deleteError = null;
    },
    setSelectedExerciseId(state, action: PayloadAction<string | null>) {
      state.formDraft.selectedExerciseId = action.payload;
    },
    setFormInputValue(state, action: PayloadAction<string>) {
      state.formDraft.inputValue = action.payload;
    },
    setFormPerformedAt(state, action: PayloadAction<string>) {
      state.formDraft.performedAt = action.payload;
    },
    setFormStatus(state, action: PayloadAction<WorkoutStatus>) {
      state.formDraft.status = action.payload;
    },
    addFormSetEntry(state) {
      const current = state.formDraft.setEntries;
      if (current.length >= 20) {
        return;
      }

      const last = current[current.length - 1] ?? { repetitions: '8', weight: '100' };
      current.push({ repetitions: last.repetitions, weight: last.weight });
    },
    removeFormSetEntry(state, action: PayloadAction<number>) {
      if (state.formDraft.setEntries.length <= 1) {
        return;
      }

      state.formDraft.setEntries = state.formDraft.setEntries.filter((_, index) => index !== action.payload);
    },
    updateFormSetEntry(
      state,
      action: PayloadAction<{ index: number; field: keyof SetDraft; value: string }>,
    ) {
      const entry = state.formDraft.setEntries[action.payload.index];
      if (!entry) {
        return;
      }

      entry[action.payload.field] = action.payload.value;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loadCatalogExercises.fulfilled, (state, action) => {
        state.exercises = action.payload;
      })
      .addCase(loadCatalogExercises.rejected, (state) => {
        state.exercises = [];
      })
      .addCase(loadWorkoutHistory.pending, (state) => {
        state.isLoadingHistory = true;
        state.historyError = null;
      })
      .addCase(loadWorkoutHistory.fulfilled, (state, action) => {
        state.isLoadingHistory = false;
        state.items = action.payload.items;
      })
      .addCase(loadWorkoutHistory.rejected, (state, action) => {
        state.isLoadingHistory = false;
        state.items = [];
        state.historyError = action.payload ?? 'Error al cargar el historial de entrenamientos.';
      })
      .addCase(startEditWorkout.pending, (state) => {
        state.isLoadingWorkout = true;
        state.editError = null;
      })
      .addCase(startEditWorkout.fulfilled, (state, action) => {
        state.isLoadingWorkout = false;
        state.selectedWorkout = action.payload;
        state.formDraft = createDraftFromWorkout(action.payload);
        state.isEditDialogOpen = true;
      })
      .addCase(startEditWorkout.rejected, (state, action) => {
        state.isLoadingWorkout = false;
        state.editError = action.payload ?? 'Error al cargar el entrenamiento.';
      })
      .addCase(submitCreateWorkout.pending, (state) => {
        state.isSubmitting = true;
        state.createError = null;
      })
      .addCase(submitCreateWorkout.fulfilled, (state) => {
        state.isSubmitting = false;
        state.isCreateDialogOpen = false;
        state.formDraft = createDefaultFormDraft();
      })
      .addCase(submitCreateWorkout.rejected, (state, action) => {
        state.isSubmitting = false;
        state.createError = action.payload ?? 'Error al crear el entrenamiento.';
      })
      .addCase(submitUpdateWorkout.pending, (state) => {
        state.isSubmitting = true;
        state.editError = null;
      })
      .addCase(submitUpdateWorkout.fulfilled, (state) => {
        state.isSubmitting = false;
        state.isEditDialogOpen = false;
        state.selectedWorkout = null;
        state.formDraft = createDefaultFormDraft();
      })
      .addCase(submitUpdateWorkout.rejected, (state, action) => {
        state.isSubmitting = false;
        state.editError = action.payload ?? 'Error al actualizar el entrenamiento.';
      })
      .addCase(confirmDeleteWorkout.pending, (state) => {
        state.isDeleting = true;
        state.deleteError = null;
      })
      .addCase(confirmDeleteWorkout.fulfilled, (state) => {
        state.isDeleting = false;
        state.isDeleteDialogOpen = false;
        state.selectedWorkout = null;
      })
      .addCase(confirmDeleteWorkout.rejected, (state, action) => {
        state.isDeleting = false;
        state.deleteError = action.payload ?? 'Error al borrar el entrenamiento.';
      });
  },
});

export const {
  setFrom,
  setTo,
  openCreateDialog,
  closeCreateDialog,
  closeEditDialog,
  openDeleteDialog,
  closeDeleteDialog,
  setSelectedExerciseId,
  setFormInputValue,
  setFormPerformedAt,
  setFormStatus,
  addFormSetEntry,
  removeFormSetEntry,
  updateFormSetEntry,
} = workoutsSlice.actions;

export const workoutsReducer = workoutsSlice.reducer;
