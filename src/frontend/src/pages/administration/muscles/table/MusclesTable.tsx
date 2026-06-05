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

import { DataTable } from '../../../../components/DataTable/DataTable';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import type { IMuscle } from '../../../../interfaces/muscles/IMuscles';
import { fetchAdminMuscles, setMusclesForm, setMusclesPopUpCode, setMusclesTable } from '../../../../redux/actions/muscles/musclesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { selectMusclesState } from '../../../../redux/states/adminMuscles/adminMusclesState';

export function MusclesTable() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { table } = useAppSelector(selectMusclesState);
  const rows = table.items;
  const selectedIds = table.selectedIds ?? [];
  const sortBy = (table.sortBy as 'code' | 'name' | 'description') || 'name';
  const sortDirection = table.sortDirection || 'asc';
  const page = table.page || 0;
  const rowsPerPage = table.pageSize || 10;
  const totalCount = table.totalCount;
  const [menuAnchorEl, setMenuAnchorEl] = useState<HTMLElement | null>(null);
  const [menuRowId, setMenuRowId] = useState<string | null>(null);
  const activeRowIds = rows
    .filter((row) => typeof row.id === 'string')
    .map((row) => row.id as string);
  const selectedActiveCount = activeRowIds.filter((id) => selectedIds.includes(id)).length;
  const allActiveSelected = activeRowIds.length > 0 && selectedActiveCount === activeRowIds.length;
  const someActiveSelected = selectedActiveCount > 0 && !allActiveSelected;

  const openMenu = (event: React.MouseEvent<HTMLElement>, rowId: string) => {
    setMenuAnchorEl(event.currentTarget);
    setMenuRowId(rowId);
  };

  const closeMenu = () => {
    setMenuAnchorEl(null);
    setMenuRowId(null);
  };

  const selectedRow = rows.find((row) => row.id === menuRowId) ?? null;

  const toggleSelectAll = (checked: boolean) => {
    dispatch(setMusclesTable({ ...table, selectedIds: checked ? activeRowIds : [] }));
  };

  const toggleSelectRow = (rowId: string, checked: boolean) => {
    const next = checked
      ? Array.from(new Set([...selectedIds, rowId]))
      : selectedIds.filter((id) => id !== rowId);
    dispatch(setMusclesTable({ ...table, selectedIds: next }));
  };

  const onSortChange = (field: 'code' | 'name' | 'description') => {
    const newDirection = sortBy === field && sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setMusclesTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchAdminMuscles());
  };

  const onPageChange = (_event: unknown, newPage: number) => {
    dispatch(setMusclesTable({ ...table, page: newPage }));
    void dispatch(fetchAdminMuscles());
  };

  const onRowsPerPageChange = (event: ChangeEvent<HTMLInputElement>) => {
    dispatch(setMusclesTable({ ...table, pageSize: Number.parseInt(event.target.value, 10), page: 0 }));
    void dispatch(fetchAdminMuscles());
  };

  const onEdit = (row: IMuscle) => {
    dispatch(setMusclesForm({ ...row }));
    dispatch(setMusclesPopUpCode(PopUpCode.Update));
  };

  const onDelete = (row: IMuscle) => {
    dispatch(setMusclesTable({ ...table, selectedIds: row.id ? [row.id] : [] }));
    dispatch(setMusclesPopUpCode(PopUpCode.Delete));
  };

  return (
    <>
      <DataTable
        ariaLabel={t('administration.muscles.table')}
        columnWidths={['6%', '13%', '13%', '46%', '10%']}
        head={(
          <TableRow>
            <TableCell padding="checkbox">
              <Checkbox
                checked={allActiveSelected}
                indeterminate={someActiveSelected}
                onChange={(_event, checked) => toggleSelectAll(checked)}
                inputProps={{ 'aria-label': t('administration.muscles.selectAllRows') }}
              />
            </TableCell>
            <TableCell sortDirection={sortBy === 'code' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'code'}
                direction={sortBy === 'code' ? sortDirection : 'asc'}
                onClick={() => onSortChange('code')}
              >
                {t('common.fields.code')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'name' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'name'}
                direction={sortBy === 'name' ? sortDirection : 'asc'}
                onClick={() => onSortChange('name')}
              >
                {t('common.fields.name')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'description' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'description'}
                direction={sortBy === 'description' ? sortDirection : 'asc'}
                onClick={() => onSortChange('description')}
              >
                {t('common.fields.description')}
              </TableSortLabel>
            </TableCell>
            <TableCell align="right">{t('common.actions.actions')}</TableCell>
          </TableRow>
        )}
        body={(
          <>
            {rows.map((row) => (
              <TableRow key={row.id ?? `${row.code}-${row.name}`}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={typeof row.id === 'string' ? selectedIds.includes(row.id) : false}
                    disabled={typeof row.id !== 'string'}
                    onChange={(_event, checked) => {
                      if (typeof row.id === 'string') {
                        toggleSelectRow(row.id, checked);
                      }
                    }}
                    inputProps={{ 'aria-label': t('administration.muscles.selectRow', { code: row.code }) }}
                  />
                </TableCell>
                <TableCell>{row.code}</TableCell>
                <TableCell>{row.name}</TableCell>
                <TableCell>{row.description ?? '-'}</TableCell>
                <TableCell align="right">
                  <IconButton
                    size="small"
                    aria-label={t('common.actionsMenu')}
                    onClick={(event) => {
                      if (typeof row.id === 'string') {
                        openMenu(event, row.id);
                      }
                    }}
                    disabled={typeof row.id !== 'string'}
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
          page,
          rowsPerPage,
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
