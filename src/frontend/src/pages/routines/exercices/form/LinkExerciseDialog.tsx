import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';

import SearchRoundedIcon from '@mui/icons-material/SearchRounded';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import InputAdornment from '@mui/material/InputAdornment';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import type { IExercise } from '../../../../interfaces/IExercises/IExercises';
import { listExercisesPage } from '../../../../services/api/exercises/exercisesApi';
import { setExerciseFormAction } from '../../../../redux/actions/exercises/exercisesActions';
import type { AppDispatch } from '../../../../redux/store';
import { selectExercisesState } from '../../../../redux/states/exercises/exercisesState';

interface LinkExerciseDialogProps {
  open: boolean;
  linkedExerciseIds: string[];
  onClose: () => void;
  onLink: (exerciseId: string, name: string) => void;
}

export function LinkExerciseDialog({ open, linkedExerciseIds, onClose, onLink }: LinkExerciseDialogProps) {
  const dispatch = useDispatch<AppDispatch>();
  const { form } = useSelector(selectExercisesState);
  const [catalog, setCatalog] = useState<IExercise[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');

  useEffect(() => {
    if (!open) {
      return;
    }

    let isDisposed = false;

    const loadCatalog = async () => {
      setLoading(true);
      try {
        const response = await listExercisesPage({
          page: 0,
          pageSize: 100,
        });

        if (!isDisposed) {
          setCatalog(response.items ?? []);
        }
      } catch {
        if (!isDisposed) {
          setCatalog([]);
        }
      } finally {
        if (!isDisposed) {
          setLoading(false);
        }
      }
    };

    void loadCatalog();

    return () => {
      isDisposed = true;
    };
  }, [open, search]);

  const filtered = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return catalog.filter((ex) => {
      if (linkedExerciseIds.includes(ex.id ?? '')) {
        return false;
      }

      if (!normalizedSearch) {
        return true;
      }

      return [ex.name, ex.code, ex.description]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(normalizedSearch));
    });
  }, [catalog, linkedExerciseIds, search]);

  const selectedExercise = catalog.find((ex) => ex.id === form.id) ?? null;

  const handleClose = () => {
    setSearch('');
    dispatch(setExerciseFormAction());
    onClose();
  };

  const handleSubmit = () => {
    if (!selectedExercise?.id) {
      return;
    }

    onLink(selectedExercise.id, selectedExercise.name);
    handleClose();
  };

  return (
    <PopupDialog
      open={open}
      title="Añadir ejercicio"
      onClose={handleClose}
      onSubmit={handleSubmit}
      saveLabel="Añadir"
      closeLabel="Cancelar"
      disableSave={!selectedExercise?.id}
    >
      <TextField
        fullWidth
        placeholder="Buscar ejercicio..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchRoundedIcon />
            </InputAdornment>
          ),
        }}
        sx={{ mb: 1.5 }}
        variant="outlined"
        size="small"
      />

      <Box
        sx={{
          height: 360,
          minHeight: 360,
          maxHeight: 360,
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {loading ? (
          <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <CircularProgress size={28} />
          </Box>
        ) : filtered.length === 0 ? (
          <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', px: 1 }}>
              {catalog.length === 0 ? 'No hay ejercicios en el catálogo' : 'Sin resultados para tu búsqueda'}
            </Typography>
          </Box>
        ) : (
          <List
            disablePadding
            sx={{
              height: '100%',
              overflowY: 'auto',
              px: 0.5,
            }}
          >
            {filtered.map((ex) => (
              <ListItem key={ex.id} disablePadding>
                <ListItemButton
                  selected={form.id === ex.id}
                  onClick={() => dispatch(setExerciseFormAction(ex))}
                >
                  <ListItemText
                    primary={ex.name}
                    //secondary={[ex.category, ex.difficulty].filter(Boolean).join(' · ')}
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        )}
      </Box>
    </PopupDialog>
  );
}