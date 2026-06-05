import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Drawer from '@mui/material/Drawer';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { fetchUnits, setUnitsFilters, setUnitsPopUpCode, setUnitsTable } from '../../../../redux/actions/units/unitsActions';
import { selectUnitState } from '../../../../redux/states/units/unitsState';
import { PopUpCode } from '../../../../enums/popUp/popUp';

export function UnitsFiltersDrawer() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { filters, table, popUpCode } = useAppSelector(selectUnitState);
  const open = popUpCode === PopUpCode.Filter;

  const handleClose = () => dispatch(setUnitsPopUpCode(PopUpCode.Default));

  const handleApply = () => {
    dispatch(setUnitsTable({ ...table, page: 0 }));
    dispatch(setUnitsPopUpCode(PopUpCode.Default));
    void dispatch(fetchUnits());
  };

  const handleClear = () => {
    dispatch(setUnitsFilters({ code: '', name: '', description: '' }));
    dispatch(setUnitsTable({ ...table, page: 0 }));
    void dispatch(fetchUnits());
  };

  return (
    <Drawer anchor="right" open={open} onClose={handleClose}>
      <Box
        sx={{
          px: 2,
          py: 2,
          background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
          color: '#f4f8f7',
        }}
      >
        <Typography variant="h6" fontWeight={800} sx={{ letterSpacing: 0.3, lineHeight: 1.2 }}>
          {t('common.filters')}
        </Typography>
        <Typography variant="caption" sx={{ opacity: 0.85 }}>
          {t('app.tagline')}
        </Typography>
      </Box>
      <Stack spacing={2} sx={{ width: { xs: 280, sm: 340 }, p: 2 }}>
        <Stack direction="row" spacing={1}>
          <Button
            variant="outlined"
            sx={{
              color: 'text.primary',
              borderColor: 'divider',
              '&:hover': {
                borderColor: 'text.primary',
                bgcolor: 'action.hover',
              },
            }}
            onClick={handleClear}
          >
            {t('common.clearFiltersAction')}
          </Button>
          <Button variant="contained" onClick={handleApply}>
            {t('common.search')}
          </Button>
        </Stack>
    
        <TextField
          label={t('common.fields.code')}
          value={filters.code}
          onChange={(event) => dispatch(setUnitsFilters({ ...filters, code: event.target.value }))}
          fullWidth
        />
        <TextField
          label={t('common.fields.name')}
          value={filters.name}
          onChange={(event) => dispatch(setUnitsFilters({ ...filters, name: event.target.value }))}
          fullWidth
        />
      </Stack>
    </Drawer>
  );
}
