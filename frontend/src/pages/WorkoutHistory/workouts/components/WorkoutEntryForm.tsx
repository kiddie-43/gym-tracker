import { useMemo } from 'react';

import Autocomplete from '@mui/material/Autocomplete';
import Avatar from '@mui/material/Avatar';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import MenuItem from '@mui/material/MenuItem';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import AddRoundedIcon from '@mui/icons-material/AddRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';

import { useAppDispatch, useAppSelector } from '../../../../redux/hooks/reduxHooks';
import type { Exercise } from '../../../../shared/types/catalog';
import type { CreateWorkoutRequest } from '../../../../shared/types/workouts';
import {
  addFormSetEntry,
  removeFormSetEntry,
  setFormInputValue,
  setFormPerformedAt,
  setFormStatus,
  setSelectedExerciseId,
  updateFormSetEntry,
} from '../../../../redux/actions/workoutsActions';

type WorkoutEntryFormProps = {
  onSubmit: (request: CreateWorkoutRequest) => Promise<void> | void;
  exercises: Exercise[];
  formId?: string;
  hideSubmitButton?: boolean;
  isSubmitting?: boolean;
  submitLabel?: string;
};

function resolveExerciseImageUrl(imageUrl?: string | null): string | undefined {
  if (!imageUrl) {
    return undefined;
  }

  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl;
  }

  if (imageUrl.startsWith('/')) {
    return `https://wger.de${imageUrl}`;
  }

  return `https://wger.de/${imageUrl}`;
}

