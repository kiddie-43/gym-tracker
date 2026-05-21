import { useMemo, useState, type ChangeEvent } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import type { IImportMusclesResult } from '../../../interfaces/muscles/IMuscles';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';

export interface MusclesCsvImportDialogProps {
  open: boolean;
  loading: boolean;
  result: IImportMusclesResult | null;
  error: string | null;
  onClose: () => void;
  onImport: (rows: Array<{ name?: string; code?: string; description?: string }>) => Promise<void>;
}

function parseCsvRows(raw: string): Array<{ name?: string; code?: string; description?: string }> {
  const lines = raw
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length === 0) {
    return [];
  }

  const [header, ...dataLines] = lines;
  const columns = header.split(',').map((item) => item.trim().toLowerCase());
  const nameIndex = columns.indexOf('name');
  const nombreIndex = columns.indexOf('nombre');
  const codeIndex = columns.indexOf('code');
  const descriptionIndex = columns.indexOf('description');

  return dataLines.map((line) => {
    const values = line.split(',').map((item) => item.trim());
    const resolvedNameIndex = nameIndex >= 0 ? nameIndex : nombreIndex;

    return {
      name: resolvedNameIndex >= 0 ? values[resolvedNameIndex] : undefined,
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
  const [csvText, setCsvText] = useState('');
  const [selectedFileName, setSelectedFileName] = useState<string | null>(null);
  const [fileReadError, setFileReadError] = useState<string | null>(null);

  const previewRows = useMemo(() => parseCsvRows(csvText), [csvText]);

  const handleFileSelected = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    try {
      const content = await file.text();
      setCsvText(content);
      setSelectedFileName(file.name);
      setFileReadError(null);
    } catch {
      setCsvText('');
      setSelectedFileName(null);
      setFileReadError(t('administration.muscles.csv.fileReadError'));
    }
  };

  const handleImport = async () => {
    await onImport(previewRows);
  };

  return (
    <PopupDialog
      open={open}
      title={t('administration.muscles.csv.title')}
      onClose={onClose}
      onSubmit={() => {
        void handleImport();
      }}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('administration.muscles.csv.importAction')}
      disableSave={loading || !selectedFileName || previewRows.length === 0}
      isSaving={loading}
      maxWidth="md"
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Typography variant="body2" color="text.secondary">
          {t('administration.muscles.csv.hint')}
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }}>
          <Button variant="outlined" component="label">
            {t('administration.muscles.csv.selectFileAction')}
            <input hidden type="file" accept=".csv,text/csv" onChange={(event) => void handleFileSelected(event)} />
          </Button>
          <Typography variant="body2" color="text.secondary">
            {selectedFileName ?? t('administration.muscles.csv.noFileSelected')}
          </Typography>
        </Stack>
        {fileReadError ? <Alert severity="error">{fileReadError}</Alert> : null}
        {!fileReadError && selectedFileName ? (
          <Typography variant="body2" color="text.secondary">
            {t('administration.muscles.csv.previewRows', { count: previewRows.length })}
          </Typography>
        ) : null}
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
          <TableContainer sx={{ maxHeight: 240 }}>
            <Table stickyHeader size="small" aria-label={t('administration.muscles.csv.resultsTable')}>
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
          </TableContainer>
        ) : null}
      </Stack>
    </PopupDialog>
  );
}
