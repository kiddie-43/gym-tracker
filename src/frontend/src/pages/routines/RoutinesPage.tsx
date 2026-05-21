import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';

import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Fab from '@mui/material/Fab';
import { useTranslation } from 'react-i18next';

import {
  cancelTrainingFlowAction,
  createRoutineAction,
  deletePlannedSetAction,
  fetchRoutines,
  loadRoutineDetail,
  restoreTrainingFlowAction,
  setSelectedRoutine,
  setRoutinesPopUpCode,
  unlinkSessionExerciseAction,
  updatePlannedSetAction,
  createSessionAction
} from '../../redux/actions/routines/routinesActions';
import type { AppDispatch } from '../../redux/store';
import { selectFilteredRoutines, selectRoutinesState } from '../../redux/states/routines/routinesState';
import { SessionExerciseCard } from './components/SessionExerciseCard/SessionExerciseCard';
import { RoutineSessionsSection } from './components/RoutineSessionsSection/RoutineSessionsSection';
import { FeedbackMessage } from '../../components/FeedbackMessage/FeedbackMessage';
import { RoutineFormDialog } from './form/RoutineFormDialog';
import { RoutineList } from './list/RoutineList';
import { SessionList } from './components/sessions/SessionList';
import { ExerciseList } from './components/exercises/ExerciseList';
import { Filters } from './filters/Filters';

