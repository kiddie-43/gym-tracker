import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import type { TrainingFlowState } from '../../../../interfaces/routines/routines';

interface TrainingStepperProps {
  state: TrainingFlowState;
  onNext: () => void;
  onBack: () => void;
}

const stepOrder: TrainingFlowState['stepNode'][] = ['routine', 'session', 'exercise', 'exerciseData'];

export function TrainingStepper({ state, onNext, onBack }: TrainingStepperProps) {
  const currentIndex = stepOrder.indexOf(state.stepNode);

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="h6">Entrenamiento activo</Typography>
        <Typography variant="body2">Paso: {state.stepNode}</Typography>
        <Stack direction="row" spacing={1}>
          <Button onClick={onBack} disabled={currentIndex <= 0}>Atras</Button>
          <Button onClick={onNext} disabled={currentIndex >= stepOrder.length - 1} variant="contained">Siguiente</Button>
        </Stack>
      </Stack>
    </Paper>
  );
}
