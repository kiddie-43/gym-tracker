import { useCallback, useEffect, useMemo, useState } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { ImportMeasurementTypeCsvRowRequest, ImportMeasurementTypesResult, MeasurementTypeDto, UpsertMeasurementTypeRequest } from '../../../interfaces/admin/measurementTypes/measurementTypes';
import {
  createMeasurementType,
  deleteMeasurementType,
  importMeasurementTypesCsv,
  listMeasurementTypesPage,
  reactivateMeasurementType,
  updateMeasurementType,
} from '../../../services/api/admin/measurementTypes/measurementTypesApi';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { MeasurementTypesCsvImportDialog } from './MeasurementTypesCsvImportDialog';
import { MeasurementTypesFiltersDrawer } from './filters/MeasurementTypesFiltersDrawer';
import { defaultMeasurementTypeFormState, type MeasurementTypeFormState } from './form/measurementTypeForm';
import { MeasurementTypeFormDialog } from './form/MeasurementTypeFormDialog';
import { MeasurementTypesTable } from './table/MeasurementTypesTable';

export function MeasurementTypesPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const [rows, setRows] = useState<MeasurementTypeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [includeInactive, setIncludeInactive] = useState(false);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [codeInput, setCodeInput] = useState('');
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [sortBy, setSortBy] = useState<'name' | 'description'>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState<number>(0);
  const [rowsPerPage, setRowsPerPage] = useState<number>(10);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [formOpen, setFormOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<MeasurementTypeFormState>(defaultMeasurementTypeFormState);

  const [csvOpen, setCsvOpen] = useState(false);
  const [csvLoading, setCsvLoading] = useState(false);
  const [csvError, setCsvError] = useState<string | null>(null);
  const [csvResult, setCsvResult] = useState<ImportMeasurementTypesResult | null>(null);

  const [selectedRowIds, setSelectedRowIds] = useState<string[]>([]);
  const [deleteTargets, setDeleteTargets] = useState<MeasurementTypeDto[]>([]);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const selectedRows = useMemo(
    () => rows.filter((row) => selectedRowIds.includes(row.id) && !row.isDeleted),
    [rows, selectedRowIds],
  );

  const openDeleteDialog = (targets: MeasurementTypeDto[]) => {
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
      const results = await Promise.allSettled(deleteTargets.map((row) => deleteMeasurementType(row.id)));
      const failedCount = results.filter((r) => r.status === 'rejected').length;

      if (failedCount > 0) {
        setError(t('administration.measurementTypes.bulkDeleteError', { failed: failedCount, total: deleteTargets.length }));
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

  const handleSortChange = (field: 'name' | 'description') => {
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
      const data = await listMeasurementTypesPage({
        includeInactive,
        search: search || undefined,
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
  }, [includeInactive, search, sortBy, sortDirection, page, rowsPerPage, t]);

  useEffect(() => {
    void loadRows();
  }, [loadRows]);

  const closeForm = () => {
    if (formLoading) {
      return;
    }

    setFormOpen(false);
    setFormError(null);
    setFormState(defaultMeasurementTypeFormState);
  };

  const openCreate = () => {
    setFormState(defaultMeasurementTypeFormState);
    setFormError(null);
    setFormOpen(true);
  };

  const openEdit = (row: MeasurementTypeDto) => {
    setFormState({
      id: row.id,
      code: row.key,
      name: row.name,
      category: row.category,
      description: row.description ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  };

  const submitForm = async () => {
    setFormLoading(true);
    setFormError(null);

    const request: UpsertMeasurementTypeRequest = {
      name: formState.name.trim(),
      key: formState.code.trim() || undefined,
      category: formState.category.trim() || undefined,
      description: formState.description.trim() || undefined,
    };

    try {
      if (formState.id) {
        await updateMeasurementType(formState.id, request);
      } else {
        await createMeasurementType(request);
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

  const onDelete = async (row: MeasurementTypeDto) => {
    setError(null);

    try {
      await deleteMeasurementType(row.id);
      await loadRows();
    } catch (deleteError) {
      const message = deleteError instanceof Error ? deleteError.message : t('administration.common.deleteError');
      setError(message);
    }
  };

  const onReactivate = async (row: MeasurementTypeDto) => {
    setError(null);

    try {
      await reactivateMeasurementType(row.id);
      await loadRows();
    } catch (reactivateError) {
      const message = reactivateError instanceof Error
        ? reactivateError.message
        : t('administration.common.reactivateError');
      setError(message);
    }
  };

  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.measurementTypes.title')}</Typography>
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

      <MeasurementTypesFiltersDrawer
        open={filtersOpen}
        searchInput={searchInput}
        codeInput={codeInput}
        includeInactive={includeInactive}
        onClose={() => setFiltersOpen(false)}
        onSearchInputChange={setSearchInput}
        onCodeInputChange={setCodeInput}
        onIncludeInactiveChange={setIncludeInactive}
        onApplySearch={() => {
          setSearch(searchInput);
          setPage(0);
        }}
        onClearFilters={() => {
          setSearchInput('');
          setCodeInput('');
          setSearch('');
          setIncludeInactive(false);
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
          <MeasurementTypesTable
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
              void onReactivate(row);
            }}
          />
        </Box>
      ) : null}

      <MeasurementTypeFormDialog
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

      <MeasurementTypesCsvImportDialog
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
        onImport={async (rows: ImportMeasurementTypeCsvRowRequest[]) => {
          setCsvLoading(true);
          setCsvError(null);
          setCsvResult(null);

          try {
            const result = await importMeasurementTypesCsv({ rows });
            setCsvResult(result);
            await loadRows();
          } catch (importError) {
            const message = importError instanceof Error ? importError.message : t('administration.measurementTypes.csv.importError');
            setCsvError(message);
          } finally {
            setCsvLoading(false);
          }
        }}
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
          {t('administration.measurementTypes.deleteManyWarning', { count: deleteTargets.length })}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
