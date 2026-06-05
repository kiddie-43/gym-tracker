import CalendarTodayOutlinedIcon from '@mui/icons-material/CalendarTodayOutlined';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import SportsGymnasticsRoundedIcon from '@mui/icons-material/SportsGymnasticsRounded';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { alpha } from '@mui/material/styles';
import { useRef } from 'react';
import { useTranslation } from 'react-i18next';

import { ActionMenu } from '../../../../components/ActionMenu/ActionMenu';
import type { IExercise } from '../../../../interfaces/IExercises/IExercises';
import { setRemoveExerciseAction } from '../../../../redux/actions/exercises/exercisesActions';
import type { AppDispatch } from '../../../../redux/store';
import { appLayoutTokens } from '../../../../theme/theme';
import { selectExercisesState } from '../../../../redux/states/exercises/exercisesState';
import { useDispatch, useSelector } from 'react-redux';
import routineHero01 from '../../../../assets/images/routines/routine-hero-01.jpg';
import routineHero02 from '../../../../assets/images/routines/routine-hero-02.jpg';
import routineHero03 from '../../../../assets/images/routines/routine-hero-03.jpg';
import routineHero04 from '../../../../assets/images/routines/routine-hero-04.jpg';
import routineHero05 from '../../../../assets/images/routines/routine-hero-05.jpg';
import routineHero06 from '../../../../assets/images/routines/routine-hero-06.jpg';

const CARD_IMAGES = [
  routineHero01,
  routineHero02,
  routineHero03,
  routineHero04,
  routineHero05,
  routineHero06,
];

const HOLD_TO_OPEN_MENU_MS = 450;
const SUPPRESS_CLICK_AFTER_HOLD_MS = 120;

interface ExerciseListProps {
  onExerciseSelect: (exercise: IExercise) => void;
}

function buildSeed(input: string): number {
  let hash = 0;
  for (let i = 0; i < input.length; i += 1) {
    hash = (hash << 5) - hash + input.charCodeAt(i);
    hash |= 0;
  }
  return Math.abs(hash);
}

