import Autocomplete from '@mui/material/Autocomplete';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import Drawer from '@mui/material/Drawer';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';

import type { IUnit } from '../../../../interfaces/units/IUnit';
import type { IExercisesFilter } from '../../../../interfaces/IExercises/IExercises';
import type { IMuscle } from '../../../../interfaces/muscles/IMuscles';
import { updateExerciseFilterAction } from '../../../../redux/actions/exercises/exercisesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import {listUnitsPage } from '../../../../services/api/units/unitsApi';
import { listMusclesPage } from '../../../../services/api/muscles/musclesApi';

type ExercisesFiltersDrawerProps = {
  open: boolean;
  onClose: () => void;
  onApplySearch: () => void;
  onClearFilters: () => void;
};

export function ExercisesFiltersDrawer({
  open,
  onClose,
  onApplySearch,
  onClearFilters,
}: ExercisesFiltersDrawerProps) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { filters } = useAppSelector((state) => state.exercises);
  const [measurementTypeOptions, setMeasurementTypeOptions] = useState<IUnit[]>([]);
  const [muscleOptions, setMuscleOptions] = useState<IMuscle[]>([]);

  useEffect(() => {
    let isMounted = true;

    const loadReferences = async () => {
      try {
        const [musclesPage, unitsPage] = await Promise.all([
          listMusclesPage({ page: 0, pageSize: 999 }),
          listUnitsPage({
            page: 0, pageSize: 999,
            code: '',
            name: '',
            description: ''
          }),
        ]);

        if (!isMounted) {
          return;
        }

        setMuscleOptions((musclesPage.items ?? []));
        setMeasurementTypeOptions((unitsPage.items ?? []));
      } catch {
        if (!isMounted) {
          return;
        }

        setMuscleOptions([]);
        setMeasurementTypeOptions([]);
      }
    };

    void loadReferences();

    return () => {
      isMounted = false;
    };
  }, []);

  const onEditFilter = (key: keyof IExercisesFilter, value: unknown) => {
    dispatch(updateExerciseFilterAction({ key, value }));
  };

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
            {t('common.filters')}
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.85 }}>
            {t('app.tagline')}
          </Typography>
        </Box>
        <Stack spacing={2} sx={{ width: { xs: 280, sm: 340 }, p: 2 }}>
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={onClearFilters}>
              {t('common.clearFiltersAction')}
            </Button>
            <Button variant="contained" onClick={onApplySearch}>
              {t('common.search')}
            </Button>
          </Stack>
        
        
          <Autocomplete
            multiple
            options={measurementTypeOptions}
            disableCloseOnSelect
            value={measurementTypeOptions.filter((item) => item.id !== undefined && (filters.unitId ?? []).includes(item.id ))}
            getOptionLabel={(option) => `${option.name} (${option.code ?? ''})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onEditFilter('unitId', value.map((item) => item.id!))}
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
                {`${option.name} (${option.code ?? ''})`}
              </li>
            )}
          />
          <Autocomplete
            multiple
            options={muscleOptions}
            disableCloseOnSelect
            value={muscleOptions.filter((item) => item.id !== undefined && (filters.primaryMuscleId ?? []).includes(item.id))}
            getOptionLabel={(option) => `${option.name} (${option.code ?? ''})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onEditFilter('primaryMuscleId', value.map((item) => item.id!))}
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
                {`${option.name} (${option.code ?? ''})`}
              </li>
            )}
          />
          <Autocomplete
            multiple
            options={muscleOptions}
            disableCloseOnSelect
            value={muscleOptions.filter((item) => item.id !== undefined && (filters.secondaryMuscleId ?? []).includes(item.id))}
            getOptionLabel={(option) => `${option.name} (${option.code ?? ''})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_event, value) => onEditFilter('secondaryMuscleId', value.map((item) => item.id!))}
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
                {`${option.name} (${option.code ?? ''})`}
              </li>
            )}
          />
         
        </Stack>
      </Drawer>
  );
}
