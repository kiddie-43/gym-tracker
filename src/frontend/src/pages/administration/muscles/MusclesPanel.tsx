import { useCallback, useEffect, useMemo, useState } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IImportMusclesResult, IMuscle, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import {
  createMuscle,
  deleteMuscle,
  editMuscle,
  getMusclesPaginated,
  importAdminMusclesCsv,
  reactivateAdminMuscle,
} from '../../../redux/actions/muscles/muscles';
import { setAdminMusclesError } from '../../../redux/actions/adminMuscles/adminMusclesActions';
import { selectAdminMusclesState } from '../../../redux/states/adminMuscles/adminMusclesState';
import { MusclesFiltersDrawer } from './filters/MusclesFiltersDrawer';
import { defaultMuscleFormState, type MuscleFormState } from './form/muscleForm';
import { MuscleFormDialog } from './form/MuscleFormDialog';
import { MusclesCsvImportDialog } from './MusclesCsvImportDialog';
import { MusclesTable } from './table/MusclesTable';

export function MusclesPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { list: rows, loading, error } = useAppSelector(selectAdminMusclesState);
  const [includeDeleted, setIncludeDeleted] = useState<boolean>(false);
  const [searchInput, setSearchInput] = useState<string>('');
  const [filterCode, setFilterCode] = useState<string>('');
  const [filterName, setFilterName] = useState<string>('');
  const [filtersOpen, setFiltersOpen] = useState<boolean>(false);
  const [selectedRowIds, setSelectedRowIds] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState<'code' | 'name' | 'description'>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState<number>(0);
  const [rowsPerPage, setRowsPerPage] = useState<number>(10);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [appliedSearch, setAppliedSearch] = useState<string>('');
  const [appliedCode, setAppliedCode] = useState<string>('');
  const [appliedName, setAppliedName] = useState<string>('');

  const [formOpen, setFormOpen] = useState<boolean>(false);
  const [formLoading, setFormLoading] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<MuscleFormState>(defaultMuscleFormState);

  const [deleteTargets, setDeleteTargets] = useState<IMuscle[]>([]);
  const [deleteLoading, setDeleteLoading] = useState<boolean>(false);

  const [csvOpen, setCsvOpen] = useState<boolean>(false);
  const [csvLoading, setCsvLoading] = useState<boolean>(false);
  const [csvError, setCsvError] = useState<string | null>(null);
  const [csvResult, setCsvResult] = useState<IImportMusclesResult | null>(null);

  const selectedRows = useMemo(
    () => rows.filter((row) => selectedRowIds.includes(row.id) && !row.isDeleted),
    [rows, selectedRowIds],
  );

  const deleteDialogOpen = deleteTargets.length > 0;

  const loadRows = useCallback(async () => {
    try {
      const result = await dispatch(getMusclesPaginated({
        includeDeleted,
        search: appliedSearch || undefined,
        code: appliedCode || undefined,
        name: appliedName || undefined,
        sortBy,
        sortDirection,
        page: page + 1,
        pageSize: rowsPerPage,
      })).unwrap();
      setTotalCount(result.totalCount);
    } catch {
      // Error already set in Redux state by the thunk
    }
  }, [dispatch, includeDeleted, appliedSearch, appliedCode, appliedName, sortBy, sortDirection, page, rowsPerPage]);

  useEffect(() => {
    void loadRows();
  }, [loadRows]);

  useEffect(() => {
    const activeIds = new Set(rows.filter((row) => !row.isDeleted).map((row) => row.id));
    setSelectedRowIds((current) => current.filter((id) => activeIds.has(id)));
  }, [rows]);

  const handleSortChange = (field: 'code' | 'name' | 'description') => {
    setPage(0);

    if (sortBy === field) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'));
      return;
    }

    setSortBy(field);
    setSortDirection('asc');
  };

  const openCreate = () => {
    setFormState(defaultMuscleFormState);
    setFormError(null);
    setFormOpen(true);
  };

  const openEdit = (row: IMuscle) => {
    setFormState({
      id: row.id,
      name: row.name,
      code: row.code,
      description: row.description ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  };

  const closeForm = () => {
    if (formLoading) {
      return;
    }

    setFormOpen(false);
    setFormState(defaultMuscleFormState);
    setFormError(null);
  };

  const submitForm = async () => {
    setFormLoading(true);
    setFormError(null);

    const request: IUpsertMuscleRequest = {
      name: formState.name.trim(),
      code: formState.code.trim(),
      description: formState.description.trim() || null,
      muscleGroupIds: ['general'],
      active: true,
    };

    try {
      if (formState.id) {
        await dispatch(editMuscle({ id: formState.id, data: request })).unwrap();
      } else {
        await dispatch(createMuscle(request)).unwrap();
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

  const openDeleteDialog = (targets: IMuscle[]) => {
    const rowsToDelete = targets.filter((row) => !row.isDeleted);

    if (rowsToDelete.length === 0) {
      return;
    }

    setDeleteTargets(rowsToDelete);
  };

  const closeDeleteDialog = () => {
    if (deleteLoading) {
      return;
    }

    setDeleteTargets([]);
  };

  const handleDelete = async () => {
    if (deleteTargets.length === 0) {
      return;
    }

    setDeleteLoading(true);

    try {
      const results = await Promise.allSettled(deleteTargets.map((row) => dispatch(deleteMuscle(row.id)).unwrap()));
      const failedCount = results.filter((result) => result.status === 'rejected').length;

      if (failedCount > 0) {
        dispatch(setAdminMusclesError(t('administration.muscles.bulkDeleteError', { failed: failedCount, total: deleteTargets.length })));
      }

      setDeleteTargets([]);
      setSelectedRowIds([]);
      await loadRows();
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleReactivate = async (row: IMuscle) => {
    try {
      await dispatch(reactivateAdminMuscle(row.id)).unwrap();
      await loadRows();
    } catch (reactivateError) {
      const message = reactivateError instanceof Error
        ? reactivateError.message
        : t('administration.common.reactivateError');
      dispatch(setAdminMusclesError(message));
    }
  };

  const handleCsvImport = async (importRows: Array<{ name?: string; code?: string; description?: string }>) => {
    setCsvLoading(true);
    setCsvError(null);

    try {
      const result = await dispatch(importAdminMusclesCsv({ rows: importRows })).unwrap();
      setCsvResult(result);
      await loadRows();
    } catch (importError) {
      const message = importError instanceof Error ? importError.message : t('administration.muscles.csv.importError');
      setCsvError(message);
    } finally {
      setCsvLoading(false);
    }
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.muscles.title')}</Typography>
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
        <Button variant="contained" onClick={openCreate}>
          {t('common.actions.create')}
        </Button>
      </Stack>

      <MusclesFiltersDrawer
        open={filtersOpen}
        searchInput={searchInput}
        filterCode={filterCode}
        filterName={filterName}
        includeDeleted={includeDeleted}
        onClose={() => setFiltersOpen(false)}
        onSearchInputChange={setSearchInput}
        onFilterCodeChange={setFilterCode}
        onFilterNameChange={setFilterName}
        onIncludeDeletedChange={setIncludeDeleted}
        onApplySearch={() => {
          setAppliedSearch(searchInput);
          setAppliedCode(filterCode);
          setAppliedName(filterName);
          setPage(0);
        }}
        onClearFilters={() => {
          setSearchInput('');
          setFilterCode('');
          setFilterName('');
          setAppliedSearch('');
          setAppliedCode('');
          setAppliedName('');
          setIncludeDeleted(false);
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
          <MusclesTable
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
            onDelete={(row) => {
              openDeleteDialog([row]);
            }}
            onReactivate={(row) => {
              void handleReactivate(row);
            }}
          />
        </Box>
      ) : null}

      <MuscleFormDialog
        open={formOpen}
        loading={formLoading}
        error={formError}
        formState={formState}
        onClose={closeForm}
        onSubmit={() => {
          void submitForm();
        }}
        onChange={(patch) => {
          setFormState((current) => ({ ...current, ...patch }));
        }}
      />

      <MusclesCsvImportDialog
        open={csvOpen}
        loading={csvLoading}
        error={csvError}
        result={csvResult}
        onClose={() => {
          if (csvLoading) {
            return;
          }

          setCsvOpen(false);
          setCsvError(null);
          setCsvResult(null);
        }}
        onImport={handleCsvImport}
      />

      <PopupDialog
        open={deleteDialogOpen}
        title={t('common.messages.confirmDelete')}
        onClose={closeDeleteDialog}
        onSubmit={() => {
          void handleDelete();
        }}
        closeLabel={t('common.actions.cancel')}
        saveLabel={t('common.actions.delete')}
        disableSave={deleteLoading}
        isSaving={deleteLoading}
      >
        <Typography>
          {deleteTargets.length === 1
            ? t('common.messages.deleteWarning', { code: deleteTargets[0].code })
            : t('administration.muscles.deleteManyWarning', { count: deleteTargets.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
