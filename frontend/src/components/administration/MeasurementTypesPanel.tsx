import { useEffect, useState } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
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

import type { MeasurementTypeDto, UpsertMeasurementTypeRequest } from '../../interfaces/admin/measurementTypes/measurementTypes';
import {
  createMeasurementType,
  deleteMeasurementType,
  listMeasurementTypes,
  reactivateMeasurementType,
  updateMeasurementType,
} from '../../services/api/admin/measurementTypes/measurementTypesApi';

type FormState = {
  id: string | null;
  name: string;
  fieldsText: string;
};

const defaultFormState: FormState = {
  id: null,
  name: '',
  fieldsText: '',
};

function toFields(text: string) {
  return text
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0)
    .map((name) => ({ name }));
}

export function MeasurementTypesPanel() {
  const { t } = useTranslation();
  const [rows, setRows] = useState<MeasurementTypeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [includeDeleted, setIncludeDeleted] = useState(false);

  const [formOpen, setFormOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formState, setFormState] = useState<FormState>(defaultFormState);

  const loadRows = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await listMeasurementTypes(includeDeleted);
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

  const closeForm = () => {
    if (formLoading) {
      return;
    }

    setFormOpen(false);
    setFormError(null);
    setFormState(defaultFormState);
  };

  const openCreate = () => {
    setFormState(defaultFormState);
    setFormError(null);
    setFormOpen(true);
  };

  const openEdit = (row: MeasurementTypeDto) => {
    setFormState({
      id: row.id,
      name: row.name,
      fieldsText: row.fields.map((field) => field.name).join(', '),
    });
    setFormError(null);
    setFormOpen(true);
  };

  const submitForm = async () => {
    setFormLoading(true);
    setFormError(null);

    const request: UpsertMeasurementTypeRequest = {
      name: formState.name.trim(),
      fields: toFields(formState.fieldsText),
    };

    try {
      if (formState.id) {
        await updateMeasurementType(formState.id, request);
      } else {
        await createMeasurementType(request);
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

  const onDelete = async (row: MeasurementTypeDto) => {
    setError(null);

    try {
      await deleteMeasurementType(row.id);
      await loadRows();
    } catch (deleteError) {
      const message = deleteError instanceof Error ? deleteError.message : t('administration.common.deleteError');
      setError(message);
    }
  };

  const onReactivate = async (row: MeasurementTypeDto) => {
    setError(null);

    try {
      await reactivateMeasurementType(row.id);
      await loadRows();
    } catch (reactivateError) {
      const message = reactivateError instanceof Error
        ? reactivateError.message
        : t('administration.common.reactivateError');
      setError(message);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={1}>
        <Typography variant="h5" fontWeight={700}>{t('administration.measurementTypes.title')}</Typography>
        <Button variant="contained" onClick={openCreate}>{t('common.actions.add')}</Button>
      </Stack>

      <FormControlLabel
        control={<Checkbox checked={includeDeleted} onChange={(_event, checked) => setIncludeDeleted(checked)} />}
        label={t('administration.common.includeDeleted')}
      />

      {error ? (
        <Alert
          severity="error"
          action={<Button color="inherit" size="small" onClick={() => void loadRows()}>{t('administration.common.retry')}</Button>}
        >
          {error}
        </Alert>
      ) : null}

      {!loading && rows.length === 0 ? (
        <Paper sx={{ p: 4 }}><Typography>{t('common.messages.noData')}</Typography></Paper>
      ) : null}

      {!loading && rows.length > 0 ? (
        <Paper>
          <Table aria-label={t('administration.measurementTypes.table')}>
            <TableHead>
              <TableRow>
                <TableCell>{t('administration.measurementTypes.fields.name')}</TableCell>
                <TableCell>{t('administration.measurementTypes.fields.fields')}</TableCell>
                <TableCell align="right">{t('common.actions.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.name}</TableCell>
                  <TableCell>{row.fields.map((field) => field.name).join(', ')}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button size="small" onClick={() => openEdit(row)} disabled={row.isDeleted}>{t('common.actions.edit')}</Button>
                      {row.isDeleted ? (
                        <Button size="small" color="success" onClick={() => void onReactivate(row)}>{t('administration.common.reactivate')}</Button>
                      ) : (
                        <Button size="small" color="error" onClick={() => void onDelete(row)}>{t('common.actions.delete')}</Button>
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
        <DialogTitle>{formState.id ? t('administration.measurementTypes.editTitle') : t('administration.measurementTypes.createTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label={t('administration.measurementTypes.fields.name')}
              value={formState.name}
              onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
              fullWidth
            />
            <TextField
              label={t('administration.measurementTypes.fields.fieldsInput')}
              value={formState.fieldsText}
              onChange={(event) => setFormState((current) => ({ ...current, fieldsText: event.target.value }))}
              helperText={t('administration.measurementTypes.fields.fieldsHelp')}
              fullWidth
            />
            {formError ? <Alert severity="error">{formError}</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeForm} disabled={formLoading}>{t('common.actions.cancel')}</Button>
          <Button onClick={() => void submitForm()} disabled={formLoading} variant="contained">{t('common.actions.save')}</Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
