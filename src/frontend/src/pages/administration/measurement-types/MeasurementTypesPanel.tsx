import { useEffect } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Badge from '@mui/material/Badge';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IImportMeasurementTypeCsvRowRequest, IMeasurementType } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import {
  deleteAdminMeasurementTypes,
  fetchAdminMeasurementTypes,
  importAdminMeasurementTypesCsv,
  reactivateAdminMeasurementType,
  submitAdminMeasurementTypeForm,
} from '../../../redux/actions/adminMeasurementTypes/adminMeasurementTypesThunks';
import {
  setAdminMeasurementTypesFilters,
  setAdminMeasurementTypesForm,
  setAdminMeasurementTypesPopUpCode,
  setAdminMeasurementTypesTable,
} from '../../../redux/actions/adminMeasurementTypes/adminMeasurementTypesActions';
import { adminMeasurementTypesInitialState, selectAdminMeasurementTypesState } from '../../../redux/states/adminMeasurementTypes/adminMeasurementTypesState';
import { MeasurementTypesFiltersDrawer } from './filters/MeasurementTypesFiltersDrawer';
import { MeasurementTypeFormDialog } from './form/MeasurementTypeFormDialog';
import { MeasurementTypesCsvImportDialog } from './MeasurementTypesCsvImportDialog';
import { MeasurementTypesTable } from './table/MeasurementTypesTable';

