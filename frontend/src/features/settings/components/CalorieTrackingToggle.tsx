import FormControlLabel from '@mui/material/FormControlLabel';
import Stack from '@mui/material/Stack';
import Switch from '@mui/material/Switch';
import TextField from '@mui/material/TextField';

import type { UserPreferences } from '../../../shared/types/settings';

type CalorieTrackingToggleProps = {
  preferences: UserPreferences;
  onChange: (preferences: UserPreferences) => void;
};

export function CalorieTrackingToggle({ preferences, onChange }: CalorieTrackingToggleProps) {
  return (
    <Stack spacing={2}>
      <FormControlLabel
        control={<Switch checked={preferences.calorieTrackingEnabled} onChange={(event) => onChange({ ...preferences, calorieTrackingEnabled: event.target.checked })} />}
        label="Activar seguimiento de calorias"
      />
      <TextField
        label="Objetivo diario de calorias"
        type="number"
        value={preferences.dailyCalorieGoal ?? ''}
        onChange={(event) =>
          onChange({
            ...preferences,
            dailyCalorieGoal: event.target.value === '' ? null : Number(event.target.value),
          })
        }
      />
    </Stack>
  );
}