export function WorkoutEntryForm({
  onSubmit,
  exercises,
  formId,
  hideSubmitButton = false,
  isSubmitting = false,
  submitLabel = 'Guardar entrenamiento',
}: WorkoutEntryFormProps) {
  const dispatch = useAppDispatch();
  const { selectedExerciseId, inputValue, performedAt, status, setEntries } = useAppSelector(
    (state) => state.workouts.formDraft,
  );

  const selectedExercise = useMemo(
    () =>
      exercises.find((exercise) => exercise.id === selectedExerciseId)
      ?? exercises.find((exercise) => exercise.name.toLocaleLowerCase('es-ES') === inputValue.toLocaleLowerCase('es-ES'))
      ?? null,
    [exercises, selectedExerciseId, inputValue],
  );

  const filteredExercises = useMemo(() => {
    const query = inputValue.trim().toLocaleLowerCase('es-ES');

    if (query.length < 2) {
      return [];
    }

    return exercises
      .map((exercise) => ({
        exercise,
        normalizedName: exercise.name.toLocaleLowerCase('es-ES'),
      }))
      .filter(({ normalizedName }) => normalizedName.includes(query))
      .sort((a, b) => {
        const aIndex = a.normalizedName.indexOf(query);
        const bIndex = b.normalizedName.indexOf(query);

        if (aIndex !== bIndex) {
          return aIndex - bIndex;
        }

        return a.normalizedName.localeCompare(b.normalizedName, 'es');
      })
      .slice(0, 40)
      .map(({ exercise }) => exercise);
  }, [exercises, inputValue]);

  function addSetEntry() {
    dispatch(addFormSetEntry());
  }

  function removeSetEntry(index: number) {
    dispatch(removeFormSetEntry(index));
  }

  function updateSetEntry(index: number, field: 'repetitions' | 'weight', value: string) {
    dispatch(updateFormSetEntry({ index, field, value }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedExercise) {
      return;
    }

    const setsToSubmit = setEntries.length > 0 ? setEntries : [{ repetitions: '8', weight: '100' }];

    await onSubmit({
      performedAt: new Date(performedAt).toISOString(),
      status,
      notes: '',
      exerciseEntries: [
        {
          externalExerciseId: selectedExercise.id,
          exerciseName: selectedExercise.name,
          muscleGroupIds: selectedExercise.muscleGroupIds,
          imageUrl: selectedExercise.imageUrl,
          sets: setsToSubmit.map((setEntry) => ({
            repetitions: Number(setEntry.repetitions),
            weight: Number(setEntry.weight),
            restSeconds: 120,
            completed: true,
          })),
        },
      ],
    });
  }

  return (
    <Stack component="form" id={formId} spacing={2} onSubmit={handleSubmit}>
      <Autocomplete
        options={filteredExercises}
        value={selectedExercise}
        inputValue={inputValue}
        onChange={(_, value) => {
          dispatch(setSelectedExerciseId(value?.id ?? null));
          dispatch(setFormInputValue(value?.name ?? ''));
        }}
        onInputChange={(_, value, reason) => {
          dispatch(setFormInputValue(value));
          if (reason === 'input') {
            dispatch(setSelectedExerciseId(null));
          }
        }}
        autoHighlight
        isOptionEqualToValue={(option, value) => option.id === value.id}
        getOptionLabel={(option) => option.name}
        renderOption={(props, option) => (
          <li {...props}>
            <Stack direction="row" spacing={1.25} alignItems="center" sx={{ width: '100%' }}>
              <Avatar
                src={resolveExerciseImageUrl(option.imageUrl)}
                alt={option.name}
                variant="rounded"
                sx={{ width: 36, height: 36, fontSize: 12, bgcolor: 'action.hover' }}
              >
                {option.name.slice(0, 2).toUpperCase()}
              </Avatar>
              <Stack spacing={0.15} sx={{ minWidth: 0 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }} noWrap>
                  {option.name}
                </Typography>
                <Typography variant="caption" color="text.secondary" noWrap>
                  {option.imageUrl ? 'Con imagen' : 'Sin imagen'}
                </Typography>
              </Stack>
            </Stack>
          </li>
        )}
        noOptionsText={inputValue.trim().length < 2 ? 'Escribe al menos 2 letras para buscar.' : 'No se encontraron ejercicios.'}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Ejercicio"
            size="small"
            InputProps={{
              ...params.InputProps,
              startAdornment: selectedExercise ? (
                <Stack direction="row" spacing={1} alignItems="center">
                  <Avatar
                    src={resolveExerciseImageUrl(selectedExercise.imageUrl)}
                    alt={selectedExercise.name}
                    variant="rounded"
                    sx={{ width: 28, height: 28, fontSize: 11, bgcolor: 'action.hover' }}
                  >
                    {selectedExercise.name.slice(0, 2).toUpperCase()}
                  </Avatar>
                  {params.InputProps.startAdornment}
                </Stack>
              ) : params.InputProps.startAdornment,
            }}
            helperText={exercises.length === 0
              ? 'No hay ejercicios disponibles en catalogo.'
              : 'Escribe al menos 2 letras para filtrar mas rapido.'}
          />
        )}
      />
      <TextField
        label="Realizado el"
        type="datetime-local"
        value={performedAt}
        onChange={(event) => dispatch(setFormPerformedAt(event.target.value))}
        size="small"
        InputLabelProps={{ shrink: true }}
      />
      <TextField
        size="small"
        select
        label="Estado"
        value={status}
        onChange={(event) => dispatch(setFormStatus(event.target.value as 'completed' | 'incomplete'))}
      >
        <MenuItem value="completed">Completado</MenuItem>
        <MenuItem value="incomplete">Incompleto</MenuItem>
      </TextField>
      <Stack spacing={1.5}>
        {setEntries.map((setEntry, index) => (
          <Stack key={`set-${index + 1}`} direction="row" spacing={1} alignItems="center" sx={{ pt: 0.25 }}>
            <Typography variant="body2" sx={{ fontWeight: 600, lineHeight: 1.25, minWidth: 'fit-content' }}>
              Serie {index + 1}
            </Typography>
            <TextField
              size="small"
              label="Repeticiones"
              type="number"
              value={setEntry.repetitions}
              onChange={(event) => updateSetEntry(index, 'repetitions', event.target.value)}
              inputProps={{ min: 1 }}
              sx={{ flex: 1, minWidth: 0 }}
            />
            <TextField
              size="small"
              label="Peso"
              type="number"
              value={setEntry.weight}
              onChange={(event) => updateSetEntry(index, 'weight', event.target.value)}
              inputProps={{ min: 0 }}
              sx={{ flex: 1, minWidth: 0 }}
            />
            <IconButton
              aria-label={`Eliminar serie ${index + 1}`}
              onClick={() => removeSetEntry(index)}
              disabled={setEntries.length <= 1}
              size="small"
            >
              <DeleteOutlineRoundedIcon fontSize="small" />
            </IconButton>
          </Stack>
        ))}
        <Button
          variant="outlined"
          startIcon={<AddRoundedIcon />}
          onClick={addSetEntry}
          disabled={setEntries.length >= 20}
          sx={{ alignSelf: 'center' }}
        >
          Anadir serie
        </Button>
      </Stack>
      {!hideSubmitButton ? (
        <Button type="submit" variant="contained" disabled={isSubmitting || !selectedExercise}>
          {submitLabel}
        </Button>
      ) : null}
    </Stack>
  );
}
