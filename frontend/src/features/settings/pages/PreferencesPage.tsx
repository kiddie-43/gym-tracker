import { useEffect, useState } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';

import { PageHeader } from '../../../shared/components/PageHeader';
import type { UserPreferences } from '../../../shared/types/settings';
import { getPreferences, updatePreferences } from '../api/preferencesApi';
import { CalorieTrackingToggle } from '../components/CalorieTrackingToggle';

const defaultPreferences: UserPreferences = {
  calorieTrackingEnabled: false,
  dailyCalorieGoal: null,
};

export function PreferencesPage() {
  const [preferences, setPreferences] = useState<UserPreferences>(defaultPreferences);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  useEffect(() => {
    void getPreferences().then(setPreferences).catch(() => setPreferences(defaultPreferences));
  }, []);

  async function handleSave() {
    const updated = await updatePreferences(preferences);
    setPreferences(updated);
    setStatusMessage('Preferencias actualizadas.');
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="Preferencias" description="Controla el comportamiento del seguimiento de calorias para los registros de comida." />
      {statusMessage ? <Alert severity="success">{statusMessage}</Alert> : null}
      <CalorieTrackingToggle preferences={preferences} onChange={setPreferences} />
      <Button variant="contained" onClick={() => void handleSave()}>Guardar preferencias</Button>
    </Stack>
  );
}
