import { useEffect } from 'react';

import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Badge from '@mui/material/Badge';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IMuscle } from '../../../interfaces/IMuscles/IMuscles';
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../enums/popUp/popUp';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import {
  deleteAdminMuscles,
  fetchAdminMuscles,
} from '../../../redux/actions/muscles/musclesActions';
import {
  setMusclesForm,
  setMusclesPopUpCode,
  setMusclesTable,
} from '../../../redux/actions/muscles/musclesActions';
import { selectMusclesState, musclesInitialState } from '../../../redux/states/adminMuscles/adminMusclesState';
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
    loading,
    error,
    popUpCode,
  } = useAppSelector(selectMusclesState);

  const { items: rows } = table;
  const selectedIds = table.selectedIds ?? [];

  const hasActiveFilters = (filters.code ?? '') !== '' || (filters.name ?? '') !== '';

  const deleteDialogOpen = popUpCode === PopUpCode.Delete;

  const deleteTargets = rows.filter((row): row is IMuscle & { id: string } =>
    row.id !== undefined && selectedIds.includes(row.id)
  );
  const selectedRows = rows.filter((row): row is IMuscle & { id: string } =>
    row.id !== undefined && selectedIds.includes(row.id)
  );

  useEffect(() => {
    dispatch(setMusclesTable(musclesInitialState.table));
    void dispatch(fetchAdminMuscles());
  }, [dispatch]);

  useEffect(() => {
    const activeIds = new Set(rows.map((row) => row.id));
    const cleaned = selectedIds.filter((id) => activeIds.has(id));
    if (cleaned.length !== selectedIds.length) {
      dispatch(setMusclesTable({ ...table, selectedIds: cleaned }));
    }
  }, [rows]); // eslint-disable-line react-hooks/exhaustive-deps

  const openCreate = () => {
    dispatch(setMusclesForm({ ...musclesInitialState.form }));
    dispatch(setMusclesPopUpCode(PopUpCode.Create));
  };

  const openDeleteDialog = (targets: IMuscle[]) => {
    if (targets.length === 0) return;
    dispatch(setMusclesTable({ ...table, selectedIds: targets.map((r) => r.id).filter((id): id is string => id !== undefined) }));
    dispatch(setMusclesPopUpCode(PopUpCode.Delete));
  };

  const closeDeleteDialog = () => {
    if (loading) return;
    dispatch(setMusclesPopUpCode(PopUpCode.Default));
  };

  const handleDelete = async () => {
    if (selectedIds.length === 0) return;
    const result = await dispatch(deleteAdminMuscles(selectedIds));
    if (deleteAdminMuscles.fulfilled.match(result)) {
      dispatch(setMusclesTable({ ...table, selectedIds: [] }));
      dispatch(setMusclesPopUpCode(PopUpCode.Default));
      void dispatch(fetchAdminMuscles());
    }
  };


  return (
    <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h4">{t('administration.muscles.title')}</Typography>
        <Button
          variant="outlined"
          startIcon={
            <Badge variant="dot" color="primary" invisible={!hasActiveFilters}>
              <FilterListRoundedIcon />
            </Badge>
          }
          onClick={() => dispatch(setMusclesPopUpCode(PopUpCode.Filter))}
        >
          {t('common.filters')}
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
          <Button variant="outlined" onClick={() => dispatch(setMusclesPopUpCode(PopUpCode.CsvImport))}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>
          {t('common.actions.create')}
        </Button>
      </Stack>

      <MusclesFiltersDrawer />

      {error ? (
        <FeedbackMessage type="error" message={error} />
      ) : null}

      {loading ? (
        <Stack alignItems="center" justifyContent="center" sx={{ flex: 1 }}>
          <CircularProgress aria-label={t('common.loading')} />
        </Stack>
      ) : null}

      {!loading && !error && rows.length === 0 ? (
        <FeedbackMessage type="empty" message={t('common.messages.noData')} />
      ) : null}

      {!loading && rows.length > 0 ? (
        <Box sx={{ flex: 1, minHeight: 0, display: 'flex', flexDirection: 'column' }}>
          <MusclesTable />
        </Box>
      ) : null}

      <MuscleFormDialog />

      <MusclesCsvImportDialog />

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
