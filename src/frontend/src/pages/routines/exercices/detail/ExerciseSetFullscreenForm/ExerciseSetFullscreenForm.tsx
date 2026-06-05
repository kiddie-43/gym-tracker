import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import IconButton from '@mui/material/IconButton';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useEffect, useMemo, useState } from 'react';
import { alpha, useTheme } from '@mui/material/styles';

export interface ExerciseSetFormValues {
  weightKg: number;
  repetitions: number;
  rpe: number;
  restRecommendedSeconds: number;
  notes?: string;
}

interface ExerciseSetFullscreenFormProps {
  open: boolean;
  mode: 'create' | 'edit';
  initialValues?: ExerciseSetFormValues | null;
  onClose: () => void;
  onSave: (values: ExerciseSetFormValues) => void;
}

const defaultSetValues: ExerciseSetFormValues = {
  weightKg: 20,
  repetitions: 8,
  rpe: 8,
  restRecommendedSeconds: 90,
  notes: '',
};

export function ExerciseSetFullscreenForm({ open, mode, initialValues, onClose, onSave }: ExerciseSetFullscreenFormProps) {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));
  const [values, setValues] = useState<ExerciseSetFormValues>(defaultSetValues);

  useEffect(() => {
    if (!open) {
      return;
    }

    setValues(initialValues ?? defaultSetValues);
  }, [open, initialValues]);

  const canSave = useMemo(
    () => values.weightKg > 0 && values.repetitions > 0 && values.rpe >= 1 && values.rpe <= 10,
    [values],
  );

  const handleSave = () => {
    if (!canSave) {
      return;
    }

    onSave(values);
  };

  return (
    <Dialog
      open={open}
      fullScreen={fullScreen}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      PaperProps={{
        sx: {
          bgcolor: 'background.paper',
          borderRadius: { xs: 0, sm: 3 },
          overflow: 'hidden',
        },
      }}
    >
      <AppBar position="sticky" color="default" elevation={0}>
        <Toolbar>
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ width: '100%' }}>
            <Stack direction="row" spacing={1} alignItems="center">
              <IconButton onClick={onClose} edge="start" aria-label="Cerrar formulario de set">
                <CloseRoundedIcon />
              </IconButton>
              <Typography variant="h6" fontWeight={700}>
                {mode === 'create' ? 'Nuevo set' : 'Editar set'}
              </Typography>
            </Stack>
            <Button variant="contained" onClick={handleSave} disabled={!canSave}>
              Guardar set
            </Button>
          </Stack>
        </Toolbar>
      </AppBar>

      <Box sx={{ flex: 1, p: { xs: 2, sm: 3 } }}>
        <Stack spacing={2.5}>
          <Typography variant="body2" color="text.secondary">
            Registra tu set rapido y vuelve al entreno.
          </Typography>

          <Paper
            variant="outlined"
            sx={{
              p: 2,
              borderRadius: 3,
              borderColor: 'divider',
              bgcolor: (t) => alpha(t.palette.background.default, 0.5),
            }}
          >
            <Stack spacing={2}>
              <TextField
                label="Peso (kg)"
                type="number"
                fullWidth
                value={values.weightKg}
                onChange={(event) => setValues((current) => ({
                  ...current,
                  weightKg: Number(event.target.value || 0),
                }))}
                inputProps={{ min: 0, step: 0.5 }}
              />

              <TextField
                label="Repeticiones"
                type="number"
                fullWidth
                value={values.repetitions}
                onChange={(event) => setValues((current) => ({
                  ...current,
                  repetitions: Number(event.target.value || 0),
                }))}
                inputProps={{ min: 0, step: 1 }}
              />

              <TextField
                label="RPE"
                type="number"
                fullWidth
                value={values.rpe}
                onChange={(event) => setValues((current) => ({
                  ...current,
                  rpe: Number(event.target.value || 0),
                }))}
                inputProps={{ min: 1, max: 10, step: 0.5 }}
              />

              <TextField
                select
                label="Descanso recomendado"
                fullWidth
                value={values.restRecommendedSeconds}
                onChange={(event) => setValues((current) => ({
                  ...current,
                  restRecommendedSeconds: Number(event.target.value),
                }))}
              >
                <MenuItem value={45}>45 segundos</MenuItem>
                <MenuItem value={60}>60 segundos</MenuItem>
                <MenuItem value={90}>90 segundos</MenuItem>
                <MenuItem value={120}>120 segundos</MenuItem>
                <MenuItem value={150}>150 segundos</MenuItem>
              </TextField>

              <TextField
                label="Notas opcionales"
                fullWidth
                multiline
                minRows={3}
                placeholder="Sensaciones, tecnica, molestias..."
                value={values.notes ?? ''}
                onChange={(event) => setValues((current) => ({
                  ...current,
                  notes: event.target.value,
                }))}
              />
            </Stack>
          </Paper>

          <Stack direction="row" justifyContent="flex-end" spacing={1.5}>
            <Button variant="outlined" onClick={onClose}>Cancelar</Button>
            <Button variant="contained" onClick={handleSave} disabled={!canSave}>Guardar set</Button>
          </Stack>
        </Stack>
      </Box>
    </Dialog>
  );
}
