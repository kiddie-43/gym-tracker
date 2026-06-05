import { useEffect, useMemo } from 'react';

import AddRoundedIcon from '@mui/icons-material/AddRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import NotesRoundedIcon from '@mui/icons-material/NotesRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import { PopUpCode } from '../../../../../enums/popUp/popUp';
import { PopupDialog } from '../../../../../components/PopupDialog/PopupDialog';
import { useAppDispatch, useAppSelector } from '../../../../../redux/hooks';
import {
  closeExerciceLogDialogAction,
  fetchCurrentDayExerciceLogAction,
  openExerciceLogDialogAction,
} from '../../../../../redux/actions/exerciceLogs/exerciceLogsActions';
import { selectExerciceLogsState } from '../../../../../redux/states/exerciceLogs/exerciceLogsState';

interface ExerciseTrainingDataFormProps {
  routineId: string;
  sessionId: string;
  exerciseId: string;
}

export function ExerciseTrainingDataForm({ routineId, sessionId, exerciseId }: ExerciseTrainingDataFormProps) {
  const dispatch = useAppDispatch();
  const { loading, error, currentDayLog, popUpCode } = useAppSelector(selectExerciceLogsState);

  useEffect(() => {
    if (!routineId || !sessionId || !exerciseId) {
      return;
    }

    dispatch(fetchCurrentDayExerciceLogAction({ routineId, sessionId, exerciseId }));
  }, [dispatch, exerciseId, routineId, sessionId]);

  const summary = useMemo(() => {
    if (!currentDayLog) {
      return {
        totalSets: 0,
        totalRepetitions: 0,
        totalLoadKg: 0,
      };
    }

    const totalSets = currentDayLog.performedSets.length;
    const totalRepetitions = currentDayLog.performedSets.reduce((acc, setItem) => acc + setItem.repetitions, 0);
    const totalLoadKg = currentDayLog.performedSets.reduce((acc, setItem) => acc + setItem.weightKg, 0);

    return {
      totalSets,
      totalRepetitions,
      totalLoadKg,
    };
  }, [currentDayLog]);

  const openCreateDialog = () => {
    dispatch(openExerciceLogDialogAction(PopUpCode.Create));
  };

  const openEditDialog = () => {
    if (!currentDayLog) {
      return;
    }

    dispatch(openExerciceLogDialogAction(PopUpCode.Update, currentDayLog));
  };

  const openDeleteDialog = () => {
    if (!currentDayLog) {
      return;
    }

    dispatch(openExerciceLogDialogAction(PopUpCode.Delete, currentDayLog));
  };

  const openNotesDialog = () => {
    dispatch(openExerciceLogDialogAction(PopUpCode.Info, currentDayLog));
  };

  const closeDialog = () => {
    dispatch(closeExerciceLogDialogAction());
  };

  return (
    <Stack spacing={2} sx={{ pb: 1 }}>
      <Paper variant="outlined" sx={{ p: 2, borderRadius: 2.5 }}>
        <Stack spacing={1}>
          <Typography variant="h6">Log de entrenamiento de hoy</Typography>
          <Typography variant="body2" color="text.secondary">
            Consulta base activa. Los formularios de crear/editar/eliminar/notas estan en preparacion.
          </Typography>
        </Stack>
      </Paper>

      {loading ? (
        <Stack alignItems="center" justifyContent="center" sx={{ py: 4 }}>
          <CircularProgress size={28} />
        </Stack>
      ) : (
        <Paper variant="outlined" sx={{ p: 2, borderRadius: 2.5 }}>
          {error ? (
            <Typography variant="body2" color="error">{error}</Typography>
          ) : currentDayLog ? (
            <Stack spacing={1}>
              <Typography variant="subtitle1">Resumen de hoy</Typography>
              <Typography variant="body2" color="text.secondary">
                {summary.totalSets} sets · {summary.totalRepetitions} reps · {summary.totalLoadKg} kg cargados
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Ultima actualizacion: {new Date(currentDayLog.updatedAt).toLocaleString()}
              </Typography>
            </Stack>
          ) : (
            <Typography variant="body2" color="text.secondary">
              No existe log para hoy en este ejercicio.
            </Typography>
          )}
        </Paper>
      )}

      <Box
        sx={{
          display: 'grid',
          gap: 1,
          gridTemplateColumns: {
            xs: '1fr',
            sm: 'repeat(2, minmax(0, 1fr))',
          },
        }}
      >
        <Button variant="contained" startIcon={<AddRoundedIcon />} onClick={openCreateDialog}>
          Crear log
        </Button>
        <Button variant="outlined" startIcon={<EditRoundedIcon />} onClick={openEditDialog} disabled={!currentDayLog}>
          Editar log
        </Button>
        <Button variant="outlined" color="error" startIcon={<DeleteOutlineRoundedIcon />} onClick={openDeleteDialog} disabled={!currentDayLog}>
          Eliminar log
        </Button>
        <Button variant="outlined" startIcon={<NotesRoundedIcon />} onClick={openNotesDialog}>
          Anadir notas
        </Button>
      </Box>

      <PopupDialog
        open={popUpCode === PopUpCode.Create}
        title="Crear log de ejercicio"
        onClose={closeDialog}
        onSubmit={closeDialog}
        closeLabel="Cerrar"
        saveLabel="Guardar"
      >
        <Typography variant="body2">
          Formulario de creacion pendiente: se agregara en el siguiente paso.
        </Typography>
      </PopupDialog>

      <PopupDialog
        open={popUpCode === PopUpCode.Update}
        title="Editar log de ejercicio"
        onClose={closeDialog}
        onSubmit={closeDialog}
        closeLabel="Cerrar"
        saveLabel="Guardar"
      >
        <Typography variant="body2">
          Formulario de edicion pendiente: se agregara en el siguiente paso.
        </Typography>
      </PopupDialog>

      <PopupDialog
        open={popUpCode === PopUpCode.Delete}
        title="Eliminar log de ejercicio"
        onClose={closeDialog}
        onSubmit={closeDialog}
        closeLabel="Cancelar"
        saveLabel="Eliminar"
      >
        <Typography variant="body2">
          Confirmacion de eliminacion pendiente: se conectara al endpoint correspondiente cuando este disponible.
        </Typography>
      </PopupDialog>

      <PopupDialog
        open={popUpCode === PopUpCode.Info}
        title="Notas del ejercicio"
        onClose={closeDialog}
        onSubmit={closeDialog}
        closeLabel="Cerrar"
        saveLabel="Guardar"
      >
        <Typography variant="body2">
          Editor de notas pendiente: se agregara en el siguiente paso.
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
