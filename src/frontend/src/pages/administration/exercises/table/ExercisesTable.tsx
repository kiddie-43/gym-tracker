import { useState } from 'react';
import type { ChangeEvent } from 'react';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import TableSortLabel from '@mui/material/TableSortLabel';

import TableCell from '@mui/material/TableCell';
import TableRow from '@mui/material/TableRow';
import { useTranslation } from 'react-i18next';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import { DataTable } from '../../../../components/DataTable/DataTable';
import type { IExercise, IExercisesFilter } from '../../../../interfaces/IExercises/IExercises';
import {
  fetchExercisesPageAction,
  setExerciseDialogStateAction,
  setExercisesTable,
} from '../../../../redux/actions/exercises/exercisesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';





export function ExercisesTable() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [menuAnchorEl, setMenuAnchorEl] = useState<HTMLElement | null>(null);
  const [menuRowId, setMenuRowId] = useState<string | null>(null);
  const { table, filters } = useAppSelector((state) => state.exercises);
  const { items, totalCount, page, pageSize, sortBy, sortDirection, selectedIds } = table;

  const openMenu = (event: React.MouseEvent<HTMLElement>, rowId: string) => {
    setMenuAnchorEl(event.currentTarget);
    setMenuRowId(rowId);
  };

  const closeMenu = () => {
    setMenuAnchorEl(null);
    setMenuRowId(null);
  };

  const selectedRow = items.find((row) => row.id === menuRowId) ?? null;
  const rowsPerPage = pageSize;
  const safeSelectedIds = selectedIds ?? [];
  const selectableRowIds = items

    .map((row) => row.id as string);
  const selectedActiveCount = selectableRowIds.filter((id) => safeSelectedIds.includes(id)).length;
  const allActiveSelected = selectableRowIds.length > 0 && selectedActiveCount === selectableRowIds.length;
  const someActiveSelected = selectedActiveCount > 0 && !allActiveSelected;

  const buildQuery = (overrides: Partial<IExercisesFilter> = {}): IExercisesFilter => ({
    unitId: filters.unitId,
    primaryMuscleId: filters.primaryMuscleId,
    secondaryMuscleId: filters.secondaryMuscleId,
    sortBy: sortBy === '' ? undefined : (sortBy as IExercisesFilter['sortBy']),
    sortDirection,
    page,
    pageSize: rowsPerPage,
    ...overrides,
  });




  const onSort = (field: 'code' | 'name' | 'category' | 'difficulty') => {
    const newDirection = sortBy === field && sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setExercisesTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchExercisesPageAction(buildQuery({ sortBy: field, sortDirection: newDirection, page: 0 })) as never);
  };

  const onEdit = (exercise: IExercise) => {
    dispatch(setExerciseDialogStateAction(PopUpCode.Update, { ...exercise }));
  };



  const onDelete = (exercise: IExercise) => {
    dispatch(setExerciseDialogStateAction(PopUpCode.Delete, exercise));
  };

  const onToggleSelect = (rowId?: string, checked?: boolean) => {
    if (!rowId) {
      dispatch(setExercisesTable({ ...table, selectedIds: checked ? selectableRowIds : [] }));
      return;
    }

    const nextSelectedIds = checked
      ? Array.from(new Set([...safeSelectedIds, rowId]))
      : safeSelectedIds.filter((id) => id !== rowId);

    dispatch(setExercisesTable({ ...table, selectedIds: nextSelectedIds }));
  };

  const onPageChange = (_event: unknown, newPage: number) => {
    dispatch(setExercisesTable({ ...table, page: newPage }));
    void dispatch(fetchExercisesPageAction(buildQuery({ page: newPage })) as never);
  };

  const onRowsPerPageChange = (event: ChangeEvent<HTMLInputElement>) => {
    const newPageSize = Number(event.target.value);
    dispatch(setExercisesTable({ ...table, page: 0, pageSize: newPageSize }));
    void dispatch(fetchExercisesPageAction(buildQuery({ page: 0, pageSize: newPageSize })) as never);
  };

  return (
    <>
      <DataTable
        ariaLabel={t('administration.exercises.table')}
        columnWidths={['4%', '9%',  '9%', '10%', '15%', '15%',  '6%']}
        head={(
          <TableRow>
            <TableCell padding="checkbox">
              <Checkbox
                checked={allActiveSelected}
                indeterminate={someActiveSelected}
                onChange={(_event, checked) => onToggleSelect(undefined, checked)}
                inputProps={{ 'aria-label': t('administration.exercises.selectAllRows') }}
              />
            </TableCell>
            <TableCell sortDirection={sortBy === 'code' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'code'}
                direction={sortBy === 'code' ? sortDirection : 'asc'}
                onClick={() => onSort('code')}
              >
                {t('common.fields.code')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'name' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'name'}
                direction={sortBy === 'name' ? sortDirection : 'asc'}
                onClick={() => onSort('name')}
              >
                {t('common.fields.name')}
              </TableSortLabel>
            </TableCell>
        
          
            <TableCell>{t('administration.exercises.fields.measurementType')}</TableCell>
            <TableCell>{t('administration.exercises.fields.primaryMuscle')}</TableCell>
            <TableCell>{t('administration.exercises.fields.secondaryMuscle')}</TableCell>
            <TableCell align="right">{t('common.actions.actions')}</TableCell>
          </TableRow>
        )}
        body={(
          <>
            {items.map((row) => (
              <TableRow key={row.id ?? row.code}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={row.id !== undefined && safeSelectedIds.includes(row.id)}
                    onChange={(_event, checked) => { if (row.id) onToggleSelect(row.id, checked); }}
                    inputProps={{ 'aria-label': t('administration.exercises.selectRow', { code: row.code }) }}
                  />
                </TableCell>
                <TableCell>{row.code}</TableCell>
                <TableCell>{row.name}</TableCell>
                <TableCell>{(row.units ?? []).map((item) => item.name).join(', ') || '-'}</TableCell>
                <TableCell sx={{ fontSize: '0.75rem' }}>{(row.primaryMuscles ?? []).map((item) => item.name).join(', ') || '-'}</TableCell>
                <TableCell sx={{ fontSize: '0.75rem' }}>{(row.secondaryMuscles ?? []).map((item) => item.name).join(', ') || '-'}</TableCell>

                <TableCell align="right">
                  <IconButton
                    size="small"
                    aria-label={t('common.actionsMenu')}
                    onClick={(event) => { if (row.id) openMenu(event, row.id); }}
                  >
                    <MoreVertRoundedIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </>
        )}
        pagination={{
          count: totalCount,
          page: page ?? 0,
          rowsPerPage: rowsPerPage ?? 10,
          onPageChange,
          onRowsPerPageChange,
        }}
      />

      <Menu
        anchorEl={menuAnchorEl}
        open={Boolean(menuAnchorEl)}
        onClose={closeMenu}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <MenuItem
          onClick={() => {
            if (selectedRow) {
              onEdit(selectedRow);
            }
            closeMenu();
          }}
        >
          <ListItemIcon>
            <EditRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.actions.edit')}</ListItemText>
        </MenuItem>

        <MenuItem
          onClick={() => {
            if (selectedRow) {
              onDelete(selectedRow);
            }
            closeMenu();
          }}
        >
          <ListItemIcon>
            <DeleteOutlineRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.actions.delete')}</ListItemText>
        </MenuItem>

      </Menu>
    </>
  );
}
