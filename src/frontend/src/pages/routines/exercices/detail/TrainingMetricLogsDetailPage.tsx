import { useEffect, useMemo, useState } from 'react';

import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import AddRoundedIcon from '@mui/icons-material/AddRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';

import { ActionMenu } from '../../../../components/ActionMenu/ActionMenu';
import { FeedbackMessage } from '../../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import type { TrainingMetricLog } from '../../../../interfaces/routines/trainingMetricLogs/TrainingMetricLog';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import {
  createTrainingMetricGroupAction,
  deleteTrainingMetricGroupAction,
  fetchAvailableTrainingMetricsAction,
  fetchCurrentDayTrainingMetricLogsAction,
  updateTrainingMetricValueAction,
} from '../../../../redux/trainingMetricLogs/thunks';
import { setTrainingMetricLogsPopUpCode } from '../../../../redux/actions/trainingMetricLogs/trainingMetricLogsActions';
import { selectTrainingMetricLogsState } from '../../../../redux/states/trainingMetricLogs/trainingMetricLogsState';
import { MetricNameBlocks } from './components/MetricNameBlocks';

interface TrainingMetricLogsDetailPageProps {
  routineId: string;
  sessionId: string;
  trainingId: string;
  exerciseId: string;
}

export function TrainingMetricLogsDetailPage({
  routineId,
  sessionId,
  trainingId,
  exerciseId,
}: TrainingMetricLogsDetailPageProps) {
  const dispatch = useAppDispatch();
  const { table, metrics, metricsDegraded, loading, error, popUpCode, message } = useAppSelector(selectTrainingMetricLogsState);
  const [selectedLog, setSelectedLog] = useState<TrainingMetricLog | null>(null);
  const [metricId, setMetricId] = useState('');
  const [metricValue, setMetricValue] = useState('');

  useEffect(() => {
    if (!routineId || !sessionId || !trainingId || !exerciseId) {
      return;
    }

    dispatch(fetchCurrentDayTrainingMetricLogsAction({ routineId, sessionId, trainingId, exerciseId }));
    dispatch(fetchAvailableTrainingMetricsAction(exerciseId));
  }, [dispatch, routineId, sessionId, trainingId, exerciseId]);

  useEffect(() => {
    if (!selectedLog && table.list.length > 0) {
      setSelectedLog(table.list[0]);
    }
  }, [selectedLog, table.list]);

  useEffect(() => {
    if (!metricId && metrics.length > 0) {
      setMetricId(metrics[0].metricId);
    }
  }, [metricId, metrics]);

  const refreshFilters = useMemo(
    () => ({
      routineId,
      sessionId,
      trainingId,
      exerciseId,
    }),
    [routineId, sessionId, trainingId, exerciseId],
  );

  const closeDialog = () => {
    dispatch(setTrainingMetricLogsPopUpCode(PopUpCode.Default));
  };

  const openCreateDialog = () => {
    setMetricValue('');
    dispatch(setTrainingMetricLogsPopUpCode(PopUpCode.Create));
  };

  const openUpdateDialog = (log: TrainingMetricLog) => {
    setSelectedLog(log);
    setMetricId(log.metricId);
    setMetricValue(String(log.value));
    dispatch(setTrainingMetricLogsPopUpCode(PopUpCode.Update));
  };

  const openDeleteDialog = (log: TrainingMetricLog) => {
    setSelectedLog(log);
    dispatch(setTrainingMetricLogsPopUpCode(PopUpCode.Delete));
  };

  const submitCreate = () => {
    const parsedValue = Number(metricValue);
    if (!metricId || Number.isNaN(parsedValue)) {
      return;
    }

    dispatch(
      createTrainingMetricGroupAction(
        {
          routineId,
          sessionId,
          trainingId,
          exerciseId,
          metricId,
          value: parsedValue,
          date: new Date().toISOString().slice(0, 10),
        },
        refreshFilters,
      ),
    );
    closeDialog();
  };

  const submitUpdate = () => {
    const parsedValue = Number(metricValue);
    if (!selectedLog || !selectedLog.groupId || !selectedLog.metricId || Number.isNaN(parsedValue)) {
      return;
    }

    dispatch(
      updateTrainingMetricValueAction(
        {
          groupId: selectedLog.groupId,
          metricId: selectedLog.metricId,
          value: parsedValue,
        },
        refreshFilters,
      ),
    );
    closeDialog();
  };

  const submitDelete = () => {
    if (!selectedLog?.groupId) {
      return;
    }

    dispatch(deleteTrainingMetricGroupAction(selectedLog.groupId, refreshFilters));
    closeDialog();
  };

  return (
    <Stack spacing={2}>
      <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
        <Stack spacing={0.5}>
          <Typography variant="h6">Logs metricos dinamicos</Typography>
          <Typography variant="body2" color="text.secondary">
            {table.list.length > 0
              ? `Registros del dia: ${String(table.list.length)}`
              : 'Aun no hay registros para el dia actual en este contexto.'}
          </Typography>
          {message ? (
            <Typography variant="caption" color="text.secondary">
              {message}
            </Typography>
          ) : null}
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
        <Stack spacing={1.5}>
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Typography variant="subtitle1">Detalle de logs del dia</Typography>
            <Button variant="contained" size="small" startIcon={<AddRoundedIcon />} onClick={openCreateDialog}>
              Crear log
            </Button>
          </Stack>

          {loading ? (
            <Stack alignItems="center" sx={{ py: 3 }}>
              <CircularProgress size={24} />
            </Stack>
          ) : error ? (
            <FeedbackMessage type="error" message={error} />
          ) : table.list.length === 0 ? (
            <FeedbackMessage type="empty" message="No hay logs para el dia actual." />
          ) : (
            <Stack spacing={1}>
              {table.list.map((item) => (
                <Paper key={item.logId} variant="outlined" sx={{ p: 1.5, borderRadius: 1.5 }}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
                    <Box>
                      <Typography variant="body2" fontWeight={600}>
                        {item.metricId} - {item.value}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Grupo: {item.groupId}
                      </Typography>
                    </Box>
                    <ActionMenu
                      ariaLabel={`Acciones del log ${item.logId}`}
                      actions={[
                        {
                          id: 'edit',
                          label: 'Editar',
                          icon: <EditRoundedIcon fontSize="small" />,
                          onClick: () => openUpdateDialog(item),
                        },
                        {
                          id: 'delete',
                          label: 'Eliminar',
                          icon: <DeleteOutlineRoundedIcon fontSize="small" />,
                          onClick: () => openDeleteDialog(item),
                        },
                      ]}
                    />
                  </Stack>
                </Paper>
              ))}
            </Stack>
          )}
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
        <Stack spacing={1.5}>
          <Typography variant="subtitle1">Metricas disponibles</Typography>
          <MetricNameBlocks
            metrics={metrics}
            loading={loading}
            error={error}
            degraded={metricsDegraded}
          />
        </Stack>
      </Paper>

      <PopupDialog
        open={popUpCode === PopUpCode.Create}
        title="Crear log metrico"
        onClose={closeDialog}
        onSubmit={submitCreate}
        closeLabel="Cancelar"
        saveLabel="Guardar"
      >
        <Stack spacing={2}>
          <TextField
            select
            label="Metrica"
            value={metricId}
            onChange={(event) => setMetricId(event.target.value)}
            fullWidth
          >
            {metrics.map((metric) => (
              <MenuItem key={metric.metricId} value={metric.metricId}>
                {metric.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Valor"
            type="number"
            value={metricValue}
            onChange={(event) => setMetricValue(event.target.value)}
            fullWidth
          />
        </Stack>
      </PopupDialog>

      <PopupDialog
        open={popUpCode === PopUpCode.Update}
        title="Editar log metrico"
        onClose={closeDialog}
        onSubmit={submitUpdate}
        closeLabel="Cancelar"
        saveLabel="Guardar"
      >
        <Stack spacing={1}>
          <Typography variant="body2">Metrica: {selectedLog?.metricId ?? '-'}</Typography>
          <TextField
            label="Nuevo valor"
            type="number"
            value={metricValue}
            onChange={(event) => setMetricValue(event.target.value)}
            fullWidth
          />
        </Stack>
      </PopupDialog>

      <PopupDialog
        open={popUpCode === PopUpCode.Delete}
        title="Eliminar log metrico"
        onClose={closeDialog}
        onSubmit={submitDelete}
        closeLabel="Cancelar"
        saveLabel="Eliminar"
      >
        <Typography variant="body2">
          Esta accion eliminara el grupo {selectedLog?.groupId ?? '-'} y todos sus valores asociados.
        </Typography>
      </PopupDialog>
    </Stack>
  );
}