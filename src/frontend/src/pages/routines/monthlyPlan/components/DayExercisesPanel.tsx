import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Fab from '@mui/material/Fab';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { ActionMenu } from '../../../../components/ActionMenu/ActionMenu';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { LinkExerciseDialog } from '../../exercices/form/LinkExerciseDialog';

import type { IMonthlyPlanExercise } from '../../../../interfaces/monthlyPlan/IWeekPlan';
import { uppercaseFirstLetter } from '../../../../utils/text/text-utils';
import { ExerciseCategory } from '../../../../components/ExerciceCategory/ExerciceCategory';

interface DayExercisesPanelProps {
  exercises: IMonthlyPlanExercise[];
  disabled?: boolean;
  onLinkExercise: (exerciseId: string) => void;
  onUpdateExercise: (id: string, exerciseId: string) => void;
  onUnlinkExercise: (id: string) => void;
  onExerciseSelected?: (exerciseId: string, exerciseName: string) => void;
}

interface EditableExercise {
  id: string;
  exerciseId: string;
  exerciseName: string;
  exerciseType?: string | null;
  primaryMuscles?: string[] | null;
}

function toEditableRows(exercises: IMonthlyPlanExercise[]): EditableExercise[] {
  return exercises.map((exercise) => ({
    id: exercise.id,
    exerciseId: exercise.exerciseId,
    exerciseName: exercise.exerciseName ?? exercise.exerciseId,
    exerciseType: exercise.exerciseType,
    primaryMuscles: exercise.primaryMuscles,
  }));
}

export function DayExercisesPanel({
  exercises,
  disabled = false,
  onLinkExercise,
  onUpdateExercise,
  onUnlinkExercise,
  onExerciseSelected,
}: DayExercisesPanelProps) {
  const { t } = useTranslation();
  const [rows, setRows] = useState<EditableExercise[]>(() => toEditableRows(exercises));
  const [dialogOpen, setDialogOpen] = useState(false);
  const [targetRowIndex, setTargetRowIndex] = useState<number | null>(null);
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);

  useEffect(() => {
    setRows(toEditableRows(exercises));
  }, [exercises]);

  const openAddExercisePicker = () => {
    setTargetRowIndex(null);
    setDialogOpen(true);
  };

  const removeRow = (id: string) => {
    setDeleteConfirmId(id);
  };

  const confirmDelete = () => {
    if (deleteConfirmId) {
      onUnlinkExercise(deleteConfirmId);
      setDeleteConfirmId(null);
    }
  };

  const cancelDelete = () => {
    setDeleteConfirmId(null);
  };

  const openExercisePicker = (rowIndex: number) => {
    setTargetRowIndex(rowIndex);
    setDialogOpen(true);
  };

  const closeExercisePicker = () => {
    setDialogOpen(false);
    setTargetRowIndex(null);
  };

  const linkExerciseToRow = (exerciseId: string) => {
    if (targetRowIndex === null) {
      onLinkExercise(exerciseId);
      return;
    }

    const target = rows[targetRowIndex];
    if (target) {
      onUpdateExercise(target.id, exerciseId);
    }
  };

  const linkedExerciseIds = rows
    .map((row, index) => (index === targetRowIndex ? '' : row.exerciseId))
    .filter((id) => id.trim().length > 0);

  return (
    <Stack spacing={2} sx={{ position: 'relative', pb: 1, height: '100%', minHeight: 0 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h6">{t('monthlyPlan.dayExercises')}</Typography>
      </Stack>

      {rows.length === 0 && (
        <Typography variant="body2" color="text.secondary">
          {t('monthlyPlan.dayExercisesEmpty')}
        </Typography>
      )}

      <Stack spacing={1.5} sx={{ flex: 1, minHeight: 0, overflowY: 'auto', pb: 9 }}>
        {rows.map((row, index) => (
          <Card
            key={row.id}
            onClick={() => onExerciseSelected?.(row.exerciseId, row.exerciseName)}
            sx={{ cursor: 'pointer', transition: 'all 0.2s', '&:hover': { boxShadow: 3 } }}
          >
            <CardContent>
              <Stack direction="row" spacing={2} alignItems="flex-start">
                {/* Image Placeholder */}
                <Box
                  sx={{
                    width: 100,
                    height: 100,
                    minWidth: 100,
                    bgcolor: 'action.hover',
                    borderRadius: 1,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: 'text.secondary',
                  }}
                >
                  <Typography variant="caption">Imagen</Typography>
                </Box>

                {/* Details Section */}
                <Stack spacing={1} sx={{ flex: 1, minWidth: 0 }}>
                  <Typography
                    variant="h6"
                    sx={{
                      color: 'text.primary',
                      display: '-webkit-box',
                      WebkitLineClamp: 1,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden',
                    }}
                  >
                    {uppercaseFirstLetter(row.exerciseName || row.exerciseId)}
                  </Typography>

                  {row.primaryMuscles && row.primaryMuscles.length > 0 && (
                    <Typography
                      variant="body2"
                      sx={{
                        color: 'text.secondary',
                        display: '-webkit-box',
                        WebkitLineClamp: 1,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                      }}
                    >
                      {row.primaryMuscles.join(', ')}
                    </Typography>
                  )}

                  {row.exerciseType && (
                    <ExerciseCategory exerciseType={row.exerciseType} />
                  )}
                </Stack>

                {/* Action Menu */}
                <Box onClick={(e) => e.stopPropagation()}>
                  <ActionMenu
                    actions={[
                      {
                        id: 'update',
                        label: t('common.actions.edit'),
                        icon: <EditIcon />,
                        onClick: () => openExercisePicker(index),
                        disabled,
                      },
                      {
                        id: 'delete',
                        label: t('common.actions.delete'),
                        icon: <DeleteIcon />,
                        onClick: () => removeRow(row.id),
                        disabled,
                      },
                    ]}
                    disabled={disabled}
                    ariaLabel={t('common.actions.actions')}
                  />
                </Box>
              </Stack>
            </CardContent>
          </Card>
        ))}
      </Stack>

      <Fab
        color="primary"
        aria-label={t('monthlyPlan.addExercise')}
        onClick={openAddExercisePicker}
        disabled={disabled}
        sx={{
          position: 'fixed',
          right: 24,
          bottom: 'calc(72px + env(safe-area-inset-bottom, 0px))',
          zIndex: (theme) => theme.zIndex.appBar + 1,
        }}
      >
        <AddIcon />
      </Fab>

      <LinkExerciseDialog
        open={dialogOpen}
        linkedExerciseIds={linkedExerciseIds}
        onClose={closeExercisePicker}
        onLink={(exerciseId) => {
          linkExerciseToRow(exerciseId);
          closeExercisePicker();
        }}
      />

      <PopupDialog
        open={deleteConfirmId !== null}
        title={t('monthlyPlan.confirmDeleteExercise')}
        onClose={cancelDelete}
        onSubmit={confirmDelete}
        saveLabel={t('common.actions.delete')}
        closeLabel={t('common.actions.cancel')}
      >
        <Typography>{t('monthlyPlan.deleteExerciseMessage')}</Typography>
      </PopupDialog>
    </Stack>
  );
}
