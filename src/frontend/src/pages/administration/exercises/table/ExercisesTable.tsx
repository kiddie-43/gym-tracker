import { useState } from 'react';
import type { ChangeEvent } from 'react';

import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';
import RemoveCircleOutlineRoundedIcon from '@mui/icons-material/RemoveCircleOutlineRounded';
import ReplayRoundedIcon from '@mui/icons-material/ReplayRounded';
import Checkbox from '@mui/material/Checkbox';
import Chip from '@mui/material/Chip';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import TableSortLabel from '@mui/material/TableSortLabel';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableRow from '@mui/material/TableRow';
import { useTranslation } from 'react-i18next';

import { DataTable } from '../../../../components/DataTable/DataTable';
import type { ExerciseDto } from '../../../../interfaces/admin/exercises/exercises';

type ExercisesTableProps = {
  rows: ExerciseDto[];
  selectedIds: string[];
  sortBy: 'code' | 'name' | 'category' | 'difficulty';
  sortDirection: 'asc' | 'desc';
  onSortChange: (field: 'code' | 'name' | 'category' | 'difficulty') => void;
  page: number;
  rowsPerPage: number;
  totalCount: number;
  onPageChange: (_event: unknown, newPage: number) => void;
  onRowsPerPageChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onSelectionChange: (ids: string[]) => void;
  onEdit: (row: ExerciseDto) => void;
  onDelete: (row: ExerciseDto) => void;
  onReactivate: (row: ExerciseDto) => void;
};

export function ExercisesTable({
  rows,
  selectedIds,
  sortBy,
  sortDirection,
  onSortChange,
  page,
  rowsPerPage,
  totalCount,
  onPageChange,
  onRowsPerPageChange,
  onSelectionChange,
  onEdit,
  onDelete,
  onReactivate,
}: ExercisesTableProps) {
  const { t } = useTranslation();
  const [menuAnchorEl, setMenuAnchorEl] = useState<HTMLElement | null>(null);
  const [menuRowId, setMenuRowId] = useState<string | null>(null);

  const activeRowIds = rows.filter((row) => !row.isDeleted).map((row) => row.id);
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
    if (checked) {
      onSelectionChange(activeRowIds);
      return;
    }
    onSelectionChange([]);
  };

  const toggleSelectRow = (rowId: string, checked: boolean) => {
    if (checked) {
      onSelectionChange(Array.from(new Set([...selectedIds, rowId])));
      return;
    }
    onSelectionChange(selectedIds.filter((id) => id !== rowId));
  };

  return (
    <>
      <DataTable
        ariaLabel={t('administration.exercises.table')}
        columnWidths={['4%', '9%', '14%', '9%', '9%', '10%', '15%', '15%', '9%', '6%']}
        head={(
          <TableRow>
            <TableCell padding="checkbox">
              <Checkbox
                checked={allActiveSelected}
                indeterminate={someActiveSelected}
                onChange={(_event, checked) => toggleSelectAll(checked)}
                inputProps={{ 'aria-label': t('administration.exercises.selectAllRows') }}
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
                {t('administration.common.fields.name')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'category' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'category'}
                direction={sortBy === 'category' ? sortDirection : 'asc'}
                onClick={() => onSortChange('category')}
              >
                {t('administration.exercises.fields.category')}
              </TableSortLabel>
            </TableCell>
            <TableCell sortDirection={sortBy === 'difficulty' ? sortDirection : false}>
              <TableSortLabel
                active={sortBy === 'difficulty'}
                direction={sortBy === 'difficulty' ? sortDirection : 'asc'}
                onClick={() => onSortChange('difficulty')}
              >
                {t('administration.exercises.fields.difficulty')}
              </TableSortLabel>
            </TableCell>
            <TableCell>{t('administration.exercises.fields.measurementType')}</TableCell>
            <TableCell>{t('administration.exercises.fields.primaryMuscle')}</TableCell>
            <TableCell>{t('administration.exercises.fields.secondaryMuscle')}</TableCell>
            <TableCell>{t('administration.common.status')}</TableCell>
            <TableCell align="right">{t('common.actions.actions')}</TableCell>
          </TableRow>
        )}
        body={(
          <>
            {rows.map((row) => (
              <TableRow key={row.id}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={selectedIds.includes(row.id)}
                    disabled={row.isDeleted}
                    onChange={(_event, checked) => toggleSelectRow(row.id, checked)}
                    inputProps={{ 'aria-label': t('administration.exercises.selectRow', { code: row.code }) }}
                  />
                </TableCell>
                <TableCell>{row.code}</TableCell>
                <TableCell>{row.name}</TableCell>
                <TableCell>{row.category}</TableCell>
                <TableCell>{row.difficulty}</TableCell>
                <TableCell>{row.measurementTypeName}</TableCell>
                <TableCell sx={{ fontSize: '0.75rem' }}>{row.primaryMuscles.map(m => m.name).join(', ')}</TableCell>
                <TableCell sx={{ fontSize: '0.75rem' }}>{row.secondaryMuscles.map(m => m.name).join(', ')}</TableCell>
                <TableCell>
                  <Chip
                    size="small"
                    color={row.isDeleted ? 'default' : 'success'}
                    icon={row.isDeleted ? <RemoveCircleOutlineRoundedIcon /> : <CheckCircleRoundedIcon />}
                    label={row.isDeleted ? t('administration.common.statusInactive') : t('administration.common.statusActive')}
                  />
                </TableCell>
                <TableCell align="right">
                  <IconButton
                    size="small"
                    aria-label={t('administration.common.actionsMenu')}
                    onClick={(event) => openMenu(event, row.id)}
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
          disabled={Boolean(selectedRow?.isDeleted)}
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
        {selectedRow?.isDeleted ? (
          <MenuItem
            onClick={() => {
              onReactivate(selectedRow);
              closeMenu();
            }}
          >
            <ListItemIcon>
              <ReplayRoundedIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{t('administration.common.reactivate')}</ListItemText>
          </MenuItem>
        ) : (
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
        )}
      </Menu>
    </>
  );
}
