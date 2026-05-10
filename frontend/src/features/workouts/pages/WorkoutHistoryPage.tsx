import { useEffect } from 'react';

import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import Fab from '@mui/material/Fab';
import AddRoundedIcon from '@mui/icons-material/AddRounded';

import { useAppDispatch, useAppSelector } from '../../../app/hooks';
import { PageHeader } from '../../../shared/components/PageHeader';
import type { WorkoutSummary, CreateWorkoutRequest } from '../../../shared/types/workouts';
import { HistoryFilters } from '../../progress/components/HistoryFilters';
import { WorkoutEntryForm } from '../components/WorkoutEntryForm';
import { WorkoutCard } from '../components/WorkoutCard';
import {
  closeCreateDialog,
  closeDeleteDialog,
  closeEditDialog,
  confirmDeleteWorkout,
  loadCatalogExercises,
  loadWorkoutHistory,
  openCreateDialog,
  openDeleteDialog,
  setFrom,
  setTo,
  startEditWorkout,
  submitCreateWorkout,
  submitUpdateWorkout,
} from '../state/workoutsSlice';

export function WorkoutHistoryPage() {
  const dispatch = useAppDispatch();
  const {
    from,
    to,
    items,
    exercises,
    selectedWorkout,
    isCreateDialogOpen,
    isEditDialogOpen,
    isDeleteDialogOpen,
    isDeleting,
    isLoadingWorkout,
    isLoadingHistory,
    deleteError,
    createError,
    editError,
    historyError,
    isSubmitting,
  } = useAppSelector((state) => state.workouts);

  useEffect(() => {
    void dispatch(loadCatalogExercises());
  }, [dispatch]);

  useEffect(() => {
    void dispatch(loadWorkoutHistory());
  }, [dispatch, from, to]);

  function handleEditClick(item: WorkoutSummary) {
    void dispatch(startEditWorkout(item.id));
  }

  function handleDeleteClick(item: WorkoutSummary) {
    dispatch(openDeleteDialog(item));
  }

  function handleDeleteConfirm() {
    if (!selectedWorkout) return;

    void dispatch(confirmDeleteWorkout(selectedWorkout.id));
  }

  async function handleUpdateSubmit(request: CreateWorkoutRequest) {
    if (!selectedWorkout) return;
    await dispatch(submitUpdateWorkout({ workoutId: selectedWorkout.id, request }));
  }

  async function handleCreateSubmit(request: CreateWorkoutRequest) {
    await dispatch(submitCreateWorkout(request));
  }

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleString('es-ES');
    } catch {
      return dateString;
    }
  };

  return (
    <Stack sx={{ 
      height: '100%', 
      display: 'flex', 
      flexDirection: 'column',
      overflow: 'hidden',
      gap: 1.5,
    }}>
      {/* Fixed Header and Filters */}
      <Box sx={{ flexShrink: 0 }}>
        <PageHeader title="Entrenamientos" description="Crea, edita y revisa tu historial de entrenamientos." />
      </Box>
      <Box sx={{ flexShrink: 0 }}>
        <HistoryFilters
          from={from}
          to={to}
          onFromChange={(value) => dispatch(setFrom(value))}
          onToChange={(value) => dispatch(setTo(value))}
        />
      </Box>
      
      {/* Scrollable Listing Area */}
      <Box sx={{ 
        flex: 1, 
        overflowY: 'auto', 
        WebkitOverflowScrolling: 'touch',
      }}>
        {isLoadingHistory && items.length === 0 ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        ) : items.length === 0 ? (
          <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No hay entrenamientos en este período.
          </Typography>
        ) : (
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(auto-fill, minmax(280px, 1fr))' },
              gap: 2,
              pb: 2,
              pr: 1,
            }}
          >
            {items.map((item) => (
              <WorkoutCard
                key={item.id}
                workout={item}
                formatDate={formatDate}
                onEdit={handleEditClick}
                onDelete={handleDeleteClick}
              />
            ))}
          </Box>
        )}
        {historyError && <Alert severity="error" sx={{ mt: 2 }}>{historyError}</Alert>}
      </Box>

      {/* Create Dialog */}
      <Dialog open={isCreateDialogOpen} onClose={() => !isSubmitting && dispatch(closeCreateDialog())} maxWidth="sm" fullWidth>
        <DialogTitle>Nuevo entrenamiento</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {createError && <Alert severity="error" sx={{ mb: 2 }}>{createError}</Alert>}
          <WorkoutEntryForm
            onSubmit={handleCreateSubmit}
            exercises={exercises}
            hideSubmitButton={false}
            isSubmitting={isSubmitting}
            submitLabel="Guardar entrenamiento"
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog open={isEditDialogOpen} onClose={() => !isLoadingWorkout && !isSubmitting && dispatch(closeEditDialog())} maxWidth="sm" fullWidth>
        <DialogTitle>Editar entrenamiento</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {editError && <Alert severity="error" sx={{ mb: 2 }}>{editError}</Alert>}
          {isLoadingWorkout ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : selectedWorkout ? (
            <WorkoutEntryForm
              onSubmit={handleUpdateSubmit}
              exercises={exercises}
              hideSubmitButton={false}
              isSubmitting={isSubmitting}
              submitLabel="Actualizar entrenamiento"
            />
          ) : null}
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={isDeleteDialogOpen} onClose={() => dispatch(closeDeleteDialog())}>
        <DialogTitle>Confirmar eliminación</DialogTitle>
        <DialogContent>
          {deleteError && <Alert severity="error" sx={{ mb: 2 }}>{deleteError}</Alert>}
          <Typography>¿Estás seguro de que deseas borrar este entrenamiento?</Typography>
          {selectedWorkout && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {formatDate(selectedWorkout.performedAt)}
            </Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => dispatch(closeDeleteDialog())}>Cancelar</Button>
          <Button 
            onClick={handleDeleteConfirm} 
            variant="contained" 
            color="error"
            disabled={isDeleting}
          >
            {isDeleting ? 'Borrando...' : 'Borrar'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* FAB Button for creating new workout */}
      <Fab
        color="primary"
        aria-label="Agregar entrenamiento"
        onClick={() => dispatch(openCreateDialog())}
        sx={{
          position: 'fixed',
          right: 20,
          bottom: 20,
        }}
      >
        <AddRoundedIcon />
      </Fab>
    </Stack>
  );
}
