import { useEffect, useState, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';

import {
  fetchMonthlyPlanAction,
  linkExerciseToDayAction,
  setMonthlyPlanSelectedDay,
  setMonthlyPlanSelectedWeek,
  unlinkPlannedExerciseAction,
  updatePlannedExerciseAction,
} from '../../../redux/actions/monthlyPlan/monthlyPlanActions';
import { selectMonthlyPlanState } from '../../../redux/states/monthlyPlan/monthlyPlanState';
import { getExerciseByIdAction } from '../../../redux/actions/exercises/exercisesActions';
import { selectExercisesState } from '../../../redux/states/exercises/exercisesState';
import {
  setPreferencesHeaderTitle,
  setPreferencesHeaderShowBackButton,
} from '../../../redux/actions/preferences/preferencesActions';
import { selectPreferencesState } from '../../../redux/states/preferences/preferencesState';
import {
  fetchBlockTemplates,
  fetchTrainingSessionHistory,
  resetTrainingSessions,
  setTrainingSessionsForm,
  submitTrainingSession,
} from '../../../redux/actions/trainingSessions/trainingSessionsActions';
import { selectTrainingSessionsState } from '../../../redux/states/trainingSessions/trainingSessionsState';
import { trainingSessionsInitialState } from '../../../redux/states/trainingSessions/trainingSessionsState';
import { resolveExerciseProfile } from '../../../utils/exerciseProfile';
import type { AppDispatch } from '../../../redux/store';
import { MonthlyWeekTabs } from './components/MonthlyWeekTabs';
import { MonthlyDayTabs } from './components/MonthlyDayTabs';
import { DayExercisesPanel } from './components/DayExercisesPanel';
import { ExerciseDetail } from './components/ExerciseDetail';

export function MonthlyPlanPage() {
  const dispatch = useDispatch<AppDispatch>();
  const {
    plan,
    loading,
    saving,
    error,
    selectedWeek,
    selectedDay,
  } = useSelector(selectMonthlyPlanState);
  const { form: exerciseDetail } = useSelector(selectExercisesState);
  const { headerBackRequestToken } = useSelector(selectPreferencesState);
  const {
    form: trainingSessionForm,
    history: trainingSessionHistory,
    loading: trainingSessionLoading,
    error: trainingSessionError,
  } = useSelector(selectTrainingSessionsState);
  const [selectedExerciseId, setSelectedExerciseId] = useState<string | null>(null);
  const prevTokenRef = useRef(0);

  useEffect(() => {
    dispatch(fetchMonthlyPlanAction());
  }, [dispatch]);

  useEffect(() => {
    // Detectar si se presionó el botón de atrás
    if (selectedExerciseId && headerBackRequestToken > prevTokenRef.current) {
      // Volver del detalle al listado
      setSelectedExerciseId(null);
      dispatch(setPreferencesHeaderTitle(''));
      dispatch(setPreferencesHeaderShowBackButton(false));
      dispatch(resetTrainingSessions());
      prevTokenRef.current = headerBackRequestToken;
      return;
    }

    // Actualizar el token después de cualquier otro cambio
    prevTokenRef.current = headerBackRequestToken;
  }, [headerBackRequestToken, selectedExerciseId, dispatch]);

  const activeWeek = plan?.weeks.find((week) => week.weekNumber === selectedWeek) ?? null;
  const activeDay = activeWeek?.days.find((day) => day.dayNumber === selectedDay) ?? null;

  const handleExerciseSelected = (exerciseId: string, exerciseName: string) => {
    prevTokenRef.current = headerBackRequestToken;
    setSelectedExerciseId(exerciseId);
    dispatch(setPreferencesHeaderTitle(exerciseName.charAt(0).toUpperCase() + exerciseName.slice(1)));
    dispatch(setPreferencesHeaderShowBackButton(true));
    // Cargar los detalles completos del ejercicio
    dispatch(getExerciseByIdAction(exerciseId));

    const exerciseType = activeDay?.exercises.find((ex) => ex.exerciseId === exerciseId)?.exerciseType;
    const profile = resolveExerciseProfile(exerciseType);

    dispatch(setTrainingSessionsForm({
      ...trainingSessionsInitialState.form,
      weekNumber: selectedWeek,
      dayNumber: selectedDay,
      exerciseId,
      secondaryMetricUnitCode: profile === 'cardio' ? 'KCAL' : 'KG',
    }));
    dispatch(fetchTrainingSessionHistory());
  };

  const onUpdateExercise = (id: string, exerciseId: string) => {
    dispatch(updatePlannedExerciseAction({
      id,
      request: { exerciseId },
    }));
  };

  const onUnlinkExercise = (id: string) => {
    dispatch(unlinkPlannedExerciseAction(id));
  };

  const onLinkExercise = (exerciseId: string) => {
    dispatch(linkExerciseToDayAction({
      weekId: selectedWeek,
      dayId: selectedDay,
      exerciseId,
    }));
  };

  if (loading && !plan) {
    return (
      <Container maxWidth="lg" sx={{ py: 4, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Container>
    );
  }

  if (!plan) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">No se pudo cargar el plan mensual.</Alert>
      </Container>
    );
  }

  const selectedExercise = activeDay?.exercises.find((ex) => ex.exerciseId === selectedExerciseId);

  return (
    <Container maxWidth="lg" sx={{ py: 3, height: '100%', display: 'flex', width: '100%' }}>
      {selectedExerciseId && selectedExercise && exerciseDetail.id ? (
        <Stack spacing={2.5} sx={{ flex: 1, minWidth: 0, minHeight: 0, width: '100%' }}>
          <Box sx={{ flex: 1, minHeight: 0 }}>
            <ExerciseDetail
              exercise={exerciseDetail}
              form={trainingSessionForm}
              history={trainingSessionHistory}
              loading={trainingSessionLoading}
              error={trainingSessionError}
              onFormChange={(form) => dispatch(setTrainingSessionsForm(form))}
              onSubmit={() => dispatch(submitTrainingSession())}
              onLoadBlockTemplate={() => dispatch(fetchBlockTemplates(exerciseDetail.exerciseType)).unwrap()}
            />
          </Box>
        </Stack>
      ) : selectedExerciseId ? (
        <Stack spacing={2.5} sx={{ flex: 1, minWidth: 0, minHeight: 0, width: '100%' }}>
          <Box sx={{ flex: 1, minHeight: 0, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <CircularProgress />
          </Box>
        </Stack>
      ) : (
        <Stack spacing={2.5} sx={{ flex: 1, minWidth: 0, minHeight: 0, width: '100%' }}>
          {error && (
            <Alert severity="error">{error}</Alert>
          )}

          <MonthlyWeekTabs
            selectedWeek={selectedWeek}
            onChange={(week) => dispatch(setMonthlyPlanSelectedWeek(week))}
          />

          <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minWidth: 0, minHeight: 0, width: '100%' }}>
            <MonthlyDayTabs
              selectedDay={selectedDay}
              activeDays={plan.activeDays}
              onChange={(day) => dispatch(setMonthlyPlanSelectedDay(day))}
            />

            {activeDay && (
              <Box sx={{ flex: 1, minHeight: 0, pt: 1.5 }}>
                <DayExercisesPanel
                  exercises={activeDay.exercises}
                  onUpdateExercise={onUpdateExercise}
                  onUnlinkExercise={onUnlinkExercise}
                  onLinkExercise={onLinkExercise}
                  onExerciseSelected={handleExerciseSelected}
                  disabled={saving}
                />
              </Box>
            )}
          </Box>
        </Stack>
      )}
    </Container>
  );
}
