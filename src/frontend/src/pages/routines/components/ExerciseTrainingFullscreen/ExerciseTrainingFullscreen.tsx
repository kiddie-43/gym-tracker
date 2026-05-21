import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';

import { ExerciseTrainingDataForm } from '../ExerciseTrainingDataForm/ExerciseTrainingDataForm';
import { ExerciseProgressComparisonPanel } from '../ExerciseProgressComparisonPanel/ExerciseProgressComparisonPanel';
import type { CreateExerciseTrainingLogRequest } from '../../../../interfaces/routines/routines';

interface ExerciseTrainingFullscreenProps {
  routineId: string;
  sessionId: string;
  exerciseId: string;
  onSubmit: (request: CreateExerciseTrainingLogRequest) => void;
}

export function ExerciseTrainingFullscreen({ routineId, sessionId, exerciseId, onSubmit }: ExerciseTrainingFullscreenProps) {
  return (
    <Box sx={{ position: 'fixed', inset: 0, bgcolor: 'background.default', p: 2, zIndex: 1200, overflow: 'auto' }}>
      <Paper sx={{ maxWidth: 680, mx: 'auto', p: 2 }}>
        <ExerciseTrainingDataForm
          routineId={routineId}
          sessionId={sessionId}
          exerciseId={exerciseId}
          onSubmit={onSubmit}
        />
        <Box sx={{ mt: 2 }}>
          <ExerciseProgressComparisonPanel exerciseId={exerciseId} />
        </Box>
      </Paper>
    </Box>
  );
}
