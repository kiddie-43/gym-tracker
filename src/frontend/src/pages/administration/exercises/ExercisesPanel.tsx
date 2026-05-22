import { useEffect } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Badge from '@mui/material/Badge';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IExercise, IImportExerciseCsvRowRequest } from '../../../interfaces/admin/exercises/exercises';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import {
  setAdminExercisesCsvResult,
  setAdminExercisesError,
  setAdminExercisesFilters,
  setAdminExercisesForm,
  setAdminExercisesPopUpCode,
  setAdminExercisesTable,
} from '../../../redux/actions/adminExercises/adminExercisesActions';
import {
  deleteAdminExercises,
  fetchAdminExercises,
  importAdminExercisesCsv,
  loadAdminExercisesReferenceData,
  reactivateAdminExercise,
  submitAdminExerciseForm,
} from '../../../redux/actions/adminExercises/adminExercisesThunks';
import {
  adminExercisesInitialState,
  selectAdminExercisesState,
} from '../../../redux/states/adminExercises/adminExercisesState';
import { ExercisesCsvImportDialog } from './ExercisesCsvImportDialog';
import { ExerciseFormDialog } from './form/ExerciseFormDialog';
import { ExercisesFiltersDrawer } from './filters/ExercisesFiltersDrawer';
import { ExercisesTable } from './table/ExercisesTable';

