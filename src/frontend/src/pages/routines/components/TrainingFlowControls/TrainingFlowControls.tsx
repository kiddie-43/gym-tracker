import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';

interface TrainingFlowControlsProps {
  onStart: () => void;
  onCancel: () => void;
  isLocked: boolean;
}

export function TrainingFlowControls({ onStart, onCancel, isLocked }: TrainingFlowControlsProps) {
  return (
    <Stack direction="row" spacing={1}>
      <Button variant="contained" onClick={onStart} disabled={isLocked}>Iniciar entrenamiento</Button>
      <Button variant="outlined" color="error" onClick={onCancel} disabled={!isLocked}>Cancelar entrenamiento</Button>
    </Stack>
  );
}
