import Autocomplete from '@mui/material/Autocomplete';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import Drawer from '@mui/material/Drawer';
import FormControlLabel from '@mui/material/FormControlLabel';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { MeasurementTypeDto } from '../../../../interfaces/admin/measurementTypes/measurementTypes';
import type { MuscleDto } from '../../../../interfaces/admin/muscles/muscles';

type ExercisesFiltersDrawerProps = {
  open: boolean;
  searchInput: string;
  includeDeleted: boolean;
  selectedDifficulties: string[];
  selectedMeasurementTypeIds: string[];
  selectedPrimaryMuscleIds: string[];
  selectedSecondaryMuscleIds: string[];
  difficultyOptions: string[];
  measurementTypeOptions: MeasurementTypeDto[];
  muscleOptions: MuscleDto[];
  onClose: () => void;
  onSearchInputChange: (value: string) => void;
  onIncludeDeletedChange: (value: boolean) => void;
  onSelectedDifficultiesChange: (value: string[]) => void;
  onSelectedMeasurementTypeIdsChange: (value: string[]) => void;
  onSelectedPrimaryMuscleIdsChange: (value: string[]) => void;
  onSelectedSecondaryMuscleIdsChange: (value: string[]) => void;
  onApplySearch: () => void;
  onClearFilters: () => void;
};

export function ExercisesFiltersDrawer({
  open,
  searchInput,
  includeDeleted,
  selectedDifficulties,
  selectedMeasurementTypeIds,
  selectedPrimaryMuscleIds,
  selectedSecondaryMuscleIds,
  difficultyOptions,
  measurementTypeOptions,
  muscleOptions,
  onClose,
  onSearchInputChange,
  onIncludeDeletedChange,
  onSelectedDifficultiesChange,
  onSelectedMeasurementTypeIdsChange,
  onSelectedPrimaryMuscleIdsChange,
  onSelectedSecondaryMuscleIdsChange,
  onApplySearch,
  onClearFilters,
}: ExercisesFiltersDrawerProps) {
  const { t } = useTranslation();

  return (
    <Drawer anchor="right" open={open} onClose={onClose}>
        <Box
          sx={{
            px: 2,
            py: 2,
            background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
            color: '#f4f8f7',
          }}
        >
          <Typography variant="h6" fontWeight={800} sx={{ letterSpacing: 0.3, lineHeight: 1.2 }}>
            {t('administration.common.filters')}
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.85 }}>
            {t('app.tagline')}
          </Typography>
        </Box>
        <Stack spacing={2} sx={{ width: { xs: 280, sm: 340 }, p: 2 }}>
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={onClearFilters}>
              {t('administration.common.clearFiltersAction')}
            </Button>
            <Button variant="contained" onClick={onApplySearch}>
              {t('administration.common.searchAction')}
            </Button>
          </Stack>
          <TextField
            label={t('administration.common.searchLabel')}
            placeholder={t('administration.common.searchPlaceholder')}
            value={searchInput}
            onChange={(event) => onSearchInputChange(event.target.value)}
            fullWidth
          />
          <Autocomplete
            multiple
            options={difficultyOptions}
            value={selectedDifficulties}
            onChange={(_event, value) => onSelectedDifficultiesChange(value)}
            renderInput={(params) => (
              <TextField
                {...params}
                label={t('administration.exercises.fields.difficulty')}
                placeholder={t('administration.exercises.fields.difficulty')}
              />
            )}
          />
          <Autocomplete
            multiple
            options={measurementTypeOptions}
            value={measurementTypeOptions.filter((item) => selectedMeasurementTypeIds.includes(item.id))}
            getOptionLabel={(option) => `${option.name} (${option.key})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onSelectedMeasurementTypeIdsChange(value.map((item) => item.id))}
            renderInput={(params) => (
              <TextField
                {...params}
                label={t('administration.exercises.fields.measurementType')}
                placeholder={t('administration.exercises.fields.measurementType')}
              />
            )}
          />
          <Autocomplete
            multiple
            options={muscleOptions}
            value={muscleOptions.filter((item) => selectedPrimaryMuscleIds.includes(item.id))}
            getOptionLabel={(option) => `${option.name} (${option.code})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onSelectedPrimaryMuscleIdsChange(value.map((item) => item.id))}
            renderInput={(params) => (
              <TextField
                {...params}
                label={t('administration.exercises.fields.primaryMuscle')}
                placeholder={t('administration.exercises.fields.primaryMuscle')}
              />
            )}
          />
          <Autocomplete
            multiple
            options={muscleOptions}
            value={muscleOptions.filter((item) => selectedSecondaryMuscleIds.includes(item.id))}
            getOptionLabel={(option) => `${option.name} (${option.code})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onSelectedSecondaryMuscleIdsChange(value.map((item) => item.id))}
            renderInput={(params) => (
              <TextField
                {...params}
                label={t('administration.exercises.fields.secondaryMuscle')}
                placeholder={t('administration.exercises.fields.secondaryMuscle')}
              />
            )}
          />
          <FormControlLabel
            control={<Checkbox checked={includeDeleted} onChange={(_event, checked) => onIncludeDeletedChange(checked)} />}
            label={t('administration.common.includeDeleted')}
          />
        </Stack>
      </Drawer>
  );
}
