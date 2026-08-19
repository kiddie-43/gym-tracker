import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Drawer from '@mui/material/Drawer';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';
import { PopUpCode } from '../../../../enums/popUp/popUp';
import { fetchAdminMuscles, setMusclesFilters, setMusclesPopUpCode, setMusclesTable } from '../../../../redux/actions/muscles/musclesActions';
import { useAppDispatch, useAppSelector } from '../../../../redux/hooks';
import { selectMusclesState } from '../../../../redux/states/adminMuscles/adminMusclesState';

export function MusclesFiltersDrawer() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { popUpCode, filters, table } = useAppSelector(selectMusclesState);
  const open = popUpCode === PopUpCode.Filter;

  const onClose = () => dispatch(setMusclesPopUpCode(PopUpCode.Default));
  const onApplySearch = () => {
    dispatch(setMusclesTable({ ...table, page: 0 }));
    dispatch(setMusclesPopUpCode(PopUpCode.Default));
    void dispatch(fetchAdminMuscles());
  };
  const onClearFilters = () => {
    dispatch(setMusclesFilters({ code: '', name: '' }));
    dispatch(setMusclesTable({ ...table, page: 0 }));
    void dispatch(fetchAdminMuscles());
  };

  return (
    <>
      <Drawer anchor="right" open={open} onClose={onClose}>
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
        <Box sx={{ width: { xs: 280, sm: 340 }, p: 2 }}>
          <Stack spacing={2}>
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
                onClick={onClearFilters}
              >
                {t('common.clearFiltersAction')}
              </Button>
              <Button variant="contained" onClick={onApplySearch}>
                {t('common.search')}
              </Button>
            </Stack>

            <TextField
              label="Código"
              placeholder="Filtrar por código..."
              value={filters.code ?? ''}
              onChange={(event) => dispatch(setMusclesFilters({ ...filters, code: event.target.value }))}
              fullWidth
            />
            <TextField
              label="Nombre"
              placeholder="Filtrar por nombre..."
              value={filters.name ?? ''}
              onChange={(event) => dispatch(setMusclesFilters({ ...filters, name: event.target.value }))}
              fullWidth
            />
           
          </Stack>
        </Box>
      </Drawer>
    </>
  );
}
