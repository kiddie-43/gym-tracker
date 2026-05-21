import { useMemo } from 'react';

import ArrowBackRoundedIcon from '@mui/icons-material/ArrowBackRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';

import type { RoutineDetail, TrainingFlowState } from '../../../../interfaces/routines/routines';
import { ExerciseTrainingDataForm } from '../ExerciseTrainingDataForm/ExerciseTrainingDataForm';
import { ExerciseProgressComparisonPanel } from '../ExerciseProgressComparisonPanel/ExerciseProgressComparisonPanel';
import type { CreateExerciseTrainingLogRequest } from '../../../../interfaces/routines/routines';

interface TrainingFlowStepperProps {
  routine: RoutineDetail;
  state: TrainingFlowState;
  onNextStep: (sessionId?: string, exerciseId?: string) => void;
  onPreviousStep: () => void;
  onCancel: () => void;
  onSubmitTrainingLog: (request: CreateExerciseTrainingLogRequest) => void;
}

const STEP_ORDER: TrainingFlowState['stepNode'][] = ['routine', 'session', 'exercise', 'exerciseData'];

export function TrainingFlowStepper({
  routine,
  state,
  onNextStep,
  onPreviousStep,
  onCancel,
  onSubmitTrainingLog,
}: TrainingFlowStepperProps) {
  const currentStepIndex = STEP_ORDER.indexOf(state.stepNode);
  const isFirstStep = currentStepIndex === 0;

  const selectedSession = useMemo(
    () => routine.sessions.find((s) => s.id === state.sessionId) ?? null,
    [routine.sessions, state.sessionId],
  );

  const selectedExercise = useMemo(
    () => selectedSession?.exercises.find((e) => e.id === state.exerciseId) ?? null,
    [selectedSession?.exercises, state.exerciseId],
  );

  const stepTitle = useMemo(() => {
    switch (state.stepNode) {
      case 'routine':
        return 'Entrenamientos';
      case 'session':
        return routine.title;
      case 'exercise':
        return selectedSession?.name ?? '';
      case 'exerciseData':
        return selectedExercise?.name ?? '';
      default:
        return '';
    }
  }, [state.stepNode, routine.title, selectedSession?.name, selectedExercise?.name]);

  const getBackButtonLabel = () => {
    if (isFirstStep) return null;
    return 'Atrás';
  };

  return (
    <Box
      sx={{
        position: 'fixed',
        inset: 0,
        bgcolor: 'background.default',
        zIndex: 1200,
        overflow: 'auto',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      {/* Header */}
      <Box
        sx={{
          bgcolor: 'primary.main',
          color: 'primary.contrastText',
          p: 2,
          boxShadow: 1,
        }}
      >
        <Container maxWidth="lg">
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Stack direction="row" alignItems="center" spacing={2}>
              {!isFirstStep && (
                <Button
                  color="inherit"
                  startIcon={<ArrowBackRoundedIcon />}
                  onClick={onPreviousStep}
                  sx={{ textTransform: 'none' }}
                >
                  {getBackButtonLabel()}
                </Button>
              )}
              <Typography variant="h6" fontWeight={700}>
                {stepTitle}
              </Typography>
            </Stack>
            <Button
              color="inherit"
              onClick={onCancel}
              variant="outlined"
              sx={{ borderColor: 'inherit' }}
            >
              Cancelar
            </Button>
          </Stack>
        </Container>
      </Box>

      {/* Content */}
      <Box
        sx={{
          flex: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          p: 3,
          overflowY: 'auto',
        }}
      >
        <Container maxWidth="lg">
          {/* Step: Routine */}
          {state.stepNode === 'routine' && (
            <Card
              variant="outlined"
              onClick={() => onNextStep()}
              sx={{
                p: 4,
                cursor: 'pointer',
                transition: 'all 0.3s',
                textAlign: 'center',
                maxWidth: 500,
                mx: 'auto',
                '&:hover': {
                  boxShadow: 3,
                  transform: 'translateY(-2px)',
                },
              }}
            >
              <Stack spacing={3}>
                <Typography variant="h4" fontWeight={700}>
                  {routine.title}
                </Typography>
                {routine.goal && (
                  <Typography variant="body1" color="text.secondary">
                    {routine.goal}
                  </Typography>
                )}
                <Grid container spacing={2} justifyContent="center">
                  <Grid size={{ xs: 6 }}>
                    <Stack alignItems="center">
                      <Typography variant="h5" fontWeight={700} color="primary">
                        {routine.sessions.length}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Sesiones
                      </Typography>
                    </Stack>
                  </Grid>
                  <Grid size={{ xs: 6 }}>
                    <Stack alignItems="center">
                      <Typography variant="h5" fontWeight={700} color="primary">
                        {routine.exerciseCount}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Ejercicios
                      </Typography>
                    </Stack>
                  </Grid>
                </Grid>
                <Typography variant="body2" sx={{ mt: 2 }}>
                  Toca para comenzar →
                </Typography>
              </Stack>
            </Card>
          )}

          {/* Step: Session */}
          {state.stepNode === 'session' && (
            <Grid container spacing={2}>
              {routine.sessions.map((session) => (
                <Grid key={session.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Card
                    variant="outlined"
                    onClick={() => onNextStep(session.id ?? undefined)}
                    sx={{
                      p: 2,
                      height: '100%',
                      cursor: 'pointer',
                      transition: 'all 0.3s',
                      display: 'flex',
                      flexDirection: 'column',
                      '&:hover': {
                        boxShadow: 2,
                        transform: 'translateY(-4px)',
                        bgcolor: 'action.hover',
                      },
                      border: state.sessionId === session.id ? '2px solid' : '1px solid',
                      borderColor: state.sessionId === session.id ? 'primary.main' : 'divider',
                    }}
                  >
                    <Stack spacing={2} sx={{ flex: 1 }}>
                      <Typography variant="h6" fontWeight={700}>
                        {session.name}
                      </Typography>
                      <Stack spacing={1}>
                        <Typography variant="caption" color="text.secondary">
                          📅 {session.daysOfWeek.join(', ')}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          💪 {session.exercises.length} ejercicio{session.exercises.length !== 1 ? 's' : ''}
                        </Typography>
                      </Stack>
                    </Stack>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}

          {/* Step: Exercise */}
          {state.stepNode === 'exercise' && selectedSession && (
            <Grid container spacing={2}>
              {selectedSession.exercises.map((exercise) => (
                <Grid key={exercise.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Card
                    variant="outlined"
                    onClick={() => onNextStep(state.sessionId ?? undefined, exercise.id)}
                    sx={{
                      p: 2,
                      height: '100%',
                      cursor: 'pointer',
                      transition: 'all 0.3s',
                      display: 'flex',
                      flexDirection: 'column',
                      '&:hover': {
                        boxShadow: 2,
                        transform: 'translateY(-4px)',
                        bgcolor: 'action.hover',
                      },
                      border: state.exerciseId === exercise.id ? '2px solid' : '1px solid',
                      borderColor: state.exerciseId === exercise.id ? 'primary.main' : 'divider',
                    }}
                  >
                    <Stack spacing={2} sx={{ flex: 1 }}>
                      <Typography variant="h6" fontWeight={700}>
                        {exercise.name}
                      </Typography>
                      <Stack spacing={1}>
                        <Typography variant="caption" fontWeight={600} color="primary">
                          {exercise.plannedSets.length} serie{exercise.plannedSets.length !== 1 ? 's' : ''}
                        </Typography>
                        {exercise.plannedSets.length > 0 && (
                          <Stack spacing={0.5} sx={{ mt: 1, p: 1, bgcolor: 'action.hover', borderRadius: 1 }}>
                            {exercise.plannedSets.slice(0, 2).map((set, idx) => (
                              <Typography key={idx} variant="caption" color="text.secondary">
                                S{set.order}: {set.repetitions} × {set.weightKg}kg
                              </Typography>
                            ))}
                            {exercise.plannedSets.length > 2 && (
                              <Typography variant="caption" color="text.secondary">
                                +{exercise.plannedSets.length - 2} más
                              </Typography>
                            )}
                          </Stack>
                        )}
                      </Stack>
                    </Stack>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}

          {/* Step: Exercise Data */}
          {state.stepNode === 'exerciseData' && selectedExercise && (
            <Stack spacing={3} sx={{ maxWidth: 600, mx: 'auto' }}>
              <ExerciseTrainingDataForm
                routineId={routine.id}
                sessionId={state.sessionId ?? ''}
                exerciseId={state.exerciseId ?? ''}
                onSubmit={onSubmitTrainingLog}
              />
              <Box sx={{ mt: 2 }}>
                <ExerciseProgressComparisonPanel exerciseId={state.exerciseId ?? ''} />
              </Box>
            </Stack>
          )}
        </Container>
      </Box>
    </Box>
  );
}
