import { useEffect, useState } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import FormControl from '@mui/material/FormControl';
import FormControlLabel from '@mui/material/FormControlLabel';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { ExerciseDto, UpsertExerciseRequest } from '../../interfaces/admin/exercises/exercises';
import type { MeasurementTypeDto } from '../../interfaces/admin/measurementTypes/measurementTypes';
import type { MuscleDto } from '../../interfaces/admin/muscles/muscles';
import {
  createExercise,
  deleteExercise,
  listExercises,
  reactivateExercise,
  updateExercise,
} from '../../services/api/admin/exercises/exercisesApi';
import { listMeasurementTypes } from '../../services/api/admin/measurementTypes/measurementTypesApi';
import { listMuscles } from '../../services/api/admin/muscles/musclesApi';

type FormState = {
  id: string | null;
  name: string;
  code: string;
  description: string;
  category: string;
  difficulty: string;
  measurementTypeId: string;
  primaryMuscleId: string;
  secondaryMuscleId: string;
};

const defaultFormState: FormState = {
  id: null,
  name: '',
  code: '',
  description: '',
  category: 'Strength',
  difficulty: 'Beginner',
  measurementTypeId: '',
  primaryMuscleId: '',
  secondaryMuscleId: '',
};

export function ExercisesPanel() {
  const { t } = useTranslation();
  const [rows, setRows] = useState<ExerciseDto[]>([]);
  const [muscles, setMuscles] = useState<MuscleDto[]>([]);
  const [measurementTypes, setMeasurementTypes] = useState<MeasurementTypeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [includeDeleted, setIncludeDeleted] = useState(false);

  const [formOpen, setFormOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<FormState>(defaultFormState);

  const referenceMuscles = muscles.filter((item) => !item.isDeleted);
  const referenceMeasurementTypes = measurementTypes.filter((item) => !item.isDeleted);

  const loadRows = async () => {
    setLoading(true);
    setError(null);

    try {
      const [exerciseRows, muscleRows, measurementRows] = await Promise.all([
        listExercises(includeDeleted),
        listMuscles(true),
        listMeasurementTypes(true),
      ]);

      setRows(exerciseRows);
      setMuscles(muscleRows);
      setMeasurementTypes(measurementRows);
    } catch (loadError) {
      const message = loadError instanceof Error ? loadError.message : t('administration.common.loadError');
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadRows();
  }, [includeDeleted]);

  const openCreate = () => {
    setFormState((current) => ({
      ...defaultFormState,
      measurementTypeId: referenceMeasurementTypes[0]?.id ?? '',
      primaryMuscleId: referenceMuscles[0]?.id ?? '',
      secondaryMuscleId: '',
      category: current.category,
      difficulty: current.difficulty,
    }));
    setFormError(null);
    setFormOpen(true);
  };

  const openEdit = (row: ExerciseDto) => {
    setFormState({
      id: row.id,
      name: row.name,
      code: row.code,
      description: row.description ?? '',
      category: row.category,
      difficulty: row.difficulty,
      measurementTypeId: row.measurementTypeId,
      primaryMuscleId: row.primaryMuscleIds[0] ?? '',
      secondaryMuscleId: row.secondaryMuscleIds[0] ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  };

  const closeForm = () => {
    if (formLoading) {
      return;
    }

    setFormOpen(false);
    setFormError(null);
    setFormState(defaultFormState);
  };

  const submitForm = async () => {
    setFormLoading(true);
    setFormError(null);

    const request: UpsertExerciseRequest = {
      name: formState.name.trim(),
      code: formState.code.trim(),
      description: formState.description.trim() || null,
      category: formState.category,
      difficulty: formState.difficulty,
      measurementTypeId: formState.measurementTypeId,
      primaryMuscleIds: formState.primaryMuscleId ? [formState.primaryMuscleId] : [],
      secondaryMuscleIds: formState.secondaryMuscleId ? [formState.secondaryMuscleId] : [],
      images: [],
      videos: [],
    };

    try {
      if (formState.id) {
        await updateExercise(formState.id, request);
      } else {
        await createExercise(request);
      }

      closeForm();
      await loadRows();
    } catch (submitError) {
      const message = submitError instanceof Error ? submitError.message : t('administration.common.saveError');
      setFormError(message);
    } finally {
      setFormLoading(false);
    }
  };

  const onDelete = async (row: ExerciseDto) => {
    setError(null);

    try {
      await deleteExercise(row.id);
      await loadRows();
    } catch (deleteError) {
      const message = deleteError instanceof Error ? deleteError.message : t('administration.common.deleteError');
      setError(message);
    }
  };

  const onReactivate = async (row: ExerciseDto) => {
    setError(null);

    try {
      await reactivateExercise(row.id);
      await loadRows();
    } catch (reactivateError) {
      const message = reactivateError instanceof Error
        ? reactivateError.message
        : t('administration.common.reactivateError');
      setError(message);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={1}>
        <Typography variant="h5" fontWeight={700}>{t('administration.exercises.title')}</Typography>
        <Button variant="contained" onClick={openCreate}>{t('common.actions.add')}</Button>
      </Stack>

      <FormControlLabel
        control={<Checkbox checked={includeDeleted} onChange={(_event, checked) => setIncludeDeleted(checked)} />}
        label={t('administration.common.includeDeleted')}
      />

      {error ? (
        <Alert
          severity="error"
          action={<Button color="inherit" size="small" onClick={() => void loadRows()}>{t('administration.common.retry')}</Button>}
        >
          {error}
        </Alert>
      ) : null}

      {!loading && rows.length === 0 ? (
        <Paper sx={{ p: 4 }}><Typography>{t('common.messages.noData')}</Typography></Paper>
      ) : null}

      {!loading && rows.length > 0 ? (
        <Paper>
          <Table aria-label={t('administration.exercises.table')}>
            <TableHead>
              <TableRow>
                <TableCell>{t('common.fields.code')}</TableCell>
                <TableCell>{t('administration.common.fields.name')}</TableCell>
                <TableCell>{t('administration.exercises.fields.category')}</TableCell>
                <TableCell>{t('administration.exercises.fields.difficulty')}</TableCell>
                <TableCell align="right">{t('common.actions.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.code}</TableCell>
                  <TableCell>{row.name}</TableCell>
                  <TableCell>{row.category}</TableCell>
                  <TableCell>{row.difficulty}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button size="small" onClick={() => openEdit(row)} disabled={row.isDeleted}>{t('common.actions.edit')}</Button>
                      {row.isDeleted ? (
                        <Button size="small" color="success" onClick={() => void onReactivate(row)}>{t('administration.common.reactivate')}</Button>
                      ) : (
                        <Button size="small" color="error" onClick={() => void onDelete(row)}>{t('common.actions.delete')}</Button>
                      )}
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      ) : null}

      <Dialog open={formOpen} onClose={closeForm} fullWidth maxWidth="sm">
        <DialogTitle>{formState.id ? t('administration.exercises.editTitle') : t('administration.exercises.createTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label={t('administration.common.fields.name')}
              value={formState.name}
              onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
              fullWidth
            />
            <TextField
              label={t('common.fields.code')}
              value={formState.code}
              onChange={(event) => setFormState((current) => ({ ...current, code: event.target.value }))}
              disabled={Boolean(formState.id)}
              helperText={formState.id ? t('common.messages.codeReadOnly') : undefined}
              fullWidth
            />
            <TextField
              label={t('common.fields.description')}
              value={formState.description}
              onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
              fullWidth
            />

            <FormControl fullWidth>
              <InputLabel id="exercise-category-label">{t('administration.exercises.fields.category')}</InputLabel>
              <Select
                labelId="exercise-category-label"
                label={t('administration.exercises.fields.category')}
                value={formState.category}
                onChange={(event) => setFormState((current) => ({ ...current, category: String(event.target.value) }))}
              >
                <MenuItem value="Strength">Strength</MenuItem>
                <MenuItem value="Cardio">Cardio</MenuItem>
                <MenuItem value="Mobility">Mobility</MenuItem>
                <MenuItem value="Stretching">Stretching</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel id="exercise-difficulty-label">{t('administration.exercises.fields.difficulty')}</InputLabel>
              <Select
                labelId="exercise-difficulty-label"
                label={t('administration.exercises.fields.difficulty')}
                value={formState.difficulty}
                onChange={(event) => setFormState((current) => ({ ...current, difficulty: String(event.target.value) }))}
              >
                <MenuItem value="Beginner">Beginner</MenuItem>
                <MenuItem value="Intermediate">Intermediate</MenuItem>
                <MenuItem value="Advanced">Advanced</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel id="exercise-measurement-type-label">{t('administration.exercises.fields.measurementType')}</InputLabel>
              <Select
                labelId="exercise-measurement-type-label"
                label={t('administration.exercises.fields.measurementType')}
                value={formState.measurementTypeId}
                onChange={(event) => setFormState((current) => ({ ...current, measurementTypeId: String(event.target.value) }))}
              >
                {referenceMeasurementTypes.map((row) => (
                  <MenuItem key={row.id} value={row.id}>{row.name}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel id="exercise-primary-muscle-label">{t('administration.exercises.fields.primaryMuscle')}</InputLabel>
              <Select
                labelId="exercise-primary-muscle-label"
                label={t('administration.exercises.fields.primaryMuscle')}
                value={formState.primaryMuscleId}
                onChange={(event) => setFormState((current) => ({ ...current, primaryMuscleId: String(event.target.value) }))}
              >
                {referenceMuscles.map((row) => (
                  <MenuItem key={row.id} value={row.id}>{row.name}</MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel id="exercise-secondary-muscle-label">{t('administration.exercises.fields.secondaryMuscle')}</InputLabel>
              <Select
                labelId="exercise-secondary-muscle-label"
                label={t('administration.exercises.fields.secondaryMuscle')}
                value={formState.secondaryMuscleId}
                onChange={(event) => setFormState((current) => ({ ...current, secondaryMuscleId: String(event.target.value) }))}
              >
                <MenuItem value="">-</MenuItem>
                {referenceMuscles.map((row) => (
                  <MenuItem key={row.id} value={row.id}>{row.name}</MenuItem>
                ))}
              </Select>
            </FormControl>

            {formError ? <Alert severity="error">{formError}</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeForm} disabled={formLoading}>{t('common.actions.cancel')}</Button>
          <Button onClick={() => void submitForm()} disabled={formLoading} variant="contained">{t('common.actions.save')}</Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