export function MeasurementTypesPanel({ supportsImport }: { supportsImport: boolean }) {
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
  } = useAppSelector(selectAdminMeasurementTypesState);

  const { list: rows, selectedIds } = table;

  const hasActiveFilters = filters.search !== '' || filters.code !== '' || filters.includeDeleted;

  const formOpen = popUpCode === 'CREATE' || popUpCode === 'EDIT';
  const filtersOpen = popUpCode === 'FILTERS';
  const deleteDialogOpen = popUpCode === 'DELETE';
  const csvOpen = popUpCode === 'CSV_IMPORT';

  const selectedRows = rows.filter((row): row is IMeasurementType & { id: string } =>
    row.id !== undefined && selectedIds.includes(row.id) && !row.isDeleted
  );

  useEffect(() => {
    dispatch(setAdminMeasurementTypesTable(adminMeasurementTypesInitialState.table));
    void dispatch(fetchAdminMeasurementTypes());
  }, [dispatch]);

  useEffect(() => {
    const activeIds = new Set(rows.filter((row) => !row.isDeleted).map((row) => row.id));
    const cleaned = selectedIds.filter((id) => activeIds.has(id));
    if (cleaned.length !== selectedIds.length) {
      dispatch(setAdminMeasurementTypesTable({ ...table, selectedIds: cleaned }));
    }
  }, [rows]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSortChange = (field: 'code' | 'name' | 'description') => {
    const newDirection = table.sortBy === field && table.sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setAdminMeasurementTypesTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchAdminMeasurementTypes());
  };

  const openCreate = () => {
    dispatch(setAdminMeasurementTypesForm({ ...adminMeasurementTypesInitialState.form }));
    dispatch(setAdminMeasurementTypesPopUpCode('CREATE'));
  };

  const openEdit = (row: IMeasurementType) => {
    dispatch(setAdminMeasurementTypesForm({ ...row }));
    dispatch(setAdminMeasurementTypesPopUpCode('EDIT'));
  };

  const closeForm = () => {
    if (loading) return;
    dispatch(setAdminMeasurementTypesPopUpCode(null));
  };

  const submitForm = async () => {
    const result = await dispatch(submitAdminMeasurementTypeForm());
    if (submitAdminMeasurementTypeForm.fulfilled.match(result)) {
      dispatch(setAdminMeasurementTypesPopUpCode(null));
      void dispatch(fetchAdminMeasurementTypes());
    }
  };

  const openDeleteDialog = (targets: IMeasurementType[]) => {
    const toDelete = targets.filter((row) => !row.isDeleted);
    if (toDelete.length === 0) return;
    dispatch(setAdminMeasurementTypesTable({
      ...table,
      selectedIds: toDelete.map((r) => r.id).filter((id): id is string => id !== undefined),
    }));
    dispatch(setAdminMeasurementTypesPopUpCode('DELETE'));
  };

  const closeDeleteDialog = () => {
    if (loading) return;
    dispatch(setAdminMeasurementTypesPopUpCode(null));
  };

  const handleDelete = async () => {
    if (selectedIds.length === 0) return;
    const result = await dispatch(deleteAdminMeasurementTypes(selectedIds));
    if (deleteAdminMeasurementTypes.fulfilled.match(result)) {
      dispatch(setAdminMeasurementTypesTable({ ...table, selectedIds: [] }));
      dispatch(setAdminMeasurementTypesPopUpCode(null));
      void dispatch(fetchAdminMeasurementTypes());
    }
  };

  const handleReactivate = async (row: IMeasurementType) => {
    const result = await dispatch(reactivateAdminMeasurementType(row.id ?? ''));
    if (reactivateAdminMeasurementType.fulfilled.match(result)) {
      void dispatch(fetchAdminMeasurementTypes());
    }
  };

  const handleCsvImport = async (importRows: IImportMeasurementTypeCsvRowRequest[]) => {
    const result = await dispatch(importAdminMeasurementTypesCsv({ rows: importRows }));
    if (importAdminMeasurementTypesCsv.fulfilled.match(result)) {
      void dispatch(fetchAdminMeasurementTypes());
    }
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.measurementTypes.title')}</Typography>
        <Button
          variant="outlined"
          startIcon={
            <Badge variant="dot" color="primary" invisible={!hasActiveFilters}>
              <FilterListRoundedIcon />
            </Badge>
          }
          onClick={() => dispatch(setAdminMeasurementTypesPopUpCode('FILTERS'))}
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
        <Button variant="outlined" onClick={() => void dispatch(fetchAdminMeasurementTypes())}>
          {t('common.actions.reload')}
        </Button>
        {supportsImport ? (
          <Button variant="outlined" onClick={() => dispatch(setAdminMeasurementTypesPopUpCode('CSV_IMPORT'))}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>
          {t('common.actions.create')}
        </Button>
      </Stack>

      <MeasurementTypesFiltersDrawer
        open={filtersOpen}
        searchInput={filters.search}
        codeInput={filters.code}
        includeDeleted={filters.includeDeleted}
        onClose={() => dispatch(setAdminMeasurementTypesPopUpCode(null))}
        onSearchInputChange={(val) => dispatch(setAdminMeasurementTypesFilters({ ...filters, search: val }))}
        onCodeInputChange={(val) => dispatch(setAdminMeasurementTypesFilters({ ...filters, code: val }))}
        onIncludeDeletedChange={(val) => dispatch(setAdminMeasurementTypesFilters({ ...filters, includeDeleted: val }))}
        onApplySearch={() => {
          dispatch(setAdminMeasurementTypesTable({ ...table, page: 0 }));
          dispatch(setAdminMeasurementTypesPopUpCode(null));
          void dispatch(fetchAdminMeasurementTypes());
        }}
        onClearFilters={() => {
          dispatch(setAdminMeasurementTypesFilters({ search: '', code: '', includeDeleted: false }));
          dispatch(setAdminMeasurementTypesTable({ ...table, page: 0 }));
          void dispatch(fetchAdminMeasurementTypes());
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
          <MeasurementTypesTable
            rows={rows}
            selectedIds={selectedIds}
            sortBy={table.sortBy}
            sortDirection={table.sortDirection}
            onSortChange={handleSortChange}
            page={table.page}
            rowsPerPage={table.rowsPerPage}
            totalCount={table.totalCount}
            onPageChange={(_event, newPage) => {
              dispatch(setAdminMeasurementTypesTable({ ...table, page: newPage }));
              void dispatch(fetchAdminMeasurementTypes());
            }}
            onRowsPerPageChange={(event) => {
              dispatch(setAdminMeasurementTypesTable({ ...table, rowsPerPage: Number.parseInt(event.target.value, 10), page: 0 }));
              void dispatch(fetchAdminMeasurementTypes());
            }}
            onSelectionChange={(ids) => dispatch(setAdminMeasurementTypesTable({ ...table, selectedIds: ids }))}
            onEdit={openEdit}
            onDelete={(row) => openDeleteDialog([row])}
            onReactivate={(row) => { void handleReactivate(row); }}
          />
        </Box>
      ) : null}

      <MeasurementTypeFormDialog
        open={formOpen}
        loading={loading}
        error={error}
        formState={form}
        onClose={closeForm}
        onSubmit={() => { void submitForm(); }}
        onChange={(patch) => dispatch(setAdminMeasurementTypesForm({ ...form, ...patch }))}
      />

      <MeasurementTypesCsvImportDialog
        open={csvOpen}
        loading={loading}
        error={error}
        result={csvResult}
        onClose={() => {
          if (loading) return;
          dispatch(setAdminMeasurementTypesPopUpCode(null));
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
        isSaving={loading}
      >
        <Typography>
          {t('administration.measurementTypes.deleteManyWarning', { count: selectedIds.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}