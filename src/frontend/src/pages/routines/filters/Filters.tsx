import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';

import FilterListIcon from '@mui/icons-material/FilterList';
import Button from '@mui/material/Button';
import Drawer from '@mui/material/Drawer';
import FormControl from '@mui/material/FormControl';
import IconButton from '@mui/material/IconButton';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import { setRoutinesFilters } from '../../../redux/actions/routines/routinesActions';
import type { AppDispatch } from '../../../redux/store';
import { selectRoutinesState } from '../../../redux/states/routines/routinesState';

export function Filters() {
  const dispatch = useDispatch<AppDispatch>();
  const { filters } = useSelector(selectRoutinesState);
  const [openFiltersDrawer, setOpenFiltersDrawer] = useState(false);

  const handleResetFilters = () => {
    dispatch(setRoutinesFilters({
      search: '',
      startDate: '',
      endDate: '',
      status: 'active',
    }));
  };

  return (
    <>
      <IconButton aria-label="Filtros" onClick={() => setOpenFiltersDrawer(true)}>
        <FilterListIcon />
      </IconButton>

      <Drawer anchor="left" open={openFiltersDrawer} onClose={() => setOpenFiltersDrawer(false)}>
        <Stack spacing={2} sx={{ width: 320, p: 2 }}>
          <Typography variant="h6" fontWeight={700}>Filtros de rutinas</Typography>

          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={handleResetFilters}>Limpiar</Button>
            <Button variant="contained" onClick={() => setOpenFiltersDrawer(false)}>Aplicar</Button>
          </Stack>

          <TextField
            label="Nombre"
            value={filters.search}
            onChange={(event) => dispatch(setRoutinesFilters({ ...filters, search: event.target.value }))}
            fullWidth
          />

          <TextField
            label="Fecha desde"
            type="date"
            value={filters.startDate}
            onChange={(event) => dispatch(setRoutinesFilters({ ...filters, startDate: event.target.value }))}
            InputLabelProps={{ shrink: true }}
            fullWidth
          />

          <TextField
            label="Fecha hasta"
            type="date"
            value={filters.endDate}
            onChange={(event) => dispatch(setRoutinesFilters({ ...filters, endDate: event.target.value }))}
            InputLabelProps={{ shrink: true }}
            fullWidth
          />

          <FormControl fullWidth>
            <InputLabel id="routines-status-filter-label">Estado</InputLabel>
            <Select
              labelId="routines-status-filter-label"
              label="Estado"
              value={filters.status}
              onChange={(event) => dispatch(setRoutinesFilters({
                ...filters,
                status: event.target.value as 'active' | 'archived' | 'all',
              }))}
            >
              <MenuItem value="active">Activas</MenuItem>
              <MenuItem value="archived">Archivadas</MenuItem>
              <MenuItem value="all">Todas</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Drawer>
    </>
  );
}