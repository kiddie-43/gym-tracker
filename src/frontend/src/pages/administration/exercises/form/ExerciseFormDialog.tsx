import Alert from '@mui/material/Alert';
import Autocomplete from '@mui/material/Autocomplete';
import Checkbox from '@mui/material/Checkbox';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import type { IMeasurementType } from '../../../../interfaces/admin/measurementTypes/measurementTypes';
import type { IMuscle } from '../../../../interfaces/muscles/IMuscles';
import type { ExerciseFormState } from './exerciseForm';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';

type ExerciseFormDialogProps = {
  open: boolean;
  loading: boolean;
  error: string | null;
  formState: ExerciseFormState;
  referenceMeasurementTypes: IMeasurementType[];
  referenceMuscles: IMuscle[];
  onClose: () => void;
  onSubmit: () => void;
  onChange: (patch: Partial<ExerciseFormState>) => void;
};

export function ExerciseFormDialog({
  open,
  loading,
  error,
  formState,
  referenceMeasurementTypes,
  referenceMuscles,
  onClose,
  onSubmit,
  onChange,
}: ExerciseFormDialogProps) {
  const { t } = useTranslation();
  const selectedMeasurementTypes = referenceMeasurementTypes.filter(
    (m) => m.id !== undefined && (formState.measurementTypeIds ?? []).includes(m.id),
  );

  return (
    <PopupDialog
      open={open}
      title={formState.id ? t('administration.exercises.editTitle') : t('administration.exercises.createTitle')}
      onClose={onClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      isSaving={loading}
      onSubmit={onSubmit}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label={t('administration.common.fields.name')}
            value={formState.name ?? ''}
            onChange={(event) => onChange({ name: event.target.value })}
            fullWidth
          />
          <TextField
            label={t('common.fields.code')}
            value={formState.code ?? ''}
            onChange={(event) => onChange({ code: event.target.value })}
            disabled={Boolean(formState.id)}
            helperText={formState.id ? t('common.messages.codeReadOnly') : undefined}
            fullWidth
          />
        </Stack>

        <TextField
          label={t('common.fields.description')}
          value={formState.description ?? ''}
          onChange={(event) => onChange({ description: event.target.value })}
          fullWidth
        />

        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <FormControl fullWidth>
            <InputLabel id="exercise-difficulty-label">{t('administration.exercises.fields.difficulty')}</InputLabel>
            <Select
              labelId="exercise-difficulty-label"
              label={t('administration.exercises.fields.difficulty')}
              value={formState.difficulty ?? ''}
              onChange={(event) => onChange({ difficulty: String(event.target.value) })}
            >
              <MenuItem value="">-</MenuItem>
              <MenuItem value="Beginner">Beginner</MenuItem>
              <MenuItem value="Intermediate">Intermediate</MenuItem>
              <MenuItem value="Advanced">Advanced</MenuItem>
            </Select>
          </FormControl>

          <FormControl fullWidth>
            <InputLabel id="exercise-category-label">{t('administration.exercises.fields.category')}</InputLabel>
            <Select
              labelId="exercise-category-label"
              label={t('administration.exercises.fields.category')}
              value={formState.category ?? ''}
              onChange={(event) => onChange({ category: String(event.target.value) })}
            >
              <MenuItem value="">-</MenuItem>
              <MenuItem value="Strength">Strength</MenuItem>
              <MenuItem value="Cardio">Cardio</MenuItem>
              <MenuItem value="Mobility">Mobility</MenuItem>
              <MenuItem value="Stretching">Stretching</MenuItem>
            </Select>
          </FormControl>
        </Stack>

        <Autocomplete
          multiple
          options={referenceMeasurementTypes}
          value={selectedMeasurementTypes}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onChange({ measurementTypeIds: value.map((m) => m.id!).filter(Boolean) })}
          renderInput={(params) => (
            <TextField
              {...params}
              label={t('administration.exercises.fields.measurementType')}
              placeholder={t('administration.exercises.fields.measurementType')}
            />
          )}
          renderOption={(props, option, { selected }) => (
            <li {...props} key={option.id}>
              <Checkbox checked={selected} />
              {option.name}
            </li>
          )}
        />

        <Autocomplete
          multiple
          options={referenceMuscles}
          value={formState.primaryMuscles}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onChange({ primaryMuscles: value })}
          renderInput={(params) => (
            <TextField
              {...params}
              label={t('administration.exercises.fields.primaryMuscle')}
              placeholder={t('administration.exercises.fields.primaryMuscle')}
            />
          )}
          renderOption={(props, option, { selected }) => (
            <li {...props} key={option.id}>
              <Checkbox checked={selected} />
              {option.name}
            </li>
          )}
        />

        <Autocomplete
          multiple
          options={referenceMuscles}
          value={formState.secondaryMuscles}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onChange({ secondaryMuscles: value })}
          renderInput={(params) => (
            <TextField
              {...params}
              label={t('administration.exercises.fields.secondaryMuscle')}
              placeholder={t('administration.exercises.fields.secondaryMuscle')}
            />
          )}
          renderOption={(props, option, { selected }) => (
            <li {...props} key={option.id}>
              <Checkbox checked={selected} />
              {option.name}
            </li>
          )}
        />

        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
