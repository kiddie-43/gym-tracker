import CalendarTodayOutlinedIcon from '@mui/icons-material/CalendarTodayOutlined';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
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
import { appLayoutTokens } from '../../../../theme/theme';
import { useAppDispatch } from '../../../../redux/hooks';
import { ISession } from '../../../../interfaces/ISession/ISession';
import { setRemoveSessionAction, setSessionFormAction, setSessionPopUpCodeAction } from '../../../../redux/actions/sessions/sessionsAction';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import sessionPlaceholder01 from '../../../../assets/images/sessions/session-placeholder-01.svg';
import sessionPlaceholder02 from '../../../../assets/images/sessions/session-placeholder-02.svg';
import sessionPlaceholder03 from '../../../../assets/images/sessions/session-placeholder-03.svg';
import sessionPlaceholder04 from '../../../../assets/images/sessions/session-placeholder-04.svg';

interface SessionListProps {
  routineTitle?: string;
  sessions: ISession[];
  onSessionSelect: (session: ISession) => void;
}

const SESSION_IMAGES = [
  sessionPlaceholder01,
  sessionPlaceholder02,
  sessionPlaceholder03,
  sessionPlaceholder04,
];

const HOLD_TO_OPEN_MENU_MS = 450;
const SUPPRESS_CLICK_AFTER_HOLD_MS = 350;

const DAY_ACCENT_BY_KEY: Record<string, string> = {
  monday: '#66BB6A',
  tuesday: '#42A5F5',
  wednesday: '#AB47BC',
  thursday: '#FFA726',
  friday: '#EF5350',
  saturday: '#26C6DA',
  sunday: '#FFCA28',
};

function buildSeed(input: string): number {
  let hash = 0;
  for (let i = 0; i < input.length; i += 1) {
    hash = (hash << 5) - hash + input.charCodeAt(i);
    hash |= 0;
  }
  return Math.abs(hash);
}

export function SessionList({ sessions, onSessionSelect }: SessionListProps) {
  const dispatch = useAppDispatch();
  const { t } = useTranslation();
  const holdTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const suppressCardClickUntilRef = useRef(0);
  const menuButtonRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const handleSessionSelect = (session: ISession) => {
    onSessionSelect(session);
  };

  const clearHoldTimer = () => {
    if (holdTimerRef.current) {
      clearTimeout(holdTimerRef.current);
      holdTimerRef.current = null;
    }
  };

  const openMenuByHold = (sessionKey: string) => {
    const button = menuButtonRefs.current[sessionKey];
    if (!button) {
      return;
    }

    suppressCardClickUntilRef.current = Date.now() + SUPPRESS_CLICK_AFTER_HOLD_MS;
    button.click();
  };

  const handleHoldStart = (sessionKey: string) => () => {
    clearHoldTimer();
    holdTimerRef.current = setTimeout(() => {
      openMenuByHold(sessionKey);
    }, HOLD_TO_OPEN_MENU_MS);
  };

  const handleHoldEnd = () => {
    clearHoldTimer();
  };

  const handleCardClick = (session: ISession) => () => {
    if (Date.now() < suppressCardClickUntilRef.current) {
      return;
    }

    handleSessionSelect(session);
  };

  const onRemoveSession = (session: ISession) => {
    dispatch(setRemoveSessionAction(session));
  };

  const getDaysPreview = (session: ISession) => {
    const translatedDays = session.daysOfWeek.map((day) => t(`common.daysOfWeek.${day}`));
    if (translatedDays.length <= 2) {
      return translatedDays.join(', ');
    }

    return `${translatedDays.slice(0, 2).join(', ')} +${translatedDays.length - 2}`;
  };

  return (
    <Box
      sx={(theme) => ({
        flex: 1,
        minHeight: 0,
        overflowY: 'auto',
        pb: {
          xs: `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.xs)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
          md: `calc(${theme.spacing(appLayoutTokens.fabPosition.bottom.md)} + ${theme.spacing(9)} + env(safe-area-inset-bottom, 0px))`,
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
        {sessions?.map((session, index) => {
          const seed = buildSeed(session.id || `${session.name}-${index}`);
          const sessionKey = session.id || `${session.name}-${index}`;
          const cardImage = SESSION_IMAGES[seed % SESSION_IMAGES.length];
          const daysPreview = getDaysPreview(session);
          const firstDay = session.daysOfWeek[0];
          const accentColor = firstDay ? (DAY_ACCENT_BY_KEY[firstDay] ?? '#90CAF9') : '#90CAF9';

          return (
            <Card
              key={session.id}
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
                    menuButtonRefs.current[sessionKey] = element;
                  }}
                  actions={[
                    {
                      id: 'edit',
                      label: t('common.actions.edit'),
                      icon: <EditRoundedIcon fontSize="small" />,
                      onClick: () => {
                        dispatch(setSessionFormAction(session));
                        dispatch(setSessionPopUpCodeAction(PopUpCode.Update));
                      },
                    },
                    {
                      id: 'delete',
                      label: t('common.actions.delete'),
                      icon: <DeleteOutlineRoundedIcon fontSize="small" />,
                      onClick: () => {
                        onRemoveSession(session);
                      },
                    },
                  ]}
                />
              </Box>

              <CardActionArea
                onClick={handleCardClick(session)}
                onMouseDown={handleHoldStart(sessionKey)}
                onMouseUp={handleHoldEnd}
                onMouseLeave={handleHoldEnd}
                onTouchStart={handleHoldStart(sessionKey)}
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
                        bgcolor: alpha(accentColor, 0.92),
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
                        {session.name}
                      </Typography>

                      {firstDay ? (
                        <Chip
                          label={t(`common.daysOfWeek.${firstDay}`)}
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
                      ) : null}
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
                        {t('common.sessions.days', { days: daysPreview })}
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
                        {t('common.routinesCard.footer.exercises', { count: session.countExercices ?? 0 })}
                      </Typography>
                    </Stack>
                  </Stack>
                </Stack>
              </CardActionArea>
            </Card>
          );
        })}
      </Box>
    </Box>
  );
}