export function ExercisesPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const {
    table,
    filters,
    form,
    csvResult,
    referenceMuscles,
    referenceMeasurementTypes,
    loading,
    error,
    popUpCode,
  } = useAppSelector(selectAdminExercisesState);

  const { list: rows, page, rowsPerPage, totalCount, sortBy, sortDirection, selectedIds } = table;

  const hasActiveFilters =
    filters.search !== '' ||
    filters.includeDeleted ||
    filters.difficulties.length > 0 ||
    filters.measurementTypeIds.length > 0 ||
    filters.primaryMuscleIds.length > 0 ||
    filters.secondaryMuscleIds.length > 0;

  const formOpen = popUpCode === 'CREATE' || popUpCode === 'EDIT';
  const filtersOpen = popUpCode === 'FILTERS';
  const deleteDialogOpen = popUpCode === 'DELETE';
  const csvOpen = popUpCode === 'CSV_IMPORT';

  const referenceMusclesActive = referenceMuscles.filter((m) => !m.isDeleted);
  const referenceMeasurementTypesActive = referenceMeasurementTypes.filter((m) => !m.isDeleted);

  const selectedRows = rows.filter(
    (row): row is IExercise & { id: string } =>
      row.id !== undefined && selectedIds.includes(row.id) && !row.isDeleted,
  );

  useEffect(() => {
    dispatch(setAdminExercisesTable(adminExercisesInitialState.table));
    void dispatch(fetchAdminExercises());
  }, [dispatch]);

  useEffect(() => {
    void dispatch(loadAdminExercisesReferenceData());
  }, [dispatch]);

  const handleSortChange = (field: 'code' | 'name' | 'category' | 'difficulty') => {
    const newDirection = table.sortBy === field && table.sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setAdminExercisesTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchAdminExercises());
  };

  const openCreate = () => {
    dispatch(setAdminExercisesForm({ ...adminExercisesInitialState.form }));
    dispatch(setAdminExercisesError(null));
    dispatch(setAdminExercisesPopUpCode('CREATE'));
  };

  const openEdit = (row: IExercise) => {
    dispatch(setAdminExercisesForm({ ...row }));
    dispatch(setAdminExercisesError(null));
    dispatch(setAdminExercisesPopUpCode('EDIT'));
  };

  const closeForm = () => {
    dispatch(setAdminExercisesPopUpCode(null));
    dispatch(setAdminExercisesError(null));
  };

  const submitForm = async () => {
    const result = await dispatch(submitAdminExerciseForm());
    if (submitAdminExerciseForm.fulfilled.match(result)) {
      closeForm();
      void dispatch(fetchAdminExercises());
    }
  };

  const openDeleteDialog = (targets: (IExercise & { id: string })[]) => {
    dispatch(setAdminExercisesTable({ ...table, selectedIds: targets.map((row) => row.id) }));
    dispatch(setAdminExercisesPopUpCode('DELETE'));
  };

  const handleDelete = async () => {
    const result = await dispatch(deleteAdminExercises(selectedRows.map((row) => row.id)));
    if (deleteAdminExercises.fulfilled.match(result)) {
      dispatch(setAdminExercisesTable({ ...table, selectedIds: [] }));
      dispatch(setAdminExercisesPopUpCode(null));
      void dispatch(fetchAdminExercises());
    }
  };

  const handleReactivate = async (row: IExercise) => {
    if (!row.id) return;
    const result = await dispatch(reactivateAdminExercise(row.id));
    if (reactivateAdminExercise.fulfilled.match(result)) {
      void dispatch(fetchAdminExercises());
    }
  };

  const handleCsvImport = async (importRows: IImportExerciseCsvRowRequest[]) => {
    await dispatch(importAdminExercisesCsv(importRows));
    void dispatch(fetchAdminExercises());
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.exercises.title')}</Typography>
        <Button
          variant="outlined"
          startIcon={
            <Badge variant="dot" color="primary" invisible={!hasActiveFilters}>
              <FilterListRoundedIcon />
            </Badge>
          }
          onClick={() => dispatch(setAdminExercisesPopUpCode('FILTERS'))}
        >
          {t('administration.common.filters')}
        </Button>
      </Stack>

      <Stack direction="row" justifyContent="flex-end" spacing={1}>
        {selectedRows.length > 0 ? (
          <Button variant="outlined" color="error" onClick={() => openDeleteDialog(selectedRows)}>
            {t('common.actions.deleteSelected')}
          </Button>
        ) : null}
        <Button variant="outlined" onClick={() => void dispatch(fetchAdminExercises())}>
          {t('common.actions.reload')}
        </Button>
        {supportsImport ? (
          <Button variant="outlined" onClick={() => dispatch(setAdminExercisesPopUpCode('CSV_IMPORT'))}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>{t('common.actions.create')}</Button>
      </Stack>

      <ExercisesFiltersDrawer
        open={filtersOpen}
        searchInput={filters.search}
        includeDeleted={filters.includeDeleted}
        selectedDifficulties={filters.difficulties}
        selectedMeasurementTypeIds={filters.measurementTypeIds}
        selectedPrimaryMuscleIds={filters.primaryMuscleIds}
        selectedSecondaryMuscleIds={filters.secondaryMuscleIds}
        difficultyOptions={['Beginner', 'Intermediate', 'Advanced']}
        measurementTypeOptions={referenceMeasurementTypesActive}
        muscleOptions={referenceMusclesActive}
        onClose={() => dispatch(setAdminExercisesPopUpCode(null))}
        onSearchInputChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, search: v }))}
        onIncludeDeletedChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, includeDeleted: v }))}
        onSelectedDifficultiesChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, difficulties: v }))}
        onSelectedMeasurementTypeIdsChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, measurementTypeIds: v }))}
        onSelectedPrimaryMuscleIdsChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, primaryMuscleIds: v }))}
        onSelectedSecondaryMuscleIdsChange={(v) => dispatch(setAdminExercisesFilters({ ...filters, secondaryMuscleIds: v }))}
        onApplySearch={() => {
          dispatch(setAdminExercisesTable({ ...table, page: 0 }));
          void dispatch(fetchAdminExercises());
          dispatch(setAdminExercisesPopUpCode(null));
        }}
        onClearFilters={() => {
          dispatch(setAdminExercisesFilters(adminExercisesInitialState.filters));
          dispatch(setAdminExercisesTable({ ...table, page: 0 }));
          void dispatch(fetchAdminExercises());
        }}
      />

      {error ? (
        <FeedbackMessage type="error" message={error} />
      ) : null}

      {loading ? (
        <Stack alignItems="center" justifyContent="center" sx={{ flex: 1 }}>
          <CircularProgress aria-label={t('administration.common.loading')} />
        </Stack>
      ) : null}

      {!loading && !error && rows.length === 0 ? (
        <FeedbackMessage type="empty" message={t('common.messages.noData')} />
      ) : null}

      {!loading && rows.length > 0 ? (
        <Box sx={{ flex: 1, minHeight: 0, display: 'flex', flexDirection: 'column' }}>
          <ExercisesTable
            rows={rows}
            selectedIds={selectedIds}
            sortBy={sortBy}
            sortDirection={sortDirection}
            onSortChange={handleSortChange}
            page={page}
            rowsPerPage={rowsPerPage}
            totalCount={totalCount}
            onPageChange={(_event, newPage) => {
              dispatch(setAdminExercisesTable({ ...table, page: newPage }));
              void dispatch(fetchAdminExercises());
            }}
            onRowsPerPageChange={(event) => {
              dispatch(setAdminExercisesTable({
                ...table,
                rowsPerPage: Number.parseInt(event.target.value, 10),
                page: 0,
              }));
              void dispatch(fetchAdminExercises());
            }}
            onSelectionChange={(ids) => dispatch(setAdminExercisesTable({ ...table, selectedIds: ids }))}
            onEdit={openEdit}
            onDelete={(row) => { if (row.id) openDeleteDialog([row as IExercise & { id: string }]); }}
            onReactivate={(row) => void handleReactivate(row)}
          />
        </Box>
      ) : null}

      <ExerciseFormDialog
        open={formOpen}
        loading={loading}
        error={error}
        formState={form}
        referenceMeasurementTypes={referenceMeasurementTypesActive}
        referenceMuscles={referenceMusclesActive}
        onClose={closeForm}
        onSubmit={() => { void submitForm(); }}
        onChange={(patch) => dispatch(setAdminExercisesForm({ ...form, ...patch }))}
      />

      <ExercisesCsvImportDialog
        open={csvOpen}
        loading={loading}
        error={error}
        result={csvResult}
        onClose={() => {
          dispatch(setAdminExercisesPopUpCode(null));
          dispatch(setAdminExercisesError(null));
          dispatch(setAdminExercisesCsvResult(null));
        }}
        onImport={handleCsvImport}
      />

      <PopupDialog
        open={deleteDialogOpen}
        title={t('common.messages.confirmDelete')}
        onClose={() => dispatch(setAdminExercisesPopUpCode(null))}
        onSubmit={() => { void handleDelete(); }}
        closeLabel={t('common.actions.cancel')}
        saveLabel={t('common.actions.delete')}
        isSaving={loading}
      >
        <Typography>
          {t('administration.exercises.deleteManyWarning', { count: selectedRows.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
