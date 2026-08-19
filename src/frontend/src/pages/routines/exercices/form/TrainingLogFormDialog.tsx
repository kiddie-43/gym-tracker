import { useDispatch, useSelector } from 'react-redux';
import { useEffect, useMemo } from 'react';

import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import {
  setTrainingMetricLogsFilters,
  setTrainingMetricLogsForm,
  setTrainingMetricLogsPopUpCode,
  setTrainingMetricLogsRestTimerSeconds,
  submitTrainingMetricLogForm,
  updateTrainingMetricLogValue,
  updateTrainingMetricLogsUnitValueAction,
} from '../../../../redux/actions/trainingMetricLogs/trainingMetricLogsActions';
import { selectTrainingMetricLogsState } from '../../../../redux/states/trainingMetricLogs/trainingMetricLogsState';
import type { AppDispatch } from '../../../../redux/store';
import { getExerciseByIdAction } from '../../../../redux/actions/exercises/exercisesActions';
import { selectExercicesForm } from '../../../../redux/states/exercises/exercisesState';
import { MetricValueInput } from './components/MetricValueInput';

interface TrainingLogFormDialogProps {
  weekNumber?: number;
  dayNumber?: number;
  exerciseId?: string;
  exerciseCode?: string;
}

function normalizeMetricCode(value: string | undefined): string {
  return (value ?? '').trim().toLowerCase().replace(/[^a-z0-9]+/g, '');
}

function getRestTimeSeconds(unitValues?: Record<string, number>): number {
  if (!unitValues) {
    return 0;
  }

  const restEntry = Object.entries(unitValues).find(([code]) => {
    const normalizedCode = normalizeMetricCode(code);
    return normalizedCode === 'resttime' || normalizedCode === 'tiempodedescanso';
  });

  if (!restEntry) {
    return 0;
  }

  const minutes = Number(restEntry[1]);
  if (!Number.isFinite(minutes) || minutes <= 0) {
    return 0;
  }

  return Math.round(minutes * 60);
}

export function TrainingLogFormDialog({ weekNumber, dayNumber, exerciseId, exerciseCode }: TrainingLogFormDialogProps) {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, error, popUpCode, form } = useSelector(selectTrainingMetricLogsState);
  const { units } = useSelector(selectExercicesForm);

  const orderedUnits = useMemo(() => {
    return [...units].sort((left, right) => {
      const leftCode = (left.code ?? '').trim().toLowerCase();
      const rightCode = (right.code ?? '').trim().toLowerCase();

      const priority = (code: string): number => {
        if (['weight', 'weightkg', 'kg', 'load', 'carga'].includes(code)) return 0;
        if (['reps', 'rep', 'repetitions', 'repeticiones'].includes(code)) return 1;
        if (['distance', 'distancia', 'km', 'meters', 'meter', 'm'].includes(code)) return 2;
        if (['time', 'tiempo', 'duration', 'min', 'minutes'].includes(code)) return 3;
        if (['kcal', 'calories', 'caloria', 'calorias', 'calorie'].includes(code)) return 4;
        if (code === 'rpe') return 5;
        return 20;
      };

      const leftPriority = priority(leftCode);
      const rightPriority = priority(rightCode);

      if (leftPriority !== rightPriority) {
        return leftPriority - rightPriority;
      }

      return (left.name ?? '').localeCompare(right.name ?? '', 'es-ES');
    });
  }, [units]);


  const onClose = () => {
    dispatch(setTrainingMetricLogsPopUpCode(PopUpCode.Default));
    dispatch(
      setTrainingMetricLogsForm({
        groupId: '',
        weekNumber,
        dayNumber,
        exerciseCode: exerciseCode ?? '',
        timestamp: '',
        unitValues: {},
        metrics: [],
      }),
    );
  };

  const onSubmit = async () => {
    const metrics = Object.entries(form.unitValues ?? {})
      .filter(([, value]) => Number.isFinite(value))
      .map(([code, value]) => ({ code, value }));
    const restTimeSeconds = getRestTimeSeconds(form.unitValues);

    if (metrics.length === 0) {
      return;
    }

    if (popUpCode === PopUpCode.Create) {
      const resultAction = await dispatch(submitTrainingMetricLogForm());

      if (submitTrainingMetricLogForm.fulfilled.match(resultAction) && restTimeSeconds > 0) {
        dispatch(setTrainingMetricLogsRestTimerSeconds(restTimeSeconds));
      }
    } else if (popUpCode === PopUpCode.Update) {
      await dispatch(updateTrainingMetricLogValue({
        groupId: form.groupId,
        weekNumber: form.weekNumber ?? weekNumber,
        dayNumber: form.dayNumber ?? dayNumber,
        exerciseCode: form.exerciseCode || exerciseCode,
        metrics,
      }));
    }
  };

  useEffect(() => {
    if (weekNumber && dayNumber && exerciseCode) {
      dispatch(setTrainingMetricLogsFilters({ weekNumber, dayNumber, exerciseCode }));
    }
  }, [weekNumber, dayNumber, exerciseCode, dispatch]);

  useEffect(() => {
    if (exerciseId) {
      dispatch(getExerciseByIdAction(exerciseId));
    }

  }, [exerciseId, dispatch]);

  return (
    <PopupDialog
      open={popUpCode === PopUpCode.Create || popUpCode === PopUpCode.Update}
      title={popUpCode === PopUpCode.Update ? 'Editar log de entrenamiento' : 'Crear log de entrenamiento'}
      onClose={onClose}
      onSubmit={() => {
        void onSubmit();
      }}
      closeLabel="Cancelar"
      saveLabel="Guardar"
      isSaving={loading}
    >
      <Stack spacing={2}>
        {error ? <Alert severity="error">{error}</Alert> : null}

        {orderedUnits.length === 0 ? (
          <Alert severity="info">No hay unidades disponibles</Alert>
        ) : (
          orderedUnits.map((unit) => (
            <MetricValueInput
              key={unit.id ?? unit.code}
              metricCode={unit.code}
              label={unit.name}
              value={form.unitValues?.[unit.code] ?? ''}
              disabled={loading}
              onChange={(value) => {
                dispatch(updateTrainingMetricLogsUnitValueAction(unit.code, value));
              }}
            />
          ))
        )}

      </Stack>
    </PopupDialog>
  );
}
