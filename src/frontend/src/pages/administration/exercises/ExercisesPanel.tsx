import { useCallback, useEffect, useMemo, useState } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { ExerciseDto, ImportExerciseCsvRowRequest, ImportExercisesResult, UpsertExerciseRequest } from '../../../interfaces/admin/exercises/exercises';
import type { MeasurementTypeDto } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import type { MuscleDto } from '../../../interfaces/admin/muscles/muscles';
import {
  createExercise,
  deleteExercise,
  importExercisesCsv,
  listExercisesPage,
  reactivateExercise,
  updateExercise,
} from '../../../services/api/admin/exercises/exercisesApi';
import { listMeasurementTypesPage } from '../../../services/api/admin/measurementTypes/measurementTypesApi';
import { listMusclesPage } from '../../../services/api/admin/muscles/musclesApi';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { ExercisesCsvImportDialog } from './ExercisesCsvImportDialog';
import { ExerciseFormDialog } from './form/ExerciseFormDialog';
import { defaultExerciseFormState, type ExerciseFormState } from './form/exerciseForm';
import { ExercisesFiltersDrawer } from './filters/ExercisesFiltersDrawer';
import { ExercisesTable } from './table/ExercisesTable';

export function ExercisesPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const [rows, setRows] = useState<ExerciseDto[]>([]);
  const [muscles, setMuscles] = useState<MuscleDto[]>([]);
  const [measurementTypes, setMeasurementTypes] = useState<MeasurementTypeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [searchInput, setSearchInput] = useState('');
  const [selectedDifficulties, setSelectedDifficulties] = useState<string[]>([]);
  const [selectedMeasurementTypeIds, setSelectedMeasurementTypeIds] = useState<string[]>([]);
  const [selectedPrimaryMuscleIds, setSelectedPrimaryMuscleIds] = useState<string[]>([]);
  const [selectedSecondaryMuscleIds, setSelectedSecondaryMuscleIds] = useState<string[]>([]);
  const [appliedFilters, setAppliedFilters] = useState({
    search: '',
    difficulties: [] as string[],
    measurementTypeIds: [] as string[],
    primaryMuscleIds: [] as string[],
    secondaryMuscleIds: [] as string[],
  });
  const [totalCount, setTotalCount] = useState<number>(0);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [sortBy, setSortBy] = useState<'code' | 'name' | 'category' | 'difficulty'>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState<number>(0);
  const [rowsPerPage, setRowsPerPage] = useState<number>(10);

  const [formOpen, setFormOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<ExerciseFormState>(defaultExerciseFormState);

  const [csvOpen, setCsvOpen] = useState(false);
  const [csvLoading, setCsvLoading] = useState(false);
  const [csvError, setCsvError] = useState<string | null>(null);
  const [csvResult, setCsvResult] = useState<ImportExercisesResult | null>(null);

  const [selectedRowIds, setSelectedRowIds] = useState<string[]>([]);
  const [deleteTargets, setDeleteTargets] = useState<ExerciseDto[]>([]);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const referenceMuscles = muscles.filter((item) => !item.isDeleted);
  const referenceMeasurementTypes = measurementTypes.filter((item) => !item.isDeleted);
  const selectedRows = useMemo(
    () => rows.filter((row) => selectedRowIds.includes(row.id) && !row.isDeleted),
    [rows, selectedRowIds],
  );

  const openDeleteDialog = (targets: ExerciseDto[]) => {
    const rowsToDelete = targets.filter((row) => !row.isDeleted);
    if (rowsToDelete.length === 0) return;
    setDeleteTargets(rowsToDelete);
  };

  const closeDeleteDialog = () => {
    if (deleteLoading) return;
    setDeleteTargets([]);
  };

  const handleDelete = async () => {
    if (deleteTargets.length === 0) return;
    setDeleteLoading(true);
    setError(null);

    try {
      const results = await Promise.allSettled(deleteTargets.map((row) => deleteExercise(row.id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;

      if (failedCount > 0) {
        setError(t('administration.exercises.bulkDeleteError', { failed: failedCount, total: deleteTargets.length }));
      }

      setDeleteTargets([]);
      setSelectedRowIds([]);
      await loadRows();
    } catch (deleteError) {
      const message = deleteError instanceof Error ? deleteError.message : t('administration.common.deleteError');
      setError(message);
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleSortChange = (field: 'code' | 'name' | 'category' | 'difficulty') => {
    setPage(0);

    if (sortBy === field) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'));
      return;
    }

    setSortBy(field);
    setSortDirection('asc');
  };

  const loadRows = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await listExercisesPage({
        includeDeleted,
        search: appliedFilters.search || undefined,
        difficulties: appliedFilters.difficulties.length > 0 ? appliedFilters.difficulties : undefined,
        measurementTypeIds: appliedFilters.measurementTypeIds.length > 0 ? appliedFilters.measurementTypeIds : undefined,
        primaryMuscleIds: appliedFilters.primaryMuscleIds.length > 0 ? appliedFilters.primaryMuscleIds : undefined,
        secondaryMuscleIds: appliedFilters.secondaryMuscleIds.length > 0 ? appliedFilters.secondaryMuscleIds : undefined,
        sortBy,
        sortDirection,
        page: page + 1,
        pageSize: rowsPerPage,
      });
      setRows(data.items);
      setTotalCount(data.totalCount);
    } catch (loadError) {
      const message = loadError instanceof Error ? loadError.message : t('administration.common.loadError');
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [includeDeleted, appliedFilters, sortBy, sortDirection, page, rowsPerPage, t]);

  useEffect(() => {
    void (async () => {
      try {
        const [muscleRows, measurementRows] = await Promise.all([
          listMusclesPage({ includeDeleted: true, pageSize: 1000 }).then(p => p.items),
          listMeasurementTypesPage({ includeInactive: true, pageSize: 1000 }).then(p => p.items),
        ]);
        setMuscles(muscleRows);
        setMeasurementTypes(measurementRows);
      } catch {
        // reference data errors are non-critical
      }
    })();
  }, []);

  useEffect(() => {
    void loadRows();
  }, [loadRows]);

  const openCreate = () => {
    setFormState((current) => ({
      ...defaultExerciseFormState,
      measurementTypeIds: referenceMeasurementTypes[0]?.id ? [referenceMeasurementTypes[0].id] : [],
      primaryMuscleIds: referenceMuscles[0]?.id ? [referenceMuscles[0].id] : [],
      secondaryMuscleIds: [],
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
      measurementTypeIds: row.measurementTypeId ? [row.measurementTypeId] : [],
      primaryMuscleIds: row.primaryMuscles.map(m => m.id),
      secondaryMuscleIds: row.secondaryMuscles.map(m => m.id),
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
    setFormState(defaultExerciseFormState);
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
      measurementTypeId: formState.measurementTypeIds[0] ?? '',
      primaryMuscleIds: formState.primaryMuscleIds,
      secondaryMuscleIds: formState.secondaryMuscleIds,
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

  const updateFormState = (patch: Partial<ExerciseFormState>) => {
    setFormState((current) => ({ ...current, ...patch }));
  };

  const handleCsvImport = async (rows: ImportExerciseCsvRowRequest[]) => {
    setCsvLoading(true);
    setCsvError(null);
    setCsvResult(null);

    try {
      const result = await importExercisesCsv({ rows });
      setCsvResult(result);
      await loadRows();
    } catch (importError) {
      const message = importError instanceof Error ? importError.message : t('administration.exercises.csv.importError');
      setCsvError(message);
    } finally {
      setCsvLoading(false);
    }
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.exercises.title')}</Typography>
        <Button
          variant="outlined"
          startIcon={<FilterListRoundedIcon />}
          onClick={() => setFiltersOpen(true)}
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
        <Button variant="outlined" onClick={() => void loadRows()}>
          {t('common.actions.reload')}
        </Button>
        {supportsImport ? (
          <Button variant="outlined" onClick={() => setCsvOpen(true)}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>{t('common.actions.create')}</Button>
      </Stack>

      <ExercisesFiltersDrawer
        open={filtersOpen}
        searchInput={searchInput}
        includeDeleted={includeDeleted}
        selectedDifficulties={selectedDifficulties}
        selectedMeasurementTypeIds={selectedMeasurementTypeIds}
        selectedPrimaryMuscleIds={selectedPrimaryMuscleIds}
        selectedSecondaryMuscleIds={selectedSecondaryMuscleIds}
        difficultyOptions={['Beginner', 'Intermediate', 'Advanced']}
        measurementTypeOptions={referenceMeasurementTypes}
        muscleOptions={referenceMuscles}
        onClose={() => setFiltersOpen(false)}
        onSearchInputChange={setSearchInput}
        onIncludeDeletedChange={setIncludeDeleted}
        onSelectedDifficultiesChange={setSelectedDifficulties}
        onSelectedMeasurementTypeIdsChange={setSelectedMeasurementTypeIds}
        onSelectedPrimaryMuscleIdsChange={setSelectedPrimaryMuscleIds}
        onSelectedSecondaryMuscleIdsChange={setSelectedSecondaryMuscleIds}
        onApplySearch={() => {
          setAppliedFilters({
            search: searchInput,
            difficulties: selectedDifficulties,
            measurementTypeIds: selectedMeasurementTypeIds,
            primaryMuscleIds: selectedPrimaryMuscleIds,
            secondaryMuscleIds: selectedSecondaryMuscleIds,
          });
          setPage(0);
        }}
        onClearFilters={() => {
          setSearchInput('');
          setIncludeDeleted(false);
          setSelectedDifficulties([]);
          setSelectedMeasurementTypeIds([]);
          setSelectedPrimaryMuscleIds([]);
          setSelectedSecondaryMuscleIds([]);
          setAppliedFilters({
            search: '',
            difficulties: [],
            measurementTypeIds: [],
            primaryMuscleIds: [],
            secondaryMuscleIds: [],
          });
          setPage(0);
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
            selectedIds={selectedRowIds}
            sortBy={sortBy}
            sortDirection={sortDirection}
            onSortChange={handleSortChange}
            page={page}
            rowsPerPage={rowsPerPage}
            totalCount={totalCount}
            onPageChange={(_event, newPage) => setPage(newPage)}
            onRowsPerPageChange={(event) => {
              setRowsPerPage(Number.parseInt(event.target.value, 10));
              setPage(0);
            }}
            onSelectionChange={setSelectedRowIds}
            onEdit={openEdit}
            onDelete={(row) => openDeleteDialog([row])}
            onReactivate={(row) => void onReactivate(row)}
          />
        </Box>
      ) : null}

      <ExerciseFormDialog
        open={formOpen}
        loading={formLoading}
        error={formError}
        formState={formState}
        referenceMeasurementTypes={referenceMeasurementTypes}
        referenceMuscles={referenceMuscles}
        onClose={closeForm}
        onSubmit={() => void submitForm()}
        onChange={updateFormState}
      />

      <ExercisesCsvImportDialog
        open={csvOpen}
        loading={csvLoading}
        error={csvError}
        result={csvResult}
        onClose={() => {
          if (csvLoading) return;
          setCsvOpen(false);
          setCsvError(null);
          setCsvResult(null);
        }}
        onImport={handleCsvImport}
      />

      <PopupDialog
        open={deleteTargets.length > 0}
        title={t('common.messages.confirmDelete')}
        onClose={closeDeleteDialog}
        onSubmit={() => { void handleDelete(); }}
        closeLabel={t('common.actions.cancel')}
        saveLabel={t('common.actions.delete')}
        isSaving={deleteLoading}
      >
        <Typography>
          {t('administration.exercises.deleteManyWarning', { count: deleteTargets.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
