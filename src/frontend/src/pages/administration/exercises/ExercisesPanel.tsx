import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded';
import Badge from '@mui/material/Badge';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';

import { FeedbackMessage } from '../../../components/FeedbackMessage/FeedbackMessage';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';
import { PopUpCode } from '../../../enums/popUp/popUp';
import type { IExercise, IExercisesFilter } from '../../../interfaces/IExercises/IExercises';
import {
    deleteSelectedExercisesAction,
    fetchExercisesPageAction,
    importAdminExercisesCsv,
    setExerciseDialogStateAction,
    setExercisesCsvResult,
    setExercisesFilters,
    setExercisesTable,
} from '../../../redux/actions/exercises/exercisesActions';
import { useAppDispatch, useAppSelector } from '../../../redux/hooks';
import { exercisesInitialState, selectExercisesState } from '../../../redux/states/exercises/exercisesState';
import { ExercisesFiltersDrawer } from './filters/ExercisesFiltersDrawer';
import { ExerciseFormDialog } from './form/ExerciseFormDialog';
import { ExercisesCsvImportDialog } from './importCsv/ExercisesCsvImportDialog';
import type { ExercisesCsvImportDialogProps } from './importCsv/ExercisesCsvImportDialog';
import { ExercisesTable } from './table/ExercisesTable';

export function ExercisesPanel({ supportsImport }: { supportsImport: boolean }) {
    const { t } = useTranslation();
    const dispatch = useAppDispatch();

    const {
        table,
        filters,
        csvResult,
        loading,
        error,
        popUpCode,
    } = useAppSelector(selectExercisesState);



    const hasActiveFilters =
        
        (filters.unitId ?? []).length > 0 ||
        (filters.primaryMuscleId ?? []).length > 0 ||
        (filters.secondaryMuscleId ?? []).length > 0;

    const rows = table.items;
    const selectedIds = table.selectedIds ?? [];
    const selectedRows = rows.filter(
        (row): row is IExercise & { id: string } => row.id !== undefined && selectedIds.includes(row.id),
    );

    const buildQuery = (overrides: Partial<IExercisesFilter> = {}): IExercisesFilter => ({


        unitId: filters.unitId,
        primaryMuscleId: filters.primaryMuscleId,
        secondaryMuscleId: filters.secondaryMuscleId,
        sortBy: table.sortBy === '' ? undefined : (table.sortBy as IExercisesFilter['sortBy']),
        sortDirection: table.sortDirection,
        page: table.page,
        pageSize: table.pageSize,
        ...overrides,
    });




    useEffect(() => {
        dispatch(setExercisesTable(exercisesInitialState.table));
        void dispatch(fetchExercisesPageAction());
    }, [dispatch]);

    const handleReload = () => {
        void dispatch(fetchExercisesPageAction());
    };

    const handleCreate = () => {
        dispatch(setExerciseDialogStateAction(PopUpCode.Create));
    };

    const handleOpenDeleteDialog = (targets: (IExercise & { id: string })[]) => {
        dispatch(setExercisesTable({ ...table, selectedIds: targets.map((row) => row.id) }));
        dispatch(setExerciseDialogStateAction(PopUpCode.Delete));
    };

    const handleConfirmDelete = async () => {
        await dispatch(deleteSelectedExercisesAction(table.selectedIds ?? []) as never);
    };

    const handleCsvImport: ExercisesCsvImportDialogProps['onImport'] = async (importRows) => {
        await dispatch(importAdminExercisesCsv(importRows) as never);
        void dispatch(fetchExercisesPageAction(buildQuery()) as never);
    };

    return (
        <Stack spacing={2} sx={{ height: '100%', minHeight: 0, overflow: 'hidden' }}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h4">{t('administration.exercises.title')}</Typography>
                <Button
                    variant="outlined"
                    startIcon={(
                        <Badge variant="dot" color="primary" invisible={!hasActiveFilters}>
                            <FilterListRoundedIcon />
                        </Badge>
                    )}
                    onClick={() => dispatch(setExerciseDialogStateAction(PopUpCode.Filter))}
                >
                    {t('common.filters')}
                </Button>
            </Stack>

            <Stack direction="row" justifyContent="flex-end" spacing={1}>
                {selectedRows.length > 0 ? (
                    <Button variant="outlined" color="error" onClick={() => handleOpenDeleteDialog(selectedRows)}>
                        {t('common.actions.deleteSelected')}
                    </Button>
                ) : null}
                <Button variant="outlined" onClick={handleReload}>
                    {t('common.actions.reload')}
                </Button>
                {supportsImport ? (
                    <Button variant="outlined" onClick={() => dispatch(setExerciseDialogStateAction(PopUpCode.CsvImport))}>
                        {t('common.actions.import')}
                    </Button>
                ) : null}
                <Button variant="contained" onClick={handleCreate}>{t('common.actions.create')}</Button>
            </Stack>

            <ExercisesFiltersDrawer
                open={PopUpCode.Filter === popUpCode}
                onClose={() => dispatch(setExerciseDialogStateAction(PopUpCode.Default))}
                onApplySearch={() => {
                    dispatch(setExercisesTable({ ...table, page: 0 }));
                    void dispatch(fetchExercisesPageAction({ ...buildQuery(), page: 0 }) as never);
                    dispatch(setExerciseDialogStateAction(PopUpCode.Default));
                }}
                onClearFilters={() => {
                    dispatch(setExercisesFilters(exercisesInitialState.filters));
                    dispatch(setExercisesTable({ ...table, page: 0 }));
                    void dispatch(fetchExercisesPageAction(buildQuery({ ...exercisesInitialState.filters, page: 0 })) as never);
                }}
            />

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
                    <ExercisesTable/>
                </Box>
            ) : null}

            <ExerciseFormDialog
            />

            <ExercisesCsvImportDialog
                open={PopUpCode.CsvImport === popUpCode}
                loading={loading}
                error={error}
                result={(csvResult as ExercisesCsvImportDialogProps['result']) ?? null}
                onClose={() => {
                    dispatch(setExerciseDialogStateAction(PopUpCode.Default));
                    dispatch(setExercisesCsvResult(null));
                }}
                onImport={handleCsvImport}
            />

            <PopupDialog
                open={PopUpCode.Delete === popUpCode}
                title={t('common.messages.confirmDelete')}
                onClose={() => dispatch(setExerciseDialogStateAction(PopUpCode.Default))}
                onSubmit={() => { void handleConfirmDelete(); }}
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
