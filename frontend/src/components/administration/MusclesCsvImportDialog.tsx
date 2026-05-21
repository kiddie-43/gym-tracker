import { useMemo, useState } from 'react';

import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { ImportMusclesResult } from '../../interfaces/admin/muscles/muscles';

export interface MusclesCsvImportDialogProps {
  open: boolean;
  loading: boolean;
  result: ImportMusclesResult | null;
  error: string | null;
  onClose: () => void;
  onImport: (rows: Array<{ code?: string; description?: string }>) => Promise<void>;
}

function parseCsvRows(raw: string): Array<{ code?: string; description?: string }> {
  const lines = raw
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length === 0) {
    return [];
  }

  const [header, ...dataLines] = lines;
  const columns = header.split(',').map((item) => item.trim().toLowerCase());
  const codeIndex = columns.indexOf('code');
  const descriptionIndex = columns.indexOf('description');

  return dataLines.map((line) => {
    const values = line.split(',').map((item) => item.trim());
    return {
      code: codeIndex >= 0 ? values[codeIndex] : undefined,
      description: descriptionIndex >= 0 ? values[descriptionIndex] : undefined,
    };
  });
}

export function MusclesCsvImportDialog({
  open,
  loading,
  result,
  error,
  onClose,
  onImport,
}: MusclesCsvImportDialogProps) {
  const { t } = useTranslation();
  const [csvText, setCsvText] = useState('code,description');

  const previewRows = useMemo(() => parseCsvRows(csvText), [csvText]);

  const handleImport = async () => {
    await onImport(previewRows);
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>{t('administration.muscles.csv.title')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Typography variant="body2" color="text.secondary">
            {t('administration.muscles.csv.hint')}
          </Typography>
          <TextField
            label={t('administration.muscles.csv.inputLabel')}
            value={csvText}
            onChange={(event) => setCsvText(event.target.value)}
            multiline
            minRows={8}
            fullWidth
          />
          {error ? <Alert severity="error">{error}</Alert> : null}
          {result ? (
            <Alert severity="info">
              {t('administration.muscles.csv.summary', {
                total: result.totalRows,
                imported: result.importedRows,
                rejected: result.rejectedRows,
              })}
            </Alert>
          ) : null}
          {result ? (
            <Box sx={{ maxHeight: 240, overflow: 'auto' }}>
              <Table size="small" aria-label={t('administration.muscles.csv.resultsTable')}>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('administration.muscles.csv.columns.row')}</TableCell>
                    <TableCell>{t('common.fields.code')}</TableCell>
                    <TableCell>{t('administration.muscles.csv.columns.status')}</TableCell>
                    <TableCell>{t('administration.muscles.csv.columns.reason')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {result.results.map((row) => (
                    <TableRow key={`${row.rowNumber}-${row.code ?? 'empty'}`}>
                      <TableCell>{row.rowNumber}</TableCell>
                      <TableCell>{row.code ?? '-'}</TableCell>
                      <TableCell>
                        {row.imported
                          ? t('administration.muscles.csv.status.imported')
                          : t('administration.muscles.csv.status.rejected')}
                      </TableCell>
                      <TableCell>{row.reason ?? '-'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          ) : null}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('common.actions.cancel')}</Button>
        <Button onClick={handleImport} variant="contained" disabled={loading}>
          {t('administration.muscles.csv.importAction')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
