import Alert from '@mui/material/Alert';
import Autocomplete from '@mui/material/Autocomplete';
import Checkbox from '@mui/material/Checkbox';

import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import type { IUnit } from '../../../../interfaces/units/IUnit';
import type { IExercise } from '../../../../interfaces/IExercises/IExercises';
import type { IMuscle } from '../../../../interfaces/muscles/IMuscles';
import { updateExerciseFormAction } from '../../../../redux/actions/exercises/exercisesActions';
import { setExerciseDialogStateAction, submitExerciseFormAction } from '../../../../redux/actions/exercises/exercisesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { selectExercisesState } from '../../../../redux/states/exercises/exercisesState';
import { listMusclesPage } from '../../../../services/api/muscles/musclesApi';
import { listUnitsPage } from '../../../../services/api/units/unitsApi';

export function ExerciseFormDialog() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const {
    form: formState,
    loading,
    error,
    popUpCode,
  } = useAppSelector(selectExercisesState);
  const [localReferenceMeasurementTypes, setLocalReferenceMeasurementTypes] = useState<IUnit[]>([]);
  const [localReferenceMuscles, setLocalReferenceMuscles] = useState<IMuscle[]>([]);
  const [loadingReferences, setLoadingReferences] = useState(false);

  useEffect(() => {
    let isMounted = true;

    const loadReferences = async () => {
      setLoadingReferences(true);

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

        setLocalReferenceMuscles(musclesPage.items ?? []);
        setLocalReferenceMeasurementTypes(unitsPage.items ?? []);
      } finally {
        if (isMounted) {
          setLoadingReferences(false);
        }
      }
    };

    void loadReferences();

    return () => {
      isMounted = false;
    };
  }, []);

  const onEditForm = (key: keyof IExercise, value: unknown) => {
    dispatch(updateExerciseFormAction({ key, value }));
  };
  const open = popUpCode === PopUpCode.Create || popUpCode === PopUpCode.Update;
  const onClose = () => {
    dispatch(setExerciseDialogStateAction(PopUpCode.Default));
  };
  const onSubmit = () => {
    void dispatch(submitExerciseFormAction() as never);
  };
  const selectedMeasurementTypes = formState.units ?? [];
  const disableSave =
    loading ||
    loadingReferences ||
    (formState.name ?? '').trim().length === 0 ||
    (formState.primaryMuscles ?? []).length === 0 ||
    (formState.units ?? []).length === 0;

  return (
    <PopupDialog
      open={open}
      title={formState.id ? t('administration.exercises.editTitle') : t('administration.exercises.createTitle')}
      onClose={onClose}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('common.actions.save')}
      disableSave={disableSave}
      isSaving={loading}
      onSubmit={onSubmit}
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label={t('common.fields.name')}
            value={formState.name ?? ''}
            onChange={(event) => onEditForm('name', event.target.value)}
            fullWidth
          />
         {formState.id ? null : (
          <TextField
            label={t('common.fields.code')}
            value={formState.code ?? ''}
            onChange={(event) => onEditForm('code', event.target.value)}
            disabled={Boolean(formState.id)}
            helperText={formState.id ? t('common.messages.codeReadOnly') : undefined}
            fullWidth
          />
          )}
        </Stack>
        

        <TextField
          label={t('common.fields.description')}
          value={formState.description ?? ''}
          onChange={(event) => onEditForm('description', event.target.value)}
          fullWidth
        />

     

        <Autocomplete
          multiple
          options={localReferenceMeasurementTypes}
          value={selectedMeasurementTypes}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onEditForm('units', value)}
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
          options={localReferenceMuscles}
          value={formState.primaryMuscles ?? []}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onEditForm('primaryMuscles', value)}
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
          options={localReferenceMuscles}
          value={formState.secondaryMuscles ?? []}
          disableCloseOnSelect
          isOptionEqualToValue={(option, value) => option.id === value.id}
          getOptionLabel={(option) => option.name}
          onChange={(_event, value) => onEditForm('secondaryMuscles', value)}
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
