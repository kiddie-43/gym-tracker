import { createBrowserRouter } from 'react-router-dom';

import { AppLayout } from '../components/layout/AppLayout';
import type { Exercise, MuscleGroup } from '../interfaces/catalog';
import { apiFetch } from '../services/httpClient';
import { CatalogPage } from '../pages/Catalog/CatalogPage';
import { HomePage } from '../pages/Home/HomePage';
import { ProgressOverviewPage } from '../pages/ProgressOverview/ProgressOverviewPage';
import { RoutineBuilderPage } from '../pages/RoutineBuilder/RoutineBuilderPage';
import { DietPlannerPage } from '../pages/DietPlanner/DietPlannerPage';
import { MealLogPage } from '../pages/MealLog/MealLogPage';
import { PreferencesPage } from '../pages/Preferences/PreferencesPage';
import { ProfilePage } from '../pages/Profile/ProfilePage';
import { WorkoutHistoryPage } from '../pages/WorkoutHistory/WorkoutHistoryPage';
import { MealHistoryPage } from '../pages/MealHistory/MealHistoryPage';

async function loadCatalog() {
  const [muscleGroups, exercises] = await Promise.all([
    apiFetch<MuscleGroup[]>('/api/catalog/muscle-groups').catch(() => []),
    apiFetch<Exercise[]>('/api/catalog/exercises').catch(() => []),
  ]);

  return { muscleGroups, exercises };
}

async function catalogLoader() {
  return loadCatalog();
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'catalog',
        loader: catalogLoader,
        element: <CatalogPage />,
      },
      {
        path: 'workouts',
        element: <WorkoutHistoryPage />,
      },
      {
        path: 'progress/:exerciseId',
        element: <ProgressOverviewPage />,
      },
      {
        path: 'routines/new',
        element: <RoutineBuilderPage />,
      },
      {
        path: 'diets/new',
        element: <DietPlannerPage />,
      },
      {
        path: 'meals/log',
        element: <MealLogPage />,
      },
      {
        path: 'settings/preferences',
        element: <PreferencesPage />,
      },
      {
        path: 'profile',
        element: <ProfilePage />,
      },
      {
        path: 'meals/history',
        element: <MealHistoryPage />,
      },
    ],
  },
]);