export function ExerciseList({ onExerciseSelect }: ExerciseListProps) {
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation();
  const { table } = useSelector(selectExercisesState);
  const holdTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const suppressCardClickUntilRef = useRef(0);
  const menuButtonRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const onToggleLink = (exercise: IExercise) => {
    dispatch(setRemoveExerciseAction(exercise));
  };

  const clearHoldTimer = () => {
    if (holdTimerRef.current) {
      clearTimeout(holdTimerRef.current);
      holdTimerRef.current = null;
    }
  };

  const openMenuByHold = (exerciseKey: string) => {
    const button = menuButtonRefs.current[exerciseKey];
    if (!button) {
      return;
    }

    suppressCardClickUntilRef.current = Date.now() + SUPPRESS_CLICK_AFTER_HOLD_MS;
    button.click();
  };

  const handleHoldStart = (exerciseKey: string) => () => {
    clearHoldTimer();
    holdTimerRef.current = setTimeout(() => {
      openMenuByHold(exerciseKey);
    }, HOLD_TO_OPEN_MENU_MS);
  };

  const handleHoldEnd = () => {
    clearHoldTimer();
  };

  const handleCardClick = (exercise: IExercise) => () => {
    if (Date.now() < suppressCardClickUntilRef.current) {
      return;
    }

    onExerciseSelect(exercise);
  };

  return (
    <Box
      sx={(theme) => ({
        height: '100%',
        flex: 1,
        minHeight: 0,
        overflowY: 'auto',
        pb: {
          xs: table?.items.length === 0
            ? 0
            : `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.xs)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
          md: table?.items.length === 0
            ? 0
            : `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.md)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
        },
      })}
    >
      <Box
        sx={{
          width: '100%',
          display: 'grid',
          gap: 2,
          p: 0.5,
          gridTemplateColumns: {
            xs: '1fr',
            sm: 'repeat(auto-fit, minmax(320px, 1fr))',
            md: 'repeat(auto-fit, minmax(440px, 1fr))',
            xl: 'repeat(auto-fit, minmax(500px, 1fr))',
          },
          alignItems: 'stretch',
        }}
      >
        {table?.items.map((exercise, index) => {
          const exerciseKey = exercise.id ?? `${exercise.name}-${index}`;
          const seed = buildSeed(exerciseKey);
          const cardImage = CARD_IMAGES[seed % CARD_IMAGES.length];
          const categoryLabel = exercise.category?.trim() ?? '';
          const difficultyLabel = exercise.difficulty?.trim() ?? '';
          const hasCategory = Boolean(categoryLabel);
          const hasDifficulty = Boolean(difficultyLabel);
          const hasMetaFooter = hasCategory || hasDifficulty;
          const description = exercise.description?.trim();

          return (
            <Card
              key={exercise.id ?? `${exercise.name}-${index}`}
              variant="outlined"
              sx={{
                position: 'relative',
                width: '100%',
                overflow: 'hidden',
                color: 'common.white',
                border: 'none',
                borderColor: 'transparent',
                borderRadius: 3,
                boxShadow: '0 14px 28px rgba(0,0,0,0.34)',
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
                  src={cardImage}
                  alt=""
                  aria-hidden="true"
                  sx={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover',
                    objectPosition: 'center',
                  }}
                />
                <Box
                  sx={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(180deg, rgba(5,8,14,0.18) 0%, rgba(5,8,14,0.34) 38%, rgba(5,8,14,0.72) 100%)',
                  }}
                />
              </Box>

              <Box
                sx={{
                  position: 'absolute',
                  top: { xs: 8, sm: 10, md: 12 },
                  right: { xs: 8, sm: 10, md: 12 },
                  zIndex: 2,
                  borderRadius: 999,
                  bgcolor: 'rgba(10, 15, 24, 0.72)',
                  border: '1px solid rgba(255,255,255,0.22)',
                  boxShadow: '0 10px 22px rgba(0,0,0,0.32)',
                  backdropFilter: 'blur(6px)',
                  p: 0.25,
                }}
              >
                <ActionMenu
                  triggerSx={{
                    color: 'common.white',
                    width: { xs: 34, sm: 38, md: 36 },
                    height: { xs: 34, sm: 38, md: 36 },
                    bgcolor: 'transparent',
                    border: 'none',
                    boxShadow: 'none',
                    '& .MuiSvgIcon-root': {
                      fontSize: { xs: 20, sm: 22, md: 21 },
                    },
                    '&:hover': {
                      bgcolor: 'rgba(255,255,255,0.14)',
                    },
                  }}
                  triggerButtonRef={(element) => {
                    menuButtonRefs.current[exerciseKey] = element;
                  }}
                  actions={[
                    {
                      id: 'delete',
                      label: t('common.actions.delete'),
                      icon: <DeleteOutlineRoundedIcon fontSize="small" />,
                      onClick: () => {
                        onToggleLink(exercise);
                      },
                    },
                  ]}
                />
              </Box>

              <CardActionArea
                onClick={handleCardClick(exercise)}
                onMouseDown={handleHoldStart(exerciseKey)}
                onMouseUp={handleHoldEnd}
                onMouseLeave={handleHoldEnd}
                onTouchStart={handleHoldStart(exerciseKey)}
                onTouchEnd={handleHoldEnd}
                onTouchCancel={handleHoldEnd}
              >
                <Stack
                  sx={{
                    minHeight: { xs: 170, sm: 188 },
                    px: 2,
                    py: 2,
                    position: 'relative',
                    alignItems: 'flex-start',
                    justifyContent: 'flex-start',
                    textAlign: 'left',
                  }}
                >
                  <Stack direction="row" spacing={1.5} alignItems="flex-start" sx={{ width: '100%' }}>
                    <Box
                      sx={{
                        width: { xs: 44, sm: 52 },
                        height: { xs: 44, sm: 52 },
                        borderRadius: '50%',
                        bgcolor: alpha('#42A5F5', 0.92),
                        color: 'common.white',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        flexShrink: 0,
                        boxShadow: '0 8px 20px rgba(0,0,0,0.26)',
                      }}
                    >
                      <SportsGymnasticsRoundedIcon sx={{ fontSize: { xs: 22, sm: 26 } }} />
                    </Box>

                    <Stack spacing={0.8} sx={{ minWidth: 0 }}>
                      <Typography
                        variant="h5"
                        sx={{
                          textTransform: 'uppercase',
                          letterSpacing: 0.8,
                          textShadow: '0 3px 14px rgba(0,0,0,0.72)',
                          fontSize: { xs: '1.2rem', sm: '1.45rem' },
                          lineHeight: 1.1,
                          fontWeight: 700,
                          color: 'rgba(255,255,255,0.98)',
                          display: '-webkit-box',
                          WebkitLineClamp: 2,
                          WebkitBoxOrient: 'vertical',
                          overflow: 'hidden',
                        }}
                      >
                        {exercise.name}
                      </Typography>

                      {hasDifficulty ? (
                        <Chip
                          label={difficultyLabel}
                          size="small"
                          sx={{
                            width: 'fit-content',
                            height: 28,
                            bgcolor: alpha('#42A5F5', 0.26),
                            color: '#7EC8FF',
                            border: `1px solid ${alpha('#42A5F5', 0.72)}`,
                            boxShadow: `0 10px 22px ${alpha('#42A5F5', 0.3)}, 0 0 0 1px ${alpha('#42A5F5', 0.18)}`,
                            backdropFilter: 'blur(6px)',
                            fontWeight: 700,
                            '& .MuiChip-label': {
                              px: 1.5,
                              fontSize: '0.83rem',
                              textShadow: '0 1px 4px rgba(0,0,0,0.32)',
                            },
                          }}
                        />
                      ) : null}

                      {description ? (
                        <Typography
                          variant="body2"
                          sx={{
                            color: 'rgba(255,255,255,0.96)',
                            fontSize: { xs: '0.94rem', sm: '0.98rem' },
                            lineHeight: 1.32,
                            fontWeight: 500,
                            mt: 0.35,
                            textShadow: '0 2px 8px rgba(0,0,0,0.58)',
                            display: '-webkit-box',
                            WebkitLineClamp: 1,
                            WebkitBoxOrient: 'vertical',
                            overflow: 'hidden',
                          }}
                        >
                          {description}
                        </Typography>
                      ) : null}
                    </Stack>
                  </Stack>

                  {hasMetaFooter ? (
                    <Stack
                      direction="row"
                      spacing={0}
                      alignItems="stretch"
                      sx={{
                        mt: 'auto',
                        pt: 1.25,
                        width: '100%',
                        flexWrap: 'nowrap',
                        columnGap: { xs: 1, sm: 1.5, md: 2 },
                        borderTop: '1px solid rgba(255,255,255,0.22)',
                      }}
                    >
                      {hasDifficulty ? (
                        <Stack
                          direction="row"
                          spacing={1}
                          alignItems="center"
                          sx={{
                            flex: { xs: '1 1 0', md: '0 0 auto' },
                            minWidth: 0,
                            justifyContent: 'flex-start',
                          }}
                        >
                          <CalendarTodayOutlinedIcon sx={{ fontSize: 16, color: 'rgba(255,255,255,0.75)' }} />
                          <Typography
                            variant="caption"
                            noWrap
                            sx={{
                              color: 'rgba(255,255,255,0.9)',
                              fontSize: { xs: '0.8rem', sm: '0.86rem' },
                              minWidth: 0,
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }}
                          >
                            {difficultyLabel}
                          </Typography>
                        </Stack>
                      ) : null}

                      {hasCategory ? (
                        <Stack
                          direction="row"
                          spacing={1}
                          alignItems="center"
                          sx={{
                            flex: { xs: '1 1 0', md: '0 0 auto' },
                            minWidth: 0,
                            justifyContent: hasDifficulty ? { xs: 'flex-end', md: 'flex-start' } : 'flex-start',
                          }}
                        >
                          <FitnessCenterOutlinedIcon sx={{ fontSize: 16, color: 'rgba(255,255,255,0.75)' }} />
                          <Typography
                            variant="caption"
                            noWrap
                            sx={{
                              color: 'rgba(255,255,255,0.9)',
                              fontSize: { xs: '0.8rem', sm: '0.86rem' },
                              minWidth: 0,
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }}
                          >
                            {categoryLabel}
                          </Typography>
                        </Stack>
                      ) : null}
                    </Stack>
                  ) : null}
                </Stack>
              </CardActionArea>
            </Card>
          );
        })}
      </Box>
    </Box>
  );
}