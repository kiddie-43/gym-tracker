import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import { Badge, Box, Button, CircularProgress, Stack, Typography } from '@mui/material';
import { useEffect } from 'react';
import { useTranslation } from "react-i18next";
import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../enums/popUp/popUp';
import { useAppDispatch, useAppSelector } from "../../../redux/hooks";
import {
  deleteUnits,
  fetchUnits,
  setUnitsForm,
  setUnitsPopUpCode,
  setUnitsTable,
} from '../../../redux/actions/units/unitsActions';
import { selectUnitState, unitsInitialState } from "../../../redux/states/units/unitsState";
import { IUnit } from '../../../interfaces/units/IUnit';
import { UnitsFiltersDrawer } from "./filters/UnitsFiltersDrawer";
import { MeasurementTypeFormDialog } from "./form/UnitsFormDialog";
import { UnitsCsvImportDialog } from "./UnitsCsvImportDialog";
import { UnitsTable } from "./table/UnitsTable";

export function UnitsPanel({ supportsImport }: { supportsImport: boolean }) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const {
    table,
    filters,
    loading,
    error,
    popUpCode,
  } = useAppSelector(selectUnitState);

  const { items: rows } = table;
  const selectedIds = table.selectedIds ?? [];

  const hasActiveFilters = filters.code !== '' || filters.name !== '' || filters.description !== '';

  const deleteDialogOpen = popUpCode === PopUpCode.Delete;

  const selectedRows = rows.filter(
    (row): row is IUnit & { id: string } =>
      row.id !== undefined &&
      (selectedIds?.includes(row.id) ?? false)
  );

  useEffect(() => {
    dispatch(setUnitsTable(unitsInitialState.table));
    void dispatch(fetchUnits());
  }, [dispatch]);

  useEffect(() => {
    const activeIds = new Set(rows.map((row) => row.id));
    const cleaned = selectedIds.filter((id) => activeIds.has(id));
    if (cleaned.length !== selectedIds.length) {
      dispatch(setUnitsTable({ ...table, selectedIds: cleaned }));
    }
  }, [rows]); // eslint-disable-line react-hooks/exhaustive-deps

  const openCreate = () => {
    dispatch(setUnitsForm({ ...unitsInitialState.form }));
    dispatch(setUnitsPopUpCode(PopUpCode.Create));
  };

  const openDeleteDialog = (targets: IUnit[]) => {
    if (targets.length === 0) return;
    dispatch(setUnitsTable({
      ...table,
      selectedIds: targets.map((r) => r.id).filter((id): id is string => id !== undefined),
    }));
    dispatch(setUnitsPopUpCode(PopUpCode.Delete));
  };

  const closeDeleteDialog = () => {
    if (loading) return;
    dispatch(setUnitsPopUpCode(PopUpCode.Default));
  };

  const handleDelete = async () => {
    if (selectedIds.length === 0) return;
    const result = await dispatch(deleteUnits(selectedIds));
    if (deleteUnits.fulfilled.match(result)) {
      dispatch(setUnitsTable({ ...table, selectedIds: [] }));
      dispatch(setUnitsPopUpCode(PopUpCode.Default));
      void dispatch(fetchUnits());
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
          onClick={() => dispatch(setUnitsPopUpCode(PopUpCode.Filter))}
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
        <Button variant="outlined" onClick={() => void dispatch(fetchUnits())}>
          {t('common.actions.reload')}
        </Button>
        {supportsImport ? (
          <Button variant="outlined" onClick={() => dispatch(setUnitsPopUpCode(PopUpCode.CsvImport))}>
            {t('common.actions.import')}
          </Button>
        ) : null}
        <Button variant="contained" onClick={openCreate}>
          {t('common.actions.create')}
        </Button>
      </Stack>

      <UnitsFiltersDrawer />

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
          <UnitsTable />
        </Box>
      ) : null}

      <MeasurementTypeFormDialog />

      <UnitsCsvImportDialog />

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