export function RoutinesPage() {
  const { t } = useTranslation();
  const dispatch = useDispatch<AppDispatch>();
  const { list, loading, error, popUpCode, selectedRoutine, filters } = useSelector(selectRoutinesState);
  const filteredRoutines = useSelector(selectFilteredRoutines);
  const [selectedSessionId, setSelectedSessionId] = useState<string | null>(null);
  const [selectedExerciseId, setSelectedExerciseId] = useState<string | null>(null);
  const [openSessionDialog, setOpenSessionDialog] = useState(false);

  useEffect(() => {
    dispatch(restoreTrainingFlowAction());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchRoutines(filters.status !== 'active'));
  }, [dispatch, filters.status]);

  const selectedSession = useMemo(() => {
    if (!selectedRoutine || !selectedSessionId) {
      return null;
    }

    return selectedRoutine.sessions.find((session) => session.id === selectedSessionId) ?? null;
  }, [selectedRoutine, selectedSessionId]);

  const selectedExercise = useMemo(() => {
    if (!selectedSession || !selectedExerciseId) {
      return null;
    }

    return selectedSession.exercises.find((exercise) => exercise.id === selectedExerciseId) ?? null;
  }, [selectedSession, selectedExerciseId]);

  const showBackButton = selectedRoutine !== null;

  const headerTitle = useMemo(() => {
    if (!selectedRoutine) {
      return 'Rutinas';
    }

    if (selectedExercise) {
      return `Sets de ${selectedExercise.name}`;
    }

    if (selectedSession) {
      return `Ejercicios de ${selectedSession.name}`;
    }

    return `Detalle: ${selectedRoutine.title}`;
  }, [selectedRoutine, selectedSession, selectedExercise]);

  const backLabel = useMemo(() => {
    if (selectedExercise) {
      return 'Volver a ejercicios';
    }

    if (selectedSession) {
      return 'Volver a sesiones';
    }

    return 'Volver a rutinas';
  }, [selectedSession, selectedExercise]);

  const handleBackNavigation = () => {
    if (selectedExercise) {
      setSelectedExerciseId(null);
      return;
    }

    if (selectedSession) {
      setSelectedSessionId(null);
      setSelectedExerciseId(null);
      return;
    }

    if (selectedRoutine) {
      setSelectedSessionId(null);
      setSelectedExerciseId(null);
      dispatch(cancelTrainingFlowAction());
      dispatch(setSelectedRoutine(null));
      dispatch(fetchRoutines(filters.status !== 'active'));
    }
  };

  // Handler for session creation
  const handleCreateSession = (name: string, days: string[]) => {
    if (selectedRoutine) {
      dispatch(createSessionAction(selectedRoutine.id, name, days));
    }
  };

  return (
    <Container maxWidth={false} sx={{ py: 2 }}>
      <Stack spacing={2}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Stack direction="row" alignItems="center" spacing={1}>
            {showBackButton ? (
              <Button startIcon={<ArrowBackIcon />} onClick={handleBackNavigation}>
                {backLabel}
              </Button>
            ) : null}
            <Typography variant={showBackButton ? 'h6' : 'h4'} fontWeight={700}>{headerTitle}</Typography>
          </Stack>
          {!selectedRoutine ? <Filters /> : null}
        </Stack>

        {!selectedRoutine ? (
          <>
            {loading ? <CircularProgress /> : null}
            {!loading && error ? <FeedbackMessage type="error" message={error} /> : null}
            {!loading && !error && list.length === 0 ? (
              <FeedbackMessage type="empty" message={t('common.messages.noResults')} />
            ) : null}

            <Stack spacing={2} direction="row" flexWrap="wrap" justifyContent="center">
              <RoutineList
                routines={filteredRoutines}
                onRoutineSelect={(id: string) => {
                  setSelectedSessionId(null);
                  setSelectedExerciseId(null);
                  dispatch(loadRoutineDetail(id));
                }}
              />
            </Stack>
          </>
        ) : null}

        {/* Floating button for creating a new routine */}
        {!selectedRoutine && (
          <Fab
            color="primary"
            aria-label="Nueva rutina"
            sx={{ position: 'fixed', right: 32, bottom: 80, zIndex: 1200 }}
            onClick={() => dispatch(setRoutinesPopUpCode('create'))}
          >
            <AddIcon />
          </Fab>
        )}

        {selectedRoutine && !selectedSession ? (
          <>
            <Typography variant="h6">Sesiones</Typography>
            <SessionList
              sessions={selectedRoutine.sessions}
              onSessionSelect={(id: string) => {
                setSelectedSessionId(id);
                setSelectedExerciseId(null);
              }}
            />
          </>
        ) : null}

        {selectedRoutine && selectedSession && !selectedExercise ? (
          <>
            <ExerciseList
              exercises={selectedSession.exercises}
              onExerciseSelect={(id: string) => setSelectedExerciseId(id)}
            />
          </>
        ) : null}

        {selectedRoutine && selectedSession && selectedExercise ? (
          <>
            <SessionExerciseCard
              exercise={selectedExercise}
              onUnlink={(exerciseId) => dispatch(unlinkSessionExerciseAction(selectedRoutine.id, selectedSession.id, exerciseId))}
              onUpdateSet={(exerciseId, setId, repetitions, weightKg) =>
                dispatch(updatePlannedSetAction(selectedRoutine.id, selectedSession.id, exerciseId, setId, repetitions, weightKg))}
              onDeleteSet={(exerciseId, setId) =>
                dispatch(deletePlannedSetAction(selectedRoutine.id, selectedSession.id, exerciseId, setId))}
            />
          </>
        ) : null}
      </Stack>

      <RoutineFormDialog
        open={popUpCode === 'create'}
        onClose={() => dispatch(setRoutinesPopUpCode(null))}
        onSubmit={(title, goal) => {
          dispatch(createRoutineAction(title, goal, filters.status !== 'active'));
          dispatch(setRoutinesPopUpCode(null));
        }}
      />

      {/* Floating button for creating sessions, only when viewing routine detail (not session or exercise) */}
      {selectedRoutine && !selectedSession && (
        <>
          <RoutineSessionsSection
            routine={selectedRoutine}
            onCreateSession={handleCreateSession}
            openDialog={openSessionDialog}
            setOpenDialog={setOpenSessionDialog}
          />
          <Fab
            color="primary"
            aria-label="Crear sesión"
            sx={{ position: 'fixed', right: 32, bottom: 80, zIndex: 1200 }}
            onClick={() => setOpenSessionDialog(true)}
          >
            <AddIcon />
          </Fab>
        </>
      )}
    </Container>
  );
}
