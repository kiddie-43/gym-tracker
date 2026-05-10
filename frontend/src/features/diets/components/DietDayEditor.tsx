import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';

type DietDayEditorProps = {
  dayKey: string;
  onDayKeyChange: (value: string) => void;
};

export function DietDayEditor({ dayKey, onDayKeyChange }: DietDayEditorProps) {
  return (
    <Stack spacing={2}>
      <TextField label="Dia" value={dayKey} onChange={(event) => onDayKeyChange(event.target.value)} />
    </Stack>
  );
}
