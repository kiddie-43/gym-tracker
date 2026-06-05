import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import { useTranslation } from 'react-i18next';

interface SessionDaysPickerProps {
  selectedDays: string[];
  onChange: (days: string[]) => void;
}

const dayOptions = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'] as const;

export function SessionDaysPicker({ selectedDays, onChange }: SessionDaysPickerProps) {
  const { t } = useTranslation();

  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {dayOptions.map((day) => (
        <Chip
          key={day}
          label={t(`common.daysOfWeek.${day}`)}
          color={selectedDays.includes(day) ? 'primary' : 'default'}
          onClick={() => onChange(selectedDays.includes(day)
            ? selectedDays.filter((item) => item !== day)
            : [...selectedDays, day])}
        />
      ))}
    </Stack>
  );
}
