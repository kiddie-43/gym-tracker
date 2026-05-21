import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import Drawer from '@mui/material/Drawer';
import FormControlLabel from '@mui/material/FormControlLabel';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

type MusclesFiltersDrawerProps = {
  open: boolean;
  searchInput: string;
  filterCode: string;
  filterName: string;
  includeDeleted: boolean;
  onClose: () => void;
  onSearchInputChange: (value: string) => void;
  onFilterCodeChange: (value: string) => void;
  onFilterNameChange: (value: string) => void;
  onIncludeDeletedChange: (value: boolean) => void;
  onApplySearch: () => void;
  onClearFilters: () => void;
};

export function MusclesFiltersDrawer({
  open,
  searchInput,
  filterCode,
  filterName,
  includeDeleted,
  onClose,
  onSearchInputChange,
  onFilterCodeChange,
  onFilterNameChange,
  onIncludeDeletedChange,
  onApplySearch,
  onClearFilters,
}: MusclesFiltersDrawerProps) {
  const { t } = useTranslation();

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
            {t('administration.common.filters')}
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
                {t('administration.common.clearFiltersAction')}
              </Button>
              <Button variant="contained" onClick={onApplySearch}>
                {t('administration.common.searchAction')}
              </Button>
            </Stack>
            <Stack spacing={1}>
              <Typography variant="caption" color="text.secondary">
                {t('administration.muscles.searchHint')}
              </Typography>
              <TextField
                label={t('administration.common.searchLabel')}
                placeholder={t('administration.common.searchPlaceholder')}
                value={searchInput}
                onChange={(event) => onSearchInputChange(event.target.value)}
                fullWidth
              />
            </Stack>
            <TextField
              label="Código"
              placeholder="Filtrar por código..."
              value={filterCode}
              onChange={(event) => onFilterCodeChange(event.target.value)}
              fullWidth
            />
            <TextField
              label="Nombre"
              placeholder="Filtrar por nombre..."
              value={filterName}
              onChange={(event) => onFilterNameChange(event.target.value)}
              fullWidth
            />
            <FormControlLabel
              control={<Checkbox checked={includeDeleted} onChange={(_event, checked) => onIncludeDeletedChange(checked)} />}
              label={t('administration.common.includeDeleted')}
            />
          </Stack>
        </Box>
      </Drawer>
    </>
  );
}
