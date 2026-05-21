import React, { useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Card, CardActionArea, CardContent, CardActions, Button, Skeleton, Stack, Typography } from '@mui/material';
import ArchiveIcon from '@mui/icons-material/Archive';
import UnarchiveIcon from '@mui/icons-material/Unarchive';
import { archiveRoutineAction, reactivateRoutineAction } from '../../../redux/actions/routines/routinesActions';
import { selectRoutinesState } from '../../../redux/states/routines/routinesState';
import type { AppDispatch } from '../../../redux/store';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';

interface Routine {
  id: string;
  title: string;
  createdAt: string;
  exerciseCount: number;
  isDeleted: boolean;
}

type PendingRoutineAction = {
  routineId: string;
  mode: 'archive' | 'reactivate';
};

interface RoutineListProps {
  routines: Routine[];
  onRoutineSelect: (routineId: string) => void;
}

export const RoutineList: React.FC<RoutineListProps> = ({ routines, onRoutineSelect }) => {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, filters } = useSelector(selectRoutinesState);
  const [draggingId, setDraggingId] = useState<string | null>(null);
  const [swipeOffsetById, setSwipeOffsetById] = useState<Record<string, number>>({});
  const [pendingAction, setPendingAction] = useState<PendingRoutineAction | null>(null);
  const [processingIds, setProcessingIds] = useState<Set<string>>(new Set());
  const startXRef = useRef(0);
  const startYRef = useRef(0);
  const ignoreNextClickRef = useRef(false);

  const SWIPE_THRESHOLD = 90;
  const MAX_SWIPE = 130;

  const handleRoutineSelect = (routineId: string) => {
    if (ignoreNextClickRef.current) {
      ignoreNextClickRef.current = false;
      return;
    }

    onRoutineSelect(routineId);
  };

  const handleArchiveRoutine = (event: React.MouseEvent, routineId: string) => {
    event.stopPropagation();
    setPendingAction({ routineId, mode: 'archive' });
  };

  const handleReactivateRoutine = (event: React.MouseEvent, routineId: string) => {
    event.stopPropagation();
    setPendingAction({ routineId, mode: 'reactivate' });
  };

  const handleConfirmAction = async () => {
    if (!pendingAction) {
      return;
    }

    const includeDeleted = filters.status !== 'active';
    const { routineId, mode } = pendingAction;

    setPendingAction(null);
    setProcessingIds((prev) => {
      const next = new Set(prev);
      next.add(routineId);
      return next;
    });

    try {
      if (mode === 'archive') {
        await dispatch(archiveRoutineAction(routineId, includeDeleted));
      } else {
        await dispatch(reactivateRoutineAction(routineId, includeDeleted));
      }
    } finally {
      setProcessingIds((prev) => {
        const next = new Set(prev);
        next.delete(routineId);
        return next;
      });
    }
  };

  const handleTouchStart = (routineId: string, isDeleted: boolean, event: React.TouchEvent) => {
    if (isDeleted) {
      return;
    }

    const touch = event.touches[0];
    startXRef.current = touch.clientX;
    startYRef.current = touch.clientY;
    setDraggingId(routineId);
  };

  const handleTouchMove = (routineId: string, event: React.TouchEvent) => {
    if (draggingId !== routineId) {
      return;
    }

    const touch = event.touches[0];
    const deltaX = touch.clientX - startXRef.current;
    const deltaY = touch.clientY - startYRef.current;

    // Prioritize vertical scrolling when horizontal intent is not clear.
    if (Math.abs(deltaX) <= Math.abs(deltaY)) {
      return;
    }

    if (deltaX < -10) {
      ignoreNextClickRef.current = true;
      event.preventDefault();
      const clampedOffset = Math.max(deltaX, -MAX_SWIPE);
      setSwipeOffsetById((prev) => ({ ...prev, [routineId]: clampedOffset }));
    }
  };

  const handleTouchEnd = (routineId: string) => {
    const currentOffset = swipeOffsetById[routineId] ?? 0;

    if (currentOffset <= -SWIPE_THRESHOLD && !loading) {
      setPendingAction({ routineId, mode: 'archive' });
    }

    setSwipeOffsetById((prev) => ({ ...prev, [routineId]: 0 }));
    setDraggingId(null);
  };

  if (loading) {
    const skeletonCount = Math.max(routines.length, 2);

    return (
      <Stack
        direction={{ xs: 'column', md: 'row' }}
        flexWrap={{ xs: 'nowrap', md: 'wrap' }}
        spacing={2}
        sx={{ width: '100%' }}
      >
        {Array.from({ length: skeletonCount }).map((_, index) => (
          <Box
            key={`routine-skeleton-${index}`}
            sx={{
              width: {
                xs: '100%',
                md: 'calc(50% - 8px)',
                lg: 'calc(50% - 8px)'
              }
            }}
          >
            <Card variant="outlined" sx={{ width: '100%' }}>
              <CardContent sx={{ p: { xs: 2, md: 3 } }}>
                <Skeleton variant="text" width="55%" height={44} />
                <Skeleton variant="text" width="65%" />
                <Skeleton variant="text" width="45%" />
                <Skeleton variant="text" width="40%" sx={{ mt: { xs: 1, md: 2 } }} />
              </CardContent>
            </Card>
          </Box>
        ))}
      </Stack>
    );
  }

  return (
    <Stack
      direction={{ xs: 'column', md: 'row' }}
      flexWrap={{ xs: 'nowrap', md: 'wrap' }}
      spacing={2}
      sx={{ width: '100%' }}
    >
      {routines.map((routine) => (
        <Box
          key={routine.id}
          sx={{
            width: {
              xs: '100%',
              md: 'calc(50% - 8px)',
              lg: 'calc(50% - 8px)'
            },
            position: 'relative',
            overflow: 'hidden',
            borderRadius: 1.5
          }}
          onTouchStart={(event) => handleTouchStart(routine.id, routine.isDeleted, event)}
          onTouchMove={(event) => handleTouchMove(routine.id, event)}
          onTouchEnd={() => handleTouchEnd(routine.id)}
          onTouchCancel={() => handleTouchEnd(routine.id)}
        >
          {processingIds.has(routine.id) ? (
            <Card variant="outlined" sx={{ width: '100%' }}>
              <CardContent sx={{ p: { xs: 2, md: 3 } }}>
                <Skeleton variant="text" width="55%" height={44} />
                <Skeleton variant="text" width="65%" />
                <Skeleton variant="text" width="45%" />
                <Skeleton variant="text" width="35%" sx={{ mt: { xs: 1, md: 2 } }} />
              </CardContent>
            </Card>
          ) : null}

          {!processingIds.has(routine.id) ? (
            <>
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              display: { xs: 'flex', md: 'none' },
              alignItems: 'center',
              justifyContent: 'flex-end',
              px: 2,
              bgcolor: 'error.main',
              color: 'error.contrastText'
            }}
          >
            <Stack direction="row" spacing={1} alignItems="center">
              <ArchiveIcon fontSize="small" />
              <Typography variant="body2" fontWeight={700}>Archivar</Typography>
            </Stack>
          </Box>

          <Box
            sx={{
              transform: `translateX(${swipeOffsetById[routine.id] ?? 0}px)`,
              transition: draggingId === routine.id ? 'none' : 'transform 180ms ease'
            }}
          >
            <Card variant="outlined" sx={{ width: '100%' }}>
              <CardActionArea onClick={() => handleRoutineSelect(routine.id)}>
                <CardContent sx={{ p: { xs: 2, md: 3 } }}>
                  <Typography variant="h5" fontWeight={700} sx={{ fontSize: { xs: '1.8rem', md: '2rem' } }}>{routine.title}</Typography>
                  {routine.isDeleted ? (
                    <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 0.5 }}>
                      Archivada
                    </Typography>
                  ) : null}
                  <Typography variant="body2" color="text.secondary">
                    Creada: {new Date(routine.createdAt).toLocaleDateString()}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Ejercicios: {routine.exerciseCount}
                  </Typography>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ display: { xs: routine.isDeleted ? 'none' : 'block', md: 'none' }, mt: 0.5 }}
                  >
                    Desliza a la izquierda para archivar
                  </Typography>
                </CardContent>
              </CardActionArea>
              <CardActions sx={{ px: 2, pb: 2, display: { xs: 'none', md: 'flex' } }}>
                {routine.isDeleted ? (
                  <Button
                    size="small"
                    color="success"
                    startIcon={<UnarchiveIcon />}
                    disabled={loading}
                    onClick={(event) => handleReactivateRoutine(event, routine.id)}
                  >
                    Desarchivar
                  </Button>
                ) : (
                  <Button
                    size="small"
                    color="error"
                    startIcon={<ArchiveIcon />}
                    disabled={loading}
                    onClick={(event) => handleArchiveRoutine(event, routine.id)}
                  >
                    Archivar
                  </Button>
                )}
              </CardActions>
            </Card>
          </Box>
            </>
          ) : null}
        </Box>
      ))}

      <PopupDialog
        open={pendingAction !== null}
        title={pendingAction?.mode === 'reactivate' ? 'Confirmar desarchivado' : 'Confirmar archivado'}
        onClose={() => setPendingAction(null)}
        onSubmit={handleConfirmAction}
        closeLabel="Cancelar"
        saveLabel={pendingAction?.mode === 'reactivate' ? 'Desarchivar' : 'Archivar'}
      >
        <Typography variant="body2">
          {pendingAction?.mode === 'reactivate'
            ? 'Esta accion reactivara la rutina seleccionada y volvera a estar disponible para entrenar.'
            : 'Esta accion archivara la rutina seleccionada. Puedes restaurarla mas adelante si el flujo lo permite.'}
        </Typography>
      </PopupDialog>
    </Stack>
  );
};