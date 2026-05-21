import { useEffect, useMemo, useState } from 'react';

import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import CircularProgress from '@mui/material/CircularProgress';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import FormControlLabel from '@mui/material/FormControlLabel';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { ImportMusclesResult, MuscleDto, UpsertMuscleRequest } from '../../interfaces/admin/muscles/muscles';
import {
  createMuscle,
  deleteMuscle,
  importMusclesCsv,
  listMuscles,
  reactivateMuscle,
  updateMuscle,
} from '../../services/api/admin/muscles/musclesApi';
import { MusclesCsvImportDialog } from './MusclesCsvImportDialog';

type FormState = {
  id: string | null;
  name: string;
  code: string;
  description: string;
};

const defaultFormState: FormState = {
  id: null,
  name: '',
  code: '',
  description: '',
};

export function MusclesPanel() {
  const { t } = useTranslation();
  const [rows, setRows] = useState<MuscleDto[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [includeDeleted, setIncludeDeleted] = useState<boolean>(false);

  const [formOpen, setFormOpen] = useState<boolean>(false);
  const [formLoading, setFormLoading] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<FormState>(defaultFormState);

  const [csvOpen, setCsvOpen] = useState<boolean>(false);
  const [csvLoading, setCsvLoading] = useState<boolean>(false);
  const [csvError, setCsvError] = useState<string | null>(null);
  const [csvResult, setCsvResult] = useState<ImportMusclesResult | null>(null);

  const visibleRows = useMemo(() => rows, [rows]);

  const loadRows = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await listMuscles(includeDeleted);
      setRows(data);
    } catch (loadError) {
      const message = loadError instanceof Error ? loadError.message : t('administration.common.loadError');
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadRows();
  }, [includeDeleted]);

  const openCreate = () => {
    setFormState(defaultFormState);
    setFormError(null);
    setFormOpen(true);
  };

  const openEdit = (row: MuscleDto) => {
    setFormState({
      id: row.id,
      name: row.name,
      code: row.code,
      description: row.description ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  };

  const closeForm = () => {
    if (formLoading) {
      return;
    }

    setFormOpen(false);
    setFormState(defaultFormState);
    setFormError(null);
  };

  const submitForm = async () => {
    setFormLoading(true);
    setFormError(null);

    const request: UpsertMuscleRequest = {
      name: formState.name.trim(),
      code: formState.code.trim(),
      description: formState.description.trim() || null,
      muscleGroupIds: ['general'],
      active: true,
    };

    try {
      if (formState.id) {
        await updateMuscle(formState.id, request);
      } else {
        await createMuscle(request);
      }

      closeForm();
      await loadRows();
    } catch (submitError) {
      const message = submitError instanceof Error ? submitError.message : t('administration.common.saveError');
      setFormError(message);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async (row: MuscleDto) => {
    setError(null);

    try {
      await deleteMuscle(row.id);
      await loadRows();
    } catch (deleteError) {
      const message = deleteError instanceof Error ? deleteError.message : t('administration.common.deleteError');
      setError(message);
    }
  };

  const handleReactivate = async (row: MuscleDto) => {
    setError(null);

    try {
      await reactivateMuscle(row.id);
      await loadRows();
    } catch (reactivateError) {
      const message = reactivateError instanceof Error
        ? reactivateError.message
        : t('administration.common.reactivateError');
      setError(message);
    }
  };

  const handleCsvImport = async (importRows: Array<{ code?: string; description?: string }>) => {
    setCsvLoading(true);
    setCsvError(null);

    try {
      const result = await importMusclesCsv({ rows: importRows });
      setCsvResult(result);
      await loadRows();
    } catch (importError) {
      const message = importError instanceof Error ? importError.message : t('administration.muscles.csv.importError');
      setCsvError(message);
    } finally {
      setCsvLoading(false);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={1}>
        <Typography variant="h5" fontWeight={700}>{t('administration.muscles.title')}</Typography>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" onClick={() => setCsvOpen(true)}>
            {t('administration.muscles.csv.openAction')}
          </Button>
          <Button variant="contained" onClick={openCreate}>
            {t('common.actions.add')}
          </Button>
        </Stack>
      </Stack>

      <FormControlLabel
        control={(
          <Checkbox
            checked={includeDeleted}
            onChange={(_event, checked) => setIncludeDeleted(checked)}
          />
        )}
        label={t('administration.common.includeDeleted')}
      />

      {error ? (
        <Alert
          severity="error"
          action={(
            <Button color="inherit" size="small" onClick={() => void loadRows()}>
              {t('administration.common.retry')}
            </Button>
          )}
        >
          {error}
        </Alert>
      ) : null}

      {loading ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <CircularProgress aria-label={t('administration.common.loading')} />
        </Paper>
      ) : null}

      {!loading && visibleRows.length === 0 ? (
        <Paper sx={{ p: 4 }}>
          <Typography>{t('common.messages.noData')}</Typography>
        </Paper>
      ) : null}

      {!loading && visibleRows.length > 0 ? (
        <Paper>
          <Table aria-label={t('administration.muscles.table')}>
            <TableHead>
              <TableRow>
                <TableCell>{t('common.fields.code')}</TableCell>
                <TableCell>{t('administration.common.fields.name')}</TableCell>
                <TableCell>{t('common.fields.description')}</TableCell>
                <TableCell align="right">{t('common.actions.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {visibleRows.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.code}</TableCell>
                  <TableCell>{row.name}</TableCell>
                  <TableCell>{row.description ?? '-'}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button size="small" onClick={() => openEdit(row)} disabled={Boolean(row.isDeleted)}>
                        {t('common.actions.edit')}
                      </Button>
                      {row.isDeleted ? (
                        <Button size="small" color="success" onClick={() => void handleReactivate(row)}>
                          {t('administration.common.reactivate')}
                        </Button>
                      ) : (
                        <Button size="small" color="error" onClick={() => void handleDelete(row)}>
                          {t('common.actions.delete')}
                        </Button>
                      )}
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      ) : null}

      <Dialog open={formOpen} onClose={closeForm} fullWidth maxWidth="sm">
        <DialogTitle>
          {formState.id ? t('administration.muscles.editTitle') : t('administration.muscles.createTitle')}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label={t('administration.common.fields.name')}
              value={formState.name}
              onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
              fullWidth
            />
            <TextField
              label={t('common.fields.code')}
              value={formState.code}
              onChange={(event) => setFormState((current) => ({ ...current, code: event.target.value }))}
              disabled={Boolean(formState.id)}
              helperText={formState.id ? t('common.messages.codeReadOnly') : undefined}
              fullWidth
            />
            <TextField
              label={t('common.fields.description')}
              value={formState.description}
              onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
              fullWidth
            />
            {formError ? <Alert severity="error">{formError}</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeForm} disabled={formLoading}>{t('common.actions.cancel')}</Button>
          <Button onClick={() => void submitForm()} disabled={formLoading} variant="contained">
            {t('common.actions.save')}
          </Button>
        </DialogActions>
      </Dialog>

      <MusclesCsvImportDialog
        open={csvOpen}
        loading={csvLoading}
        error={csvError}
        result={csvResult}
        onClose={() => {
          if (csvLoading) {
            return;
          }

          setCsvOpen(false);
          setCsvError(null);
        }}
        onImport={handleCsvImport}
      />
    </Stack>
  );
}
