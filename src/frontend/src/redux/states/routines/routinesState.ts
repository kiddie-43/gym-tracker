import type { CatalogAvailability, RoutineCard, RoutineDetail, TrainingFlowState } from '../../../interfaces/routines/routines';
import type { RootState } from '../../store';
import { createSelector } from '@reduxjs/toolkit';

export type RoutinesFilters = {
  search: string;
  startDate: string;
  endDate: string;
  status: 'active' | 'archived' | 'all';
};

export type RoutineFormState = {
  title: string;
  goal: string;
};

export type RoutinesState = {
  list: RoutineCard[];
  filters: RoutinesFilters;
  form: RoutineFormState;
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
  selectedRoutine: RoutineDetail | null;
  trainingFlowState: TrainingFlowState | null;
  catalogAvailability: CatalogAvailability | null;
};

export const routinesInitialState: RoutinesState = {
  list: [],
  filters: {
    search: '',
    startDate: '',
    endDate: '',
    status: 'active',
  },
  form: {
    title: '',
    goal: '',
  },
  error: null,
  loading: false,
  popUpCode: null,
  selectedRoutine: null,
  trainingFlowState: null,
  catalogAvailability: null,
};

export const selectRoutinesState = (state: RootState) => state.routines;

export const selectFilteredRoutines = createSelector(
  [selectRoutinesState],
  (routinesState) => {
    const { list, filters } = routinesState;

    return list.filter((routine) => {
      const normalizedSearch = filters.search.trim().toLowerCase();
      const matchesSearch = normalizedSearch.length === 0
        ? true
        : routine.title.toLowerCase().includes(normalizedSearch);

      const createdAtDate = new Date(routine.createdAt);
      const matchesStartDate = filters.startDate
        ? createdAtDate >= new Date(`${filters.startDate}T00:00:00`)
        : true;
      const matchesEndDate = filters.endDate
        ? createdAtDate <= new Date(`${filters.endDate}T23:59:59`)
        : true;

      const matchesStatus = filters.status === 'all'
        ? true
        : filters.status === 'archived'
          ? routine.isDeleted
          : !routine.isDeleted;

      return matchesSearch && matchesStartDate && matchesEndDate && matchesStatus;
    });
  },
);
