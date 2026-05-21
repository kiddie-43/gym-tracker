import { Route, Routes } from 'react-router-dom';

import { AppLayout } from '../components/AppLayout/AppLayout';
import { HomePage } from '../pages/Home/HomePage';
import { AdministrationPage } from '../pages/administration/AdministrationPage';
import { PlaceholderPage } from '../pages/placeholder/PlaceholderPage';
import { RoutinesPage } from '../pages/routines/RoutinesPage';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="workouts"       element={<PlaceholderPage labelKey="nav.workout" />} />
        <Route path="progress"       element={<PlaceholderPage labelKey="nav.progress" />} />
        <Route path="routines"       element={<RoutinesPage />} />
        <Route path="diets"          element={<PlaceholderPage labelKey="nav.diets" />} />
        <Route path="meals"          element={<PlaceholderPage labelKey="nav.meals" />} />
        <Route path="settings"       element={<PlaceholderPage labelKey="nav.settings" />} />
        <Route path="administration" element={<AdministrationPage />} />
        <Route path="profile"        element={<PlaceholderPage labelKey="nav.profile" />} />
      </Route>
    </Routes>
  );
}
