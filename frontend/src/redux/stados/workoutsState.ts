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

type NewWorkoutSetDraft = {
  id: string;
  weight: string;
  reps: string;
};

type NewWorkoutDraft = {
  workoutId: string | null;
  currentStep: 1 | 2 | 3;
  searchQuery: string;
  selectedExerciseId: string | null;
  activeSetId: string | null;
  favoriteIds: string[];
  performedAt: string;
  notes: string;
  sets: NewWorkoutSetDraft[];
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
  newWorkoutDraft: NewWorkoutDraft;
};

const FAVORITES_STORAGE_KEY = 'gym-tracker.favorite-exercises';

function createId() {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.round(Math.random() * 100_000)}`;
}

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

function createDefaultNewWorkoutSet(from?: NewWorkoutSetDraft): NewWorkoutSetDraft {
  return {
    id: createId(),
    weight: from?.weight ?? '0',
    reps: from?.reps ?? '8',
  };
}

function isPlaceholderSet(setEntry: NewWorkoutSetDraft | undefined): boolean {
  if (!setEntry) {
    return false;
  }

  return setEntry.weight === '0' && setEntry.reps === '8';
}

function readFavoriteIds(): string[] {
  if (typeof window === 'undefined') {
    return [];
  }

  try {
    const raw = window.localStorage.getItem(FAVORITES_STORAGE_KEY);
    if (!raw) {
      return [];
    }

    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed.filter((id): id is string => typeof id === 'string');
  } catch {
    return [];
  }
}

function persistFavoriteIds(ids: string[]) {
  if (typeof window === 'undefined') {
    return;
  }

  try {
    window.localStorage.setItem(FAVORITES_STORAGE_KEY, JSON.stringify(ids));
  } catch {
    // Ignore storage errors.
  }
}

function createDefaultNewWorkoutDraft(favoriteIds: string[] = []): NewWorkoutDraft {
  return {
    workoutId: null,
    currentStep: 1,
    searchQuery: '',
    selectedExerciseId: null,
    activeSetId: null,
    favoriteIds,
    performedAt: getDefaultPerformedAt(),
    notes: '',
    sets: [createDefaultNewWorkoutSet()],
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
  newWorkoutDraft: createDefaultNewWorkoutDraft(),
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

export const startEditWorkoutForm = createAsyncThunk<WorkoutSummary, string, { rejectValue: string }>(
  'workouts/startEditWorkoutForm',
  async (workoutId, { rejectWithValue }) => {
    try {
      return await getWorkout(workoutId);
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'Error al cargar el entrenamiento para editar.'));
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

export const submitNewWorkoutDraft = createAsyncThunk<void, void, { state: RootState; rejectValue: string }>(
  'workouts/submitNewWorkoutDraft',
  async (_, { dispatch, getState, rejectWithValue }) => {
    const { newWorkoutDraft, exercises } = getState().workouts;

    const selectedExercise = exercises.find((exercise) => exercise.id === newWorkoutDraft.selectedExerciseId);

    if (!selectedExercise) {
      return rejectWithValue('Debes seleccionar un ejercicio.');
    }

    const cleanSets = newWorkoutDraft.sets.filter((setEntry) => {
      return setEntry.weight.trim() || setEntry.reps.trim();
    });

    if (cleanSets.length === 0) {
      return rejectWithValue('Debes agregar al menos una serie.');
    }

    const mappedSets = [] as CreateWorkoutRequest['exerciseEntries'][0]['sets'];
    for (const setEntry of cleanSets) {
      const repetitions = Number(setEntry.reps);
      const weight = Number(setEntry.weight);

      if (!Number.isFinite(repetitions) || repetitions <= 0) {
        return rejectWithValue('Las repeticiones deben ser mayores que 0.');
      }

      if (!Number.isFinite(weight) || weight < 0) {
        return rejectWithValue('El peso debe ser un numero valido (0 o mayor).');
      }

      mappedSets.push({
        repetitions,
        weight,
        restSeconds: 120,
        completed: true,
      });
    }

    const performedAtDate = new Date(newWorkoutDraft.performedAt);
    const safePerformedAt = Number.isNaN(performedAtDate.getTime()) ? new Date() : performedAtDate;

    const request: CreateWorkoutRequest = {
      performedAt: safePerformedAt.toISOString(),
      status: 'completed',
      notes: newWorkoutDraft.notes,
      exerciseEntries: [
        {
          externalExerciseId: selectedExercise.id,
          exerciseName: selectedExercise.name,
          muscleGroupIds: selectedExercise.muscleGroupIds,
          imageUrl: selectedExercise.imageUrl,
          formTypeId: selectedExercise.formTypeId,
          formTypeCode: selectedExercise.formTypeCode,
          sets: mappedSets,
        },
      ],
    };

    try {
      if (newWorkoutDraft.workoutId) {
        await updateWorkout(newWorkoutDraft.workoutId, request);
      } else {
        await createWorkout(request);
      }
      await dispatch(loadWorkoutHistory()).unwrap();
    } catch (error) {
      return rejectWithValue(getErrorMessage(error, 'Error al guardar el entrenamiento.'));
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
    initNewWorkoutDraft(state) {
      state.newWorkoutDraft = createDefaultNewWorkoutDraft(readFavoriteIds());
      state.createError = null;
      state.editError = null;
    },
    setNewWorkoutStep(state, action: PayloadAction<1 | 2 | 3>) {
      state.newWorkoutDraft.currentStep = action.payload;
    },
    setNewWorkoutSearchQuery(state, action: PayloadAction<string>) {
      state.newWorkoutDraft.searchQuery = action.payload;
    },
    selectNewWorkoutExercise(state, action: PayloadAction<string>) {
      state.newWorkoutDraft.selectedExerciseId = action.payload;
      state.newWorkoutDraft.currentStep = 2;
      state.createError = null;
    },
    setNewWorkoutPerformedAt(state, action: PayloadAction<string>) {
      state.newWorkoutDraft.performedAt = action.payload;
    },
    setNewWorkoutNotes(state, action: PayloadAction<string>) {
      state.newWorkoutDraft.notes = action.payload;
    },
    setNewWorkoutActiveSet(state, action: PayloadAction<string | null>) {
      state.newWorkoutDraft.activeSetId = action.payload;
    },
    addNewWorkoutSet(
      state,
      action: PayloadAction<{ weight?: string; reps?: string } | undefined>,
    ) {
      const currentSets = state.newWorkoutDraft.sets;
      if (currentSets.length >= 30) {
        return;
      }

      const payloadWeight = action.payload?.weight;
      const payloadReps = action.payload?.reps;

      if (currentSets.length === 1 && isPlaceholderSet(currentSets[0]) && (payloadWeight || payloadReps)) {
        currentSets[0].weight = payloadWeight ?? currentSets[0].weight;
        currentSets[0].reps = payloadReps ?? currentSets[0].reps;
        state.newWorkoutDraft.activeSetId = currentSets[0].id;
        return;
      }

      const last = currentSets[currentSets.length - 1];
      currentSets.push(
        createDefaultNewWorkoutSet({
          id: createId(),
          weight: payloadWeight ?? last?.weight ?? '0',
          reps: payloadReps ?? last?.reps ?? '8',
        }),
      );
      state.newWorkoutDraft.activeSetId = currentSets[currentSets.length - 1].id;
    },
    removeNewWorkoutSet(state, action: PayloadAction<string>) {
      const currentSets = state.newWorkoutDraft.sets;
      if (currentSets.length <= 1) {
        state.newWorkoutDraft.sets = [{ ...currentSets[0], weight: '0', reps: '8' }];
        return;
      }

      state.newWorkoutDraft.sets = currentSets.filter((setEntry) => setEntry.id !== action.payload);
      if (state.newWorkoutDraft.activeSetId === action.payload) {
        state.newWorkoutDraft.activeSetId = null;
      }
    },
    updateNewWorkoutSet(
      state,
      action: PayloadAction<{ id: string; field: 'weight' | 'reps'; value: string }>,
    ) {
      const setEntry = state.newWorkoutDraft.sets.find((setItem) => setItem.id === action.payload.id);
      if (!setEntry) {
        return;
      }

      setEntry[action.payload.field] = action.payload.value;
    },
    toggleFavoriteExercise(state, action: PayloadAction<string>) {
      const current = state.newWorkoutDraft.favoriteIds;
      const id = action.payload;
      const next = current.includes(id) ? current.filter((item) => item !== id) : [id, ...current].slice(0, 40);
      state.newWorkoutDraft.favoriteIds = next;
      persistFavoriteIds(next);
    },
    applyQuickAction(
      state,
      action: PayloadAction<{ type: 'add-2-5' | 'add-5' | 'repeat-last' }>,
    ) {
      const sets = state.newWorkoutDraft.sets;
      if (sets.length === 0) {
        return;
      }

      if (action.payload.type === 'repeat-last') {
        sets.push(createDefaultNewWorkoutSet(sets[sets.length - 1]));
        return;
      }

      const activeId = state.newWorkoutDraft.activeSetId;
      const target = (activeId && sets.find((setItem) => setItem.id === activeId)) ?? sets[sets.length - 1];
      if (!target) {
        return;
      }

      const currentWeight = Number(target.weight);
      const safeWeight = Number.isFinite(currentWeight) ? currentWeight : 0;
      const delta = action.payload.type === 'add-2-5' ? 2.5 : 5;
      target.weight = String(Math.round((safeWeight + delta) * 100) / 100);
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
      .addCase(startEditWorkoutForm.pending, (state) => {
        state.isLoadingWorkout = true;
        state.editError = null;
      })
      .addCase(startEditWorkoutForm.fulfilled, (state, action) => {
        const entry = action.payload.exerciseEntries?.[0];
        const sets = entry?.sets && entry.sets.length > 0
          ? entry.sets.map((setEntry) => ({
              id: createId(),
              weight: String(setEntry.weight ?? 0),
              reps: String(setEntry.repetitions),
            }))
          : [createDefaultNewWorkoutSet()];

        state.isLoadingWorkout = false;
        state.newWorkoutDraft = {
          workoutId: action.payload.id,
          currentStep: 2,
          searchQuery: entry?.exerciseNameSnapshot ?? entry?.exerciseName ?? '',
          selectedExerciseId: entry?.externalExerciseId ?? null,
          activeSetId: null,
          favoriteIds: state.newWorkoutDraft.favoriteIds,
          performedAt: action.payload.performedAt ? new Date(action.payload.performedAt).toISOString().slice(0, 16) : getDefaultPerformedAt(),
          notes: '',
          sets,
        };
      })
      .addCase(startEditWorkoutForm.rejected, (state, action) => {
        state.isLoadingWorkout = false;
        state.editError = action.payload ?? 'Error al cargar el entrenamiento para editar.';
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
      .addCase(submitNewWorkoutDraft.pending, (state) => {
        state.isSubmitting = true;
        state.createError = null;
      })
      .addCase(submitNewWorkoutDraft.fulfilled, (state) => {
        state.isSubmitting = false;
        state.newWorkoutDraft = createDefaultNewWorkoutDraft(state.newWorkoutDraft.favoriteIds);
      })
      .addCase(submitNewWorkoutDraft.rejected, (state, action) => {
        state.isSubmitting = false;
        state.createError = action.payload ?? 'Error al guardar el entrenamiento.';
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
  initNewWorkoutDraft,
  setNewWorkoutStep,
  setNewWorkoutSearchQuery,
  selectNewWorkoutExercise,
  setNewWorkoutPerformedAt,
  setNewWorkoutNotes,
  setNewWorkoutActiveSet,
  addNewWorkoutSet,
  removeNewWorkoutSet,
  updateNewWorkoutSet,
  toggleFavoriteExercise,
  applyQuickAction,
} = workoutsSlice.actions;

export const workoutsReducer = workoutsSlice.reducer;
