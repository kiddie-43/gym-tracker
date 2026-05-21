import { useEffect } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IMuscle } from '../../../interfaces/muscles/IMuscles';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import {
  deleteAdminMuscles,
  fetchAdminMuscles,
  importAdminMusclesCsv,
  reactivateAdminMuscle,
  submitAdminMuscleForm,
} from '../../../redux/actions/muscles/muscles';
import {
  setAdminMusclesFilters,
  setAdminMusclesForm,
  setAdminMusclesPopUpCode,
  setAdminMusclesTable,
} from '../../../redux/actions/adminMuscles/adminMusclesActions';
import { selectAdminMusclesState } from '../../../redux/states/adminMuscles/adminMusclesState';
import { MusclesFiltersDrawer } from './filters/MusclesFiltersDrawer';
import { MuscleFormDialog } from './form/MuscleFormDialog';
import { MusclesCsvImportDialog } from './MusclesCsvImportDialog';
import { MusclesTable } from './table/MusclesTable';

export function MusclesPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const {
    table,
    filters,
    form,
    loading,
    error,
    csvResult,
    popUpCode,
  } = useAppSelector(selectAdminMusclesState);

  const { list: rows, selectedIds } = table;

  const formOpen = popUpCode === 'CREATE' || popUpCode === 'EDIT';
  const filtersOpen = popUpCode === 'FILTERS';
  const deleteDialogOpen = popUpCode === 'DELETE';
  const csvOpen = popUpCode === 'CSV_IMPORT';

  const deleteTargets = rows.filter((row): row is IMuscle & { id: string } =>
    row.id !== undefined && selectedIds.includes(row.id) && !row.isDeleted
  );
  const selectedRows = rows.filter((row): row is IMuscle & { id: string } =>
    row.id !== undefined && selectedIds.includes(row.id) && !row.isDeleted
  );

  useEffect(() => {
    void dispatch(fetchAdminMuscles());
  }, [dispatch]);

  useEffect(() => {
    const activeIds = new Set(rows.filter((row) => !row.isDeleted).map((row) => row.id));
    const cleaned = selectedIds.filter((id) => activeIds.has(id));
    if (cleaned.length !== selectedIds.length) {
      dispatch(setAdminMusclesTable({ ...table, selectedIds: cleaned }));
    }
  }, [rows]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSortChange = (field: 'code' | 'name' | 'description') => {
    const newDirection = table.sortBy === field && table.sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setAdminMusclesTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchAdminMuscles());
  };

  const openCreate = () => {
    dispatch(setAdminMusclesForm({ id: undefined, name: '', code: '', description: null, active: true, muscleGroupIds: [], isDeleted: false, deletedAt: null }));
    dispatch(setAdminMusclesPopUpCode('CREATE'));
  };

  const openEdit = (row: IMuscle) => {
    dispatch(setAdminMusclesForm({ ...row }));
    dispatch(setAdminMusclesPopUpCode('EDIT'));
  };

  const closeForm = () => {
    if (loading) return;
    dispatch(setAdminMusclesPopUpCode(null));
  };

  const submitForm = async () => {
    const result = await dispatch(submitAdminMuscleForm());
    if (submitAdminMuscleForm.fulfilled.match(result)) {
      dispatch(setAdminMusclesPopUpCode(null));
      void dispatch(fetchAdminMuscles());
    }
  };

  const openDeleteDialog = (targets: IMuscle[]) => {
    const toDelete = targets.filter((row) => !row.isDeleted);
    if (toDelete.length === 0) return;
    dispatch(setAdminMusclesTable({ ...table, selectedIds: toDelete.map((r) => r.id).filter((id): id is string => id !== undefined) }));
    dispatch(setAdminMusclesPopUpCode('DELETE'));
  };

  const closeDeleteDialog = () => {
    if (loading) return;
    dispatch(setAdminMusclesPopUpCode(null));
  };

  const handleDelete = async () => {
    if (selectedIds.length === 0) return;
    const result = await dispatch(deleteAdminMuscles(selectedIds));
    if (deleteAdminMuscles.fulfilled.match(result)) {
      dispatch(setAdminMusclesTable({ ...table, selectedIds: [] }));
      dispatch(setAdminMusclesPopUpCode(null));
      void dispatch(fetchAdminMuscles());
    }
  };

  const handleReactivate = async (row: IMuscle) => {
    const result = await dispatch(reactivateAdminMuscle(row.id ?? ''));
    if (reactivateAdminMuscle.fulfilled.match(result)) {
      void dispatch(fetchAdminMuscles());
    }
  };

  const handleCsvImport = async (importRows: Array<{ name?: string; code?: string; description?: string }>) => {
    const result = await dispatch(importAdminMusclesCsv({ rows: importRows }));
    if (importAdminMusclesCsv.fulfilled.match(result)) {
      void dispatch(fetchAdminMuscles());
    }
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.muscles.title')}</Typography>
        <Button
          variant="outlined"
          startIcon={<FilterListRoundedIcon />}
          onClick={() => dispatch(setAdminMusclesPopUpCode('FILTERS'))}
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
        <Button variant="outlined" onClick={() => void dispatch(fetchAdminMuscles())}>
          {t('common.actions.reload')}
        </Button>
        {supportsImport ? (
          <Button variant="outlined" onClick={() => dispatch(setAdminMusclesPopUpCode('CSV_IMPORT'))}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>
          {t('common.actions.create')}
        </Button>
      </Stack>

      <MusclesFiltersDrawer
        open={filtersOpen}
        searchInput={filters.search}
        filterCode={filters.code}
        filterName={filters.name}
        includeDeleted={filters.includeDeleted}
        onClose={() => dispatch(setAdminMusclesPopUpCode(null))}
        onSearchInputChange={(val) => dispatch(setAdminMusclesFilters({ ...filters, search: val }))}
        onFilterCodeChange={(val) => dispatch(setAdminMusclesFilters({ ...filters, code: val }))}
        onFilterNameChange={(val) => dispatch(setAdminMusclesFilters({ ...filters, name: val }))}
        onIncludeDeletedChange={(val) => dispatch(setAdminMusclesFilters({ ...filters, includeDeleted: val }))}
        onApplySearch={() => {
          dispatch(setAdminMusclesTable({ ...table, page: 0 }));
          dispatch(setAdminMusclesPopUpCode(null));
          void dispatch(fetchAdminMuscles());
        }}
        onClearFilters={() => {
          dispatch(setAdminMusclesFilters({ search: '', code: '', name: '', includeDeleted: false }));
          dispatch(setAdminMusclesTable({ ...table, page: 0 }));
          void dispatch(fetchAdminMuscles());
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
            selectedIds={selectedIds}
            sortBy={table.sortBy}
            sortDirection={table.sortDirection}
            onSortChange={handleSortChange}
            page={table.page}
            rowsPerPage={table.rowsPerPage}
            totalCount={table.totalCount}
            onPageChange={(_event, newPage) => {
              dispatch(setAdminMusclesTable({ ...table, page: newPage }));
              void dispatch(fetchAdminMuscles());
            }}
            onRowsPerPageChange={(event) => {
              dispatch(setAdminMusclesTable({ ...table, rowsPerPage: Number.parseInt(event.target.value, 10), page: 0 }));
              void dispatch(fetchAdminMuscles());
            }}
            onSelectionChange={(ids) => dispatch(setAdminMusclesTable({ ...table, selectedIds: ids }))}
            onEdit={openEdit}
            onDelete={(row) => openDeleteDialog([row])}
            onReactivate={(row) => { void handleReactivate(row); }}
          />
        </Box>
      ) : null}

      <MuscleFormDialog
        open={formOpen}
        loading={loading}
        error={error}
        formState={{ id: form.id ?? null, name: form.name, code: form.code, description: form.description ?? '' }}
        onClose={closeForm}
        onSubmit={() => { void submitForm(); }}
        onChange={(patch) => dispatch(setAdminMusclesForm({ ...form, name: patch.name ?? form.name, code: patch.code ?? form.code, description: patch.description ?? form.description }))}
      />

      <MusclesCsvImportDialog
        open={csvOpen}
        loading={loading}
        error={error}
        result={csvResult}
        onClose={() => {
          if (loading) return;
          dispatch(setAdminMusclesPopUpCode(null));
        }}
        onImport={handleCsvImport}
      />

      <PopupDialog
        open={deleteDialogOpen}
        title={t('common.messages.confirmDelete')}
        onClose={closeDeleteDialog}
        onSubmit={() => { void handleDelete(); }}
        closeLabel={t('common.actions.cancel')}
        saveLabel={t('common.actions.delete')}
        disableSave={loading}
        isSaving={loading}
      >
        <Typography>
          {deleteTargets.length === 1
            ? t('common.messages.deleteWarning', { code: deleteTargets[0]?.code })
            : t('administration.muscles.deleteManyWarning', { count: deleteTargets.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
