import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';
import AddIcon from '@mui/icons-material/Add';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import PlayArrowRoundedIcon from '@mui/icons-material/PlayArrowRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Container from '@mui/material/Container';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { alpha } from '@mui/material/styles';

import {

  AddAndEditRoutineAction,


  setRoutinePopUpCodeAction,

  fetchRoutinesPage,
  deleteRoutineAction,
  setRoutineFormAction
} from '../../redux/actions/routines/routinesActions';

import type { AppDispatch } from '../../redux/store';
import { selectRoutinesState } from '../../redux/states/routines/routinesState';
import { FeedbackMessageSpotlight } from '../../components/FeedbackMessage/FeedbackMessage';
import { appLayoutTokens } from '../../theme/theme';
import { RoutineFormDialog } from './routines/form/RoutineFormDialog';
import { RoutineList } from './routines/list/RoutineList';
import { PopUpCode } from '../../enums/popUp/popUp';
import { CardSkeleton } from '../../components/skeleton/skeleton';
import { Fab } from '@mui/material';
import { PopupDialog } from '../../components/PopupDialog/PopupDialog';
import { selectSessionsState } from '../../redux/states/session/session';
import {
  AddAndEditSessionAction,
  deleteSessionAction,
  fetchSessionsByRoutineAction,
  getExerciseByIdSession,
  linkExerciseToSessionAction,
  resetSessions,
  setSessionFormAction,
  setSessionPopUpCodeAction,
  unlinkExerciseFromSessionAction,
} from '../../redux/actions/sessions/sessionsAction';
import { SessionList } from './sessions/list/SessionList';
import type { IRoutine } from '../../interfaces/routines/IRoutines';
import { RoutineSessionsForm } from './sessions/form/RoutineSessionsSection';
import type { ISession } from '../../interfaces/ISession/ISession';
import { selectExercisesState } from '../../redux/states/exercises/exercisesState';
import { setExerciseFormAction, setExercisePopUpCodeAction } from '../../redux/actions/exercises/exercisesActions';
import { LinkExerciseDialog } from './exercices/form/LinkExerciseDialog';
import { ExerciseList } from './exercices/list/ExerciseList';
import { setPreferencesHeaderTitle } from '../../redux/actions/preferences/preferencesActions';
import type { IExercise } from '../../interfaces/IExercises/IExercises';
import { ExerciseTrainingDataForm } from './exercices/detail/ExerciseTrainingDataForm';
import routineHero01 from '../../assets/images/routines/routine-hero-01.jpg';
import routineHero02 from '../../assets/images/routines/routine-hero-02.jpg';
import routineHero03 from '../../assets/images/routines/routine-hero-03.jpg';
import routineHero04 from '../../assets/images/routines/routine-hero-04.jpg';
import routineHero05 from '../../assets/images/routines/routine-hero-05.jpg';
import routineHero06 from '../../assets/images/routines/routine-hero-06.jpg';
import sessionPlaceholder01 from '../../assets/images/sessions/session-placeholder-01.svg';
import sessionPlaceholder02 from '../../assets/images/sessions/session-placeholder-02.svg';
import sessionPlaceholder03 from '../../assets/images/sessions/session-placeholder-03.svg';
import sessionPlaceholder04 from '../../assets/images/sessions/session-placeholder-04.svg';

type StepperKey = 'routine' | 'session' | 'exercise' | 'exerciseData';

const SKELETON_CARD_COUNT = 3;

const ROUTINE_IMAGES = [
  routineHero01,
  routineHero02,
  routineHero03,
  routineHero04,
  routineHero05,
  routineHero06,
];

const SESSION_IMAGES = [
  sessionPlaceholder01,
  sessionPlaceholder02,
  sessionPlaceholder03,
  sessionPlaceholder04,
];

function buildSeed(input: string): number {
  let hash = 0;
  for (let i = 0; i < input.length; i += 1) {
    hash = (hash << 5) - hash + input.charCodeAt(i);
    hash |= 0;
  }
  return Math.abs(hash);
}

