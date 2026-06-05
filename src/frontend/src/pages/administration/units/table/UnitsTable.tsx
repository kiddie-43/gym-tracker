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
import type { IUnit } from '../../../../interfaces/units/IUnit';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import { fetchUnits, setUnitsForm, setUnitsPopUpCode, setUnitsTable } from '../../../../redux/actions/units/unitsActions';
import { selectUnitState } from '../../../../redux/states/units/unitsState';

export function UnitsTable() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { table } = useAppSelector(selectUnitState);

  const rows = table.items;
  const selectedIds = table.selectedIds ?? [];
  const sortBy = (table.sortBy as 'code' | 'name' | 'description') || 'code';
  const sortDirection = table.sortDirection || 'asc';
  const page = table.page || 0;
  const rowsPerPage = table.pageSize || 10;
  const totalCount = table.totalCount;

  const [menuAnchorEl, setMenuAnchorEl] = useState<HTMLElement | null>(null);
  const [menuRowId, setMenuRowId] = useState<string | null>(null);

  const activeRowIds = rows.map((row) => row.id).filter((id): id is string => id !== undefined);
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
    dispatch(setUnitsTable({ ...table, selectedIds: checked ? activeRowIds : [] }));
  };

  const toggleSelectRow = (rowId: string, checked: boolean) => {
    const next = checked
      ? Array.from(new Set([...selectedIds, rowId]))
      : selectedIds.filter((id) => id !== rowId);
    dispatch(setUnitsTable({ ...table, selectedIds: next }));
  };

  const handleSortChange = (field: 'code' | 'name' | 'description') => {
    const newDirection = sortBy === field && sortDirection === 'asc' ? 'desc' : 'asc';
    dispatch(setUnitsTable({ ...table, sortBy: field, sortDirection: newDirection, page: 0 }));
    void dispatch(fetchUnits());
  };

  const handlePageChange = (_event: unknown, newPage: number) => {
    dispatch(setUnitsTable({ ...table, page: newPage }));
    void dispatch(fetchUnits());
  };

  const handleRowsPerPageChange = (event: ChangeEvent<HTMLInputElement>) => {
    dispatch(setUnitsTable({ ...table, pageSize: Number.parseInt(event.target.value, 10), page: 0 }));
    void dispatch(fetchUnits());
  };

  const handleDelete = (row: IUnit) => {
    dispatch(setUnitsTable({
      ...table,
      selectedIds: row.id ? [row.id] : [],
    }));
    dispatch(setUnitsPopUpCode(PopUpCode.Delete));
    closeMenu();
  };

  const handleEdit = (row: IUnit) => {
    dispatch(setUnitsForm({ ...row }));
    dispatch(setUnitsPopUpCode(PopUpCode.Update));
    closeMenu();
  };

  return (
    <>
      <DataTable
        ariaLabel={t('administration.measurementTypes.table')}
        columnWidths={['4%', '20%', '24%', '34%', '8%']}
        head={(
          <TableRow>
            <TableCell padding="checkbox">
              <Checkbox
                checked={allActiveSelected}
                indeterminate={someActiveSelected}
                onChange={(_event, checked) => toggleSelectAll(checked)}
                inputProps={{ 'aria-label': t('administration.measurementTypes.selectAllRows') }}
              />
            </TableCell>
            <TableCell sortDirection={sortBy === 'code' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'code'}
                direction={sortBy === 'code' ? sortDirection : 'asc'}
                onClick={() => handleSortChange('code')}
              >
                {t('common.fields.code')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'name' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'name'}
                direction={sortBy === 'name' ? sortDirection : 'asc'}
                onClick={() => handleSortChange('name')}
              >
                {t('common.fields.name')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'description' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'description'}
                direction={sortBy === 'description' ? sortDirection : 'asc'}
                onClick={() => handleSortChange('description')}
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
              <TableRow key={row.id ?? row.code}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={row.id !== undefined && selectedIds.includes(row.id)}
                    onChange={(_event, checked) => { if (row.id) toggleSelectRow(row.id, checked); }}
                    inputProps={{ 'aria-label': t('administration.measurementTypes.selectRow', { code: row.code }) }}
                  />
                </TableCell>
                <TableCell>{row.code}</TableCell>
                <TableCell>{row.name}</TableCell>
                <TableCell>{row.description ?? '-'}</TableCell>

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
          page,
          rowsPerPage,
          onPageChange: handlePageChange,
          onRowsPerPageChange: handleRowsPerPageChange,
        }}
      />

      <Menu
        anchorEl={menuAnchorEl}
        open={Boolean(menuAnchorEl)}
        onClose={closeMenu}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <MenuItem onClick={() => { if (selectedRow) handleEdit(selectedRow); }}>
          <ListItemIcon>
            <EditRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.actions.edit')}</ListItemText>
        </MenuItem>

        <MenuItem onClick={() => { if (selectedRow) handleDelete(selectedRow); }}>
          <ListItemIcon>
            <DeleteOutlineRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.actions.delete')}</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
