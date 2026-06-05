import BoltRoundedIcon from '@mui/icons-material/BoltRounded';
import CalendarTodayOutlinedIcon from '@mui/icons-material/CalendarTodayOutlined';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import DirectionsRunRoundedIcon from '@mui/icons-material/DirectionsRunRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import AccessTimeOutlinedIcon from '@mui/icons-material/AccessTimeOutlined';
import FitnessCenterRoundedIcon from '@mui/icons-material/FitnessCenterRounded';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import SportsGymnasticsRoundedIcon from '@mui/icons-material/SportsGymnasticsRounded';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { alpha, useTheme } from '@mui/material/styles';
import { useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import routineHero01 from '../../../../assets/images/routines/routine-hero-01.jpg';
import routineHero02 from '../../../../assets/images/routines/routine-hero-02.jpg';
import routineHero03 from '../../../../assets/images/routines/routine-hero-03.jpg';
import routineHero04 from '../../../../assets/images/routines/routine-hero-04.jpg';
import routineHero05 from '../../../../assets/images/routines/routine-hero-05.jpg';
import routineHero06 from '../../../../assets/images/routines/routine-hero-06.jpg';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import { ActionMenu } from '../../../../components/ActionMenu/ActionMenu';
import { IRoutine } from '../../../../interfaces/routines/IRoutines';
import {
  setRemoveRoutineAction,
  setRoutineFormAction,
  setRoutinePopUpCodeAction,
} from '../../../../redux/actions/routines/routinesActions';
import type { AppDispatch } from '../../../../redux/store';


interface RoutineListProps {
  routines: IRoutine[];
  onRoutineSelect: (routine: IRoutine) => void;
}

const CARD_IMAGES = [
  routineHero01,
  routineHero02,
  routineHero03,
  routineHero04,
  routineHero05,
  routineHero06,
];

const MOCK_ICONS = [
  FitnessCenterRoundedIcon,
  BoltRoundedIcon,
  DirectionsRunRoundedIcon,
  SportsGymnasticsRoundedIcon,
];
const ACCENT_COLOR_KEYS = ['warning', 'info', 'success', 'error', 'secondary'] as const;
const HOLD_TO_OPEN_MENU_MS = 450;
const SUPPRESS_CLICK_AFTER_HOLD_MS = 350;

function buildSeed(input: string): number {
  let hash = 0;
  for (let i = 0; i < input.length; i += 1) {
    hash = (hash << 5) - hash + input.charCodeAt(i);
    hash |= 0;
  }
  return Math.abs(hash);
}

export function RoutineList({ routines, onRoutineSelect }: RoutineListProps) {
  const dispatch = useDispatch<AppDispatch>();
  const theme = useTheme();
  const { t } = useTranslation();
  const holdTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const suppressCardClickUntilRef = useRef(0);
  const menuButtonRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const clearHoldTimer = () => {
    if (holdTimerRef.current) {
      clearTimeout(holdTimerRef.current);
      holdTimerRef.current = null;
    }
  };

  const openMenuByHold = (routineKey: string) => {
    const button = menuButtonRefs.current[routineKey];
    if (!button) {
      return;
    }

    suppressCardClickUntilRef.current = Date.now() + SUPPRESS_CLICK_AFTER_HOLD_MS;
    button.click();
  };

  const handleHoldStart = (routineKey: string) => () => {
    clearHoldTimer();
    holdTimerRef.current = setTimeout(() => {
      openMenuByHold(routineKey);
    }, HOLD_TO_OPEN_MENU_MS);
  };

  const handleHoldEnd = () => {
    clearHoldTimer();
  };

  const handleCardClick = (routine: IRoutine, routineId: string) => () => {
    if (Date.now() < suppressCardClickUntilRef.current) {
      return;
    }

    if (routineId) {
      onRoutineSelect(routine);
    }
  };
  const mockChips = [
    t('common.routinesCard.chips.strength'),
    t('common.routinesCard.chips.cardio'),
    t('common.routinesCard.chips.endurance'),
    t('common.routinesCard.chips.functional'),
    t('common.routinesCard.chips.hypertrophy'),
  ];
  const mockLastSeen = [
    t('common.routinesCard.lastSeen.today'),
    t('common.routinesCard.lastSeen.yesterday'),
    t('common.routinesCard.lastSeen.twoDaysAgo'),
    t('common.routinesCard.lastSeen.threeDaysAgo'),
  ];

  return (
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
        overflow: 'visible',
      }}
    >
      {routines.map((routine, index) => {
        const routineWithTitle = routine as IRoutine & { title?: string; imageUrl?: string };
        const routineId = routine.id ?? '';

        const routineName =
          routineWithTitle.title?.trim()
          || routine.name?.trim()
          || routine.description?.trim()
          || t('common.routinesCard.unnamedRoutine');

        const cardImage = routineWithTitle.imageUrl?.trim() || CARD_IMAGES[index % CARD_IMAGES.length];
        const seed = buildSeed(routineId || `${routineName}-${index}`);
        const routineKey = routineId || routine.createdAt || `${routineName}-${index}`;
        const Icon = MOCK_ICONS[seed % MOCK_ICONS.length];
        const accentColor = theme.palette[ACCENT_COLOR_KEYS[seed % ACCENT_COLOR_KEYS.length]].main;
        const chipLabel = mockChips[(seed * 7 + 3) % mockChips.length];
        const sessionsPerWeek = routine.totalSessions ?? ((seed % 4) + 2);
        const exerciseCount = routine.exerciseCount ?? ((seed % 16) + 10);
        const lastSeen = mockLastSeen[(seed * 5 + 1) % mockLastSeen.length];

        return (
          <Card
            key={routineId || routine.createdAt}
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
                  menuButtonRefs.current[routineKey] = element;
                }}
                actions={[
                  {
                    id: 'edit',
                    label: t('common.actions.edit'),
                    icon: <EditRoundedIcon fontSize="small" />,
                    onClick: () => {
                      dispatch(setRoutineFormAction(routine));
                      dispatch(setRoutinePopUpCodeAction(PopUpCode.Update));
                    },
                    disabled: !routineId,
                  },
                  {
                    id: 'delete',
                    label: t('common.actions.delete'),
                    icon: <DeleteOutlineRoundedIcon fontSize="small" />,
                    onClick: () => {
                      dispatch(setRemoveRoutineAction(routine));
                    },
                    disabled: !routineId,
                  },
                ]}
              />
            </Box>

            <CardActionArea
              disabled={!routineId}
              onClick={handleCardClick(routine, routineId)}
              onMouseDown={handleHoldStart(routineKey)}
              onMouseUp={handleHoldEnd}
              onMouseLeave={handleHoldEnd}
              onTouchStart={handleHoldStart(routineKey)}
              onTouchEnd={handleHoldEnd}
              onTouchCancel={handleHoldEnd}
            >
              <Stack
                sx={{
                  minHeight: { xs: 220, sm: 260 },
                  px: 2,
                  py: 3,
                  alignItems: 'flex-start',
                  justifyContent: 'flex-start',
                  textAlign: 'left',
                  backgroundImage: `linear-gradient(180deg, rgba(5,8,14,0.18) 0%, rgba(5,8,14,0.34) 38%, rgba(5,8,14,0.72) 100%), url(${cardImage})`,
                  backgroundSize: 'cover',
                  backgroundPosition: 'center',
                  backgroundRepeat: 'no-repeat',
                }}
              >
                <Stack direction="row" spacing={1.5} alignItems="flex-start" sx={{ width: '100%' }}>
                  <Box
                    sx={{
                      width: { xs: 44, sm: 52 },
                      height: { xs: 44, sm: 52 },
                      borderRadius: '50%',
                      bgcolor: alpha(accentColor, 0.92),
                      color: 'common.white',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      flexShrink: 0,
                      boxShadow: '0 8px 20px rgba(0,0,0,0.26)',
                    }}
                  >
                    <Icon sx={{ fontSize: { xs: 22, sm: 26 } }} />
                  </Box>

                  <Stack spacing={0.8} sx={{ minWidth: 0 }}>
                    <Typography
                      variant="h4"
                      sx={{
                        textTransform: 'uppercase',
                        letterSpacing: 0.8,
                        textShadow: '0 3px 14px rgba(0,0,0,0.72)',
                        fontSize: { xs: '1.35rem', sm: '1.75rem' },
                        lineHeight: 1.1,
                        fontWeight: 700,
                        color: 'rgba(255,255,255,0.98)',
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                      }}
                    >
                      {routineName}
                    </Typography>

                    <Chip
                      label={chipLabel}
                      size="small"
                      sx={{
                        width: 'fit-content',
                        height: 28,
                        bgcolor: alpha(accentColor, 0.26),
                        color: accentColor,
                        border: `1px solid ${alpha(accentColor, 0.72)}`,
                        boxShadow: `0 10px 22px ${alpha(accentColor, 0.3)}, 0 0 0 1px ${alpha(accentColor, 0.18)}`,
                        backdropFilter: 'blur(6px)',
                        fontWeight: 700,
                        '& .MuiChip-label': {
                          px: 1.5,
                          fontSize: '0.83rem',
                          textShadow: '0 1px 4px rgba(0,0,0,0.32)',
                        },
                      }}
                    />
                   {
                    routine.description?.trim() && (
                      <Typography
                        variant="body2"
                        sx={{
                          color: 'rgba(255,255,255,0.96)',
                          fontSize: { xs: '0.98rem', sm: '1.04rem' },
                          lineHeight: 1.35,
                          fontWeight: 500,
                          mt: 0.5,
                          textShadow: '0 2px 8px rgba(0,0,0,0.58)',
                          display: '-webkit-box',
                          WebkitLineClamp: 2,
                          WebkitBoxOrient: 'vertical',
                          overflow: 'hidden',
                        }}
                      >
                        {routine.description}
                      </Typography>
                    )}
                    
                  </Stack>
                </Stack>

                <Stack
                  direction="row"
                  spacing={0}
                  alignItems="stretch"
                  sx={{
                    mt: 'auto',
                    pt: 1.75,
                    width: '100%',
                    flexWrap: 'nowrap',
                    columnGap: { xs: 1, sm: 1.5, md: 2 },
                    borderTop: '1px solid rgba(255,255,255,0.22)',
                  }}
                >
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
                      {t('common.routinesCard.footer.sessionsPerWeek', { count: sessionsPerWeek })}
                    </Typography>
                  </Stack>

                  <Stack
                    direction="row"
                    spacing={1}
                    alignItems="center"
                    sx={{
                      flex: { xs: '1 1 0', md: '0 0 auto' },
                      minWidth: 0,
                      justifyContent: { xs: 'center', md: 'flex-start' },
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
                      {t('common.routinesCard.footer.exercises', { count: exerciseCount })}
                    </Typography>
                  </Stack>

                  <Stack
                    direction="row"
                    spacing={1}
                    alignItems="center"
                    sx={{
                      flex: { xs: '1 1 0', md: '0 0 auto' },
                      minWidth: 0,
                      justifyContent: { xs: 'flex-end', md: 'flex-start' },
                    }}
                  >
                    <AccessTimeOutlinedIcon sx={{ fontSize: 16, color: 'rgba(255,255,255,0.75)' }} />
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
                      {t('common.routinesCard.footer.lastSeen', { value: lastSeen })}
                    </Typography>
                  </Stack>
                </Stack>
              </Stack>
            </CardActionArea>
          </Card>
        );
      })}
    </Box>
  );
}