export function RoutinesPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { loading: loadingRoutine, popUpCode: routinesPopUpCode, table, form: formRoutines } = useSelector(selectRoutinesState);
  const { loading: loadingSessions, popUpCode: sessionsPopUpCode, table: sessionsTable, form: formSessions } = useSelector(selectSessionsState);
  const { loading: loadingExercises, table: exercisesTable, popUpCode: exercisesPopUpCode, form: formExercises } = useSelector(selectExercisesState);
  const sessionsItems = Array.isArray(sessionsTable?.items) ? sessionsTable.items : [];
  const exercises = Array.isArray(exercisesTable?.items) ? exercisesTable.items : [];
  const hasRoutines = table.items.length > 0;
  const hasSessions = sessionsItems.length > 0;
  const hasExercises = exercises.length > 0;
  const linkedExerciseIds = exercises
    .map((exercise) => exercise.id ?? '')
    .filter((id): id is string => Boolean(id));
  const [stepperKey, setStepperKey] = useState<StepperKey>('routine');

  useEffect(() => {
    dispatch(fetchRoutinesPage());
  }, [dispatch]);

  useEffect(() => {
    if (stepperKey !== 'session' || !formRoutines.id) {
      return;
    }

    dispatch(fetchSessionsByRoutineAction(formRoutines.id));
  }, [dispatch, formRoutines.id, stepperKey]);

  const showBackButton = stepperKey !== 'routine';

  const headerTitle = useMemo(() => {
    if (stepperKey === 'session') {
      return 'Sesiones';
    }

    if (stepperKey === 'exercise' || stepperKey === 'exerciseData') {
      return 'Ejercicios';
    }

    return 'Rutinas';

  }, [stepperKey]);

  useEffect(() => {
    dispatch(setPreferencesHeaderTitle(headerTitle));

    return () => {
      dispatch(setPreferencesHeaderTitle(''));
    };
  }, [dispatch, headerTitle]);

  const backLabel = useMemo(() => {
    if (stepperKey === 'exerciseData') {
      return 'Volver a ejercicios';
    }

    if (stepperKey === 'exercise') {
      return 'Volver a sesiones';
    }

    if (stepperKey === 'session') {
      return 'Volver a rutinas';
    }

    return 'Volver a rutinas';
  }, [stepperKey]);

  const handleBackNavigation = () => {

    if (stepperKey === 'routine') {
      dispatch(fetchRoutinesPage());
      return;
    }

    if (stepperKey === 'session') {
      dispatch(setRoutineFormAction());
      dispatch(setSessionFormAction());
      dispatch(resetSessions());
      dispatch(fetchRoutinesPage());
      setStepperKey('routine');
      return;
    }
    if (stepperKey === 'exercise') {
      dispatch(setSessionFormAction());
      setStepperKey('session');

      return;
    }

    if (stepperKey === 'exerciseData') {
      setStepperKey('exercise');

      return;
    }
  };
  const handleNavigation = (key: StepperKey, data: IRoutine | ISession | IExercise | null) => {
    if (key === 'routine') {
      setStepperKey(key);
      return;
    } else if (key === 'session') {
      if (!data) {
        return;
      }

      dispatch(setRoutineFormAction(data as IRoutine));
      setStepperKey(key);
      return;
    } else if (key === 'exercise') {
      if (!data) {
        return;
      }

      dispatch(setSessionFormAction(data as ISession));
      setStepperKey(key);
      dispatch(getExerciseByIdSession(formRoutines.id ?? '', (data as ISession).id ?? ''));


      return;
    } else if (key === 'exerciseData') {
      if (!data) {
        return;
      }

      dispatch(setExerciseFormAction(data as IExercise));
      setStepperKey(key);
      return;
    }

  };

  const handleCreate = () => {
    const key = stepperKey;

    if (key === 'routine') {
      dispatch(setRoutinePopUpCodeAction(PopUpCode.Create));
    } else if (key === 'session') {

      dispatch(setSessionPopUpCodeAction(PopUpCode.Create));
    } else if (key === 'exercise') {
      dispatch(setExercisePopUpCodeAction(PopUpCode.Create));
    }
  }


  const handleConfirmAction = () => {
    const key = stepperKey;
    if (key === 'routine') {
      dispatch(deleteRoutineAction(formRoutines.id ?? ''));
    } else if (key === 'session') {
      if (formRoutines.id && formSessions.id) {
        dispatch(deleteSessionAction(formRoutines.id, formSessions.id));
      }
    } else if (key === 'exercise') {
      if (formRoutines.id && formSessions.id && formExercises.id) {
        dispatch(unlinkExerciseFromSessionAction(formRoutines.id, formSessions.id, formExercises.id));
      }
    }

    handleCancelRemove();
  }

  const handleCancelRemove = () => {
    const key = stepperKey;
    if (key === 'routine') {
      dispatch(setRoutinePopUpCodeAction(PopUpCode.Default));
    } else if (key === 'session') {
      dispatch(setSessionPopUpCodeAction(PopUpCode.Default));
    } else if (key === 'exercise') {
      dispatch(setExercisePopUpCodeAction(PopUpCode.Default));
    }
  }

  const selectedRoutineName = formRoutines?.name?.trim() || formRoutines?.description?.trim() || 'Rutina seleccionada';
  const selectedRoutineDescription = formRoutines?.description?.trim() || 'Selecciona una sesion para continuar con tus ejercicios.';
  const selectedRoutineImage = (formRoutines as IRoutine & { imageUrl?: string })?.imageUrl?.trim()
    || ROUTINE_IMAGES[buildSeed(formRoutines?.id || selectedRoutineName) % ROUTINE_IMAGES.length];
  const selectedSessionName = formSessions?.name?.trim() || 'Sesion seleccionada';
  const selectedSessionImage = SESSION_IMAGES[buildSeed(formSessions?.id || selectedSessionName) % SESSION_IMAGES.length];
  const selectedSessionDays = Array.isArray(formSessions?.daysOfWeek)
    ? formSessions.daysOfWeek
      .map((day) => day?.trim())
      .filter((day): day is string => Boolean(day))
      .map((day) => {
        const key = day.toLowerCase();
        if (['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'].includes(key)) {
          return key;
        }

        return null;
      })
      .filter((day): day is string => Boolean(day))
      .map((day) => {
        const labels: Record<string, string> = {
          monday: 'Lunes',
          tuesday: 'Martes',
          wednesday: 'Miercoles',
          thursday: 'Jueves',
          friday: 'Viernes',
          saturday: 'Sabado',
          sunday: 'Domingo',
        };

        return labels[day] ?? day;
      })
    : [];
  const selectedSessionMeta = selectedSessionDays.length > 0
    ? `${selectedSessionDays.join(', ')} • ${formSessions?.countExercices ?? 0} ejercicios`
    : `${formSessions?.countExercices ?? 0} ejercicios`;

  return (
    <Container
      maxWidth={false}
      disableGutters
      sx={{
        pt: showBackButton ? 1 : 3,
        pb: 1,
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
        minHeight: 0,
        boxSizing: 'border-box',
        overflow: 'hidden',
        px: 0,
      }}
    >
      <Stack
        spacing={2}
        sx={{
          height: '100%',
          minHeight: 0,
          flex: 1,
          overflow: 'hidden',

        }}
      >
        {showBackButton ? (
          <Stack direction="row" justifyContent="space-between" alignItems="center"
            sx={{
              borderBottom: '1px solid',
              borderColor: 'divider',
              py: 0.5,
              px: appLayoutTokens.contentX,
            }}>

            <Stack direction="row" alignItems="center" spacing={2} sx={{ flex: 1, minWidth: 0 }}>
              <Button
                startIcon={<ArrowBackIcon />}
                onClick={handleBackNavigation}
                color="warning"
                sx={{
                  textTransform: 'none',
                  fontWeight: 700,
                  px: 0.5,
                  minWidth: 'auto',
                }}
              >
                {backLabel}
              </Button>
            </Stack>

            <IconButton size="small" color="inherit" aria-label="Más opciones" sx={{ ml: 1 }}>
              <MoreVertRoundedIcon />
            </IconButton>
          </Stack>
        ) : null}


        {stepperKey === 'routine' ? (
          loadingRoutine ? (
            <Box sx={(theme) => ({
              px: appLayoutTokens.contentX,
              pb: `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.xs)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
              overflowY: 'auto',
            })}>
              <Stack spacing={2}>
                {Array.from({ length: SKELETON_CARD_COUNT }).map((_, index) => (
                  <CardSkeleton key={`routine-skeleton-${index}`} cardHeight={160} />
                ))}
              </Stack>
            </Box>
          ) : (<Box sx={(theme) => ({
            flex: 1,
            minHeight: 0,
            overflowY: hasRoutines ? 'auto' : 'hidden',
            px: appLayoutTokens.contentX,
            height: '100%',
            display: hasRoutines ? 'block' : 'flex',
            pb: {
              xs: hasRoutines
                ? `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.xs)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`
                : 0,
              md: hasRoutines
                ? `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.md)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`
                : 0,
            },
          })}>
            {!hasRoutines ? (
              <FeedbackMessageSpotlight
                type="empty"
                message="No se encontraron rutinas. Crea tu primera rutina para empezar a entrenar!"
                fullHeight
              />
            ) : (
              <RoutineList
                routines={table.items}
                onRoutineSelect={(routine: IRoutine) => {
                  handleNavigation('session', routine);
                }}
              />
            )}
          </Box>)
        ) : null}

        {stepperKey === 'session' ? (
          loadingSessions ? (
            <Box sx={(theme) => ({
              px: appLayoutTokens.contentX,
              pb: `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.xs)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
              overflowY: 'auto',
            })}>
              <Stack spacing={2}>
                {Array.from({ length: SKELETON_CARD_COUNT }).map((_, index) => (
                  <CardSkeleton key={`session-skeleton-${index}`} cardHeight={160} />
                ))}
              </Stack>
            </Box>
          ) : (<Box sx={{
            flex: 1,
            minHeight: 0,
            overflow: 'hidden',
            height: '100%',
            px: appLayoutTokens.contentX,
            display: 'flex',
            flexDirection: 'column',
            gap: 1.5,
          }}>
            <Card
              variant="outlined"
              sx={{
                position: 'relative',
                overflow: 'hidden',
                borderRadius: 3,
                border: '1px solid rgba(255,255,255,0.14)',
                color: 'common.white',
                boxShadow: '0 10px 24px rgba(0,0,0,0.24)',
                minHeight: 86,
                flexShrink: 0,
              }}
            >
              <Box
                sx={{
                  position: 'absolute',
                  inset: 0,
                  overflow: 'hidden',
                  pointerEvents: 'none',
                }}
              >
                <Box
                  component="img"
                  src={selectedRoutineImage}
                  alt=""
                  aria-hidden="true"
                  sx={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'center' }}
                />
                <Box
                  sx={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(90deg, rgba(5,8,14,0.72) 0%, rgba(5,8,14,0.46) 55%, rgba(5,8,14,0.68) 100%)',
                  }}
                />
              </Box>

              <Stack direction="row" spacing={1.2} alignItems="center" sx={{ minWidth: 0, px: 1.5, py: 1.35, position: 'relative' }}>
                <Box
                  sx={{
                    width: 40,
                    height: 40,
                    borderRadius: '50%',
                    bgcolor: alpha('#66BB6A', 0.9),
                    color: 'common.white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <FitnessCenterOutlinedIcon sx={{ fontSize: 21 }} />
                </Box>

                <Stack spacing={0.2} sx={{ minWidth: 0 }}>
                  <Typography
                    variant="h6"
                    sx={{
                      fontSize: { xs: '1.1rem', sm: '1.25rem' },
                      fontWeight: 700,
                      color: 'rgba(255,255,255,0.98)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {selectedRoutineName}
                  </Typography>
                  <Typography
                    variant="caption"
                    sx={{
                      color: 'rgba(255,255,255,0.74)',
                      fontSize: '0.88rem',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {selectedRoutineDescription}
                  </Typography>
                </Stack>
              </Stack>
            </Card>

            <Typography
              variant="h6"
              sx={{
                px: 0.5,
                fontWeight: 700,
                color: 'text.primary',
                flexShrink: 0,
              }}
            >
              Sesiones de la rutina
            </Typography>

            {
              !hasSessions ? (
                <Box sx={{ flex: 1, minHeight: 0, display: 'flex' }}>
                  <FeedbackMessageSpotlight
                    type="empty"
                    message="No se encontraron sesiones para esta rutina. Crea tu primera sesion para empezar a entrenar!"
                    fullHeight
                  />
                </Box>
              ) : (<SessionList
                routineTitle={formRoutines?.name ?? formRoutines?.description}
                sessions={sessionsItems}
                onSessionSelect={(session: ISession) => {
                  handleNavigation('exercise', session);
                }} />
              )
            }
          </Box>)
        ) : null}

        {stepperKey === 'exercise' ? (
          loadingExercises ? (
            <Box sx={{ px: appLayoutTokens.contentX }}>
              <Stack spacing={2}>
                {Array.from({ length: SKELETON_CARD_COUNT }).map((_, index) => (
                  <CardSkeleton key={`exercise-skeleton-${index}`} cardHeight={160} />
                ))}
              </Stack>
            </Box>
          ) : (<Box sx={{
            flex: 1,
            minHeight: 0,
            overflow: 'hidden',
            height: '100%',
            px: appLayoutTokens.contentX,
            display: 'flex',
            flexDirection: 'column',
            gap: 1.5,
          }}>
            <Card
              variant="outlined"
              sx={{
                position: 'relative',
                overflow: 'hidden',
                borderRadius: 3,
                border: '1px solid rgba(255,255,255,0.14)',
                color: 'common.white',
                boxShadow: '0 10px 24px rgba(0,0,0,0.24)',
                minHeight: 86,
                flexShrink: 0,
              }}
            >
              <Box
                sx={{
                  position: 'absolute',
                  inset: 0,
                  overflow: 'hidden',
                  pointerEvents: 'none',
                }}
              >
                <Box
                  component="img"
                  src={selectedSessionImage}
                  alt=""
                  aria-hidden="true"
                  sx={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'center' }}
                />
                <Box
                  sx={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(90deg, rgba(5,8,14,0.72) 0%, rgba(5,8,14,0.46) 55%, rgba(5,8,14,0.68) 100%)',
                  }}
                />
              </Box>

              <Stack direction="row" spacing={1.2} alignItems="center" sx={{ minWidth: 0, px: 1.5, py: 1.35, position: 'relative' }}>
                <Box
                  sx={{
                    width: 40,
                    height: 40,
                    borderRadius: '50%',
                    bgcolor: alpha('#42A5F5', 0.9),
                    color: 'common.white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <PlayArrowRoundedIcon sx={{ fontSize: 22 }} />
                </Box>

                <Stack spacing={0.2} sx={{ minWidth: 0 }}>
                  <Typography
                    variant="h6"
                    sx={{
                      fontSize: { xs: '1.1rem', sm: '1.25rem' },
                      fontWeight: 700,
                      color: 'rgba(255,255,255,0.98)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {selectedSessionName}
                  </Typography>
                  <Typography
                    variant="caption"
                    sx={{
                      color: 'rgba(255,255,255,0.74)',
                      fontSize: '0.88rem',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {selectedSessionMeta}
                  </Typography>
                </Stack>
              </Stack>
            </Card>

            <Typography
              variant="h6"
              sx={{
                px: 0.5,
                fontWeight: 700,
                color: 'text.primary',
                flexShrink: 0,
              }}
            >
              Ejercicios de la sesion
            </Typography>

            {
              !hasExercises ? (
                <Box sx={{ flex: 1, minHeight: 0, display: 'flex' }}>
                  <FeedbackMessageSpotlight
                    type="empty"
                    message="No se encontraron ejercicios para esta sesion. Crea tu primer ejercicio para empezar a entrenar!"
                    fullHeight
                  />
                </Box>
              ) : (<ExerciseList
                onExerciseSelect={(exercise: IExercise) => {
                  handleNavigation('exerciseData', exercise);
                }}
              />
              )
            }
          </Box>)
        ) : null}

        {stepperKey === 'exerciseData' ? (
          <Box sx={{
            flex: 1,
            minHeight: 0,
            overflow: 'hidden',
            height: '100%',
            px: appLayoutTokens.contentX,
            display: 'flex',
            flexDirection: 'column',
            gap: 1.5,
          }}>
            <Card
              variant="outlined"
              sx={{
                position: 'relative',
                overflow: 'hidden',
                borderRadius: 3,
                border: '1px solid rgba(255,255,255,0.14)',
                color: 'common.white',
                boxShadow: '0 10px 24px rgba(0,0,0,0.24)',
                minHeight: 86,
                flexShrink: 0,
              }}
            >
              <Box
                sx={{
                  position: 'absolute',
                  inset: 0,
                  overflow: 'hidden',
                  pointerEvents: 'none',
                }}
              >
                <Box
                  component="img"
                  src={selectedSessionImage}
                  alt=""
                  aria-hidden="true"
                  sx={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'center' }}
                />
                <Box
                  sx={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(90deg, rgba(5,8,14,0.72) 0%, rgba(5,8,14,0.46) 55%, rgba(5,8,14,0.68) 100%)',
                  }}
                />
              </Box>

              <Stack direction="row" spacing={1.2} alignItems="center" sx={{ minWidth: 0, px: 1.5, py: 1.35, position: 'relative' }}>
                <Box
                  sx={{
                    width: 40,
                    height: 40,
                    borderRadius: '50%',
                    bgcolor: alpha('#FFB74D', 0.9),
                    color: 'common.white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <FitnessCenterOutlinedIcon sx={{ fontSize: 22 }} />
                </Box>

                <Stack spacing={0.2} sx={{ minWidth: 0 }}>
                  <Typography
                    variant="h6"
                    sx={{
                      fontSize: { xs: '1.1rem', sm: '1.25rem' },
                      fontWeight: 700,
                      color: 'rgba(255,255,255,0.98)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {formExercises?.name?.trim() || 'Ejercicio seleccionado'}
                  </Typography>
                  <Typography
                    variant="caption"
                    sx={{
                      color: 'rgba(255,255,255,0.74)',
                      fontSize: '0.88rem',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    Registro de entrenamiento del dia
                  </Typography>
                </Stack>
              </Stack>
            </Card>

            <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto' }}>
              <ExerciseTrainingDataForm
                routineId={formRoutines.id ?? ''}
                sessionId={formSessions.id ?? ''}
                exerciseId={formExercises.id ?? ''}
              />
            </Box>
          </Box>
        ) : null}
      </Stack>

      <RoutineFormDialog
        open={routinesPopUpCode === PopUpCode.Create || routinesPopUpCode === PopUpCode.Update}
        onClose={() => dispatch(setRoutinePopUpCodeAction(PopUpCode.Default))}
        onSubmit={(routine) => {
          dispatch(AddAndEditRoutineAction(routine));
        }}
      />

      {formRoutines ? (
        <RoutineSessionsForm
          onCreate={(session) => {
            dispatch(AddAndEditSessionAction(formRoutines.id ?? '', session))
          }}
          open={(sessionsPopUpCode === PopUpCode.Create || sessionsPopUpCode === PopUpCode.Update) && stepperKey === 'session'}
          onClose={() => dispatch(setSessionPopUpCodeAction(PopUpCode.Default))} />
      ) : null}

      <LinkExerciseDialog
        open={exercisesPopUpCode === PopUpCode.Create}
        linkedExerciseIds={linkedExerciseIds}
        onClose={() => dispatch(setExercisePopUpCodeAction(PopUpCode.Default))}
        onLink={(exerciseId, name) => {
          if (!formRoutines.id || !formSessions.id) {
            return;
          }

          dispatch(linkExerciseToSessionAction(formRoutines.id, formSessions.id, exerciseId, name));
        }}
      />


      <PopupDialog
        open={routinesPopUpCode === PopUpCode.Delete || sessionsPopUpCode === PopUpCode.Delete || exercisesPopUpCode === PopUpCode.Delete}
        title={'Confirmar eliminacion'}
        onClose={handleCancelRemove}
        onSubmit={handleConfirmAction}
        closeLabel="Cancelar"
        saveLabel={'Eliminar'}
      >
        <Typography variant="body2">

          {`Esta accion eliminara la ${stepperKey} seleccionada de la vista activa. Podras restaurarla mas adelante si lo necesitas.`}
        </Typography>
      </PopupDialog>



      <Fab
        color="primary"
        aria-label="Crear nueva sesión"
        onClick={handleCreate}
        sx={(theme) => ({
          position: 'fixed',
          right: {
            xs: theme.spacing(appLayoutTokens.fabPosition.right.xs),
            md: theme.spacing(appLayoutTokens.fabPosition.right.md),
          },
          bottom: {
            xs: theme.spacing(appLayoutTokens.fabPosition.bottom.xs),
            md: theme.spacing(appLayoutTokens.fabPosition.bottom.md),
          },
          zIndex: theme.zIndex.speedDial,
        })}
      >
        <AddIcon />
      </Fab>
    </Container>
  );
}
