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

import type {
  ImportExerciseCsvRowRequest,
  ImportExercisesResult,
} from '../../../interfaces/admin/exercises/exercises';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';

export interface ExercisesCsvImportDialogProps {
  open: boolean;
  loading: boolean;
  result: ImportExercisesResult | null;
  error: string | null;
  onClose: () => void;
  onImport: (rows: ImportExerciseCsvRowRequest[]) => Promise<void>;
}

function parseCsvRows(raw: string): ImportExerciseCsvRowRequest[] {
  const lines = raw
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length === 0) {
    return [];
  }

  const [header, ...dataLines] = lines;
  const columns = header.split(',').map((item) => item.trim().toLowerCase());
  const idx = (name: string) => columns.indexOf(name);

  return dataLines.map((line) => {
    const values = line.split(',').map((item) => item.trim());
    const get = (i: number) => (i >= 0 ? (values[i] ?? undefined) : undefined);
    const getArr = (i: number): string[] =>
      i >= 0 && values[i] ? values[i].split('|').map((s) => s.trim()).filter(Boolean) : [];

    return {
      code: get(idx('code')),
      name: get(idx('name')),
      description: get(idx('description')) ?? null,
      category: get(idx('category')),
      difficulty: get(idx('difficulty')),
      measurementTypeId: get(idx('measurementtypeid')),
      primaryMuscleIds: getArr(idx('primarymuscleids')),
      secondaryMuscleIds: getArr(idx('secondarymuscleids')),
    };
  });
}

export function ExercisesCsvImportDialog({
  open,
  loading,
  result,
  error,
  onClose,
  onImport,
}: ExercisesCsvImportDialogProps) {
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
      setFileReadError(t('administration.exercises.csv.fileReadError'));
    }
  };

  return (
    <PopupDialog
      open={open}
      title={t('administration.exercises.csv.title')}
      onClose={onClose}
      onSubmit={() => {
        void onImport(previewRows);
      }}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('administration.exercises.csv.importAction')}
      disableSave={loading || !selectedFileName || previewRows.length === 0}
      isSaving={loading}
      maxWidth="md"
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Typography variant="body2" color="text.secondary">
          {t('administration.exercises.csv.hint')}
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }}>
          <Button variant="outlined" component="label">
            {t('administration.exercises.csv.selectFileAction')}
            <input hidden type="file" accept=".csv,text/csv" onChange={(event) => void handleFileSelected(event)} />
          </Button>
          <Typography variant="body2" color="text.secondary">
            {selectedFileName ?? t('administration.exercises.csv.noFileSelected')}
          </Typography>
        </Stack>
        {fileReadError ? <Alert severity="error">{fileReadError}</Alert> : null}
        {!fileReadError && selectedFileName ? (
          <Typography variant="body2" color="text.secondary">
            {t('administration.exercises.csv.previewRows', { count: previewRows.length })}
          </Typography>
        ) : null}
        {error ? <Alert severity="error">{error}</Alert> : null}
        {result ? (
          <Alert severity="info">
            {t('administration.exercises.csv.summary', {
              total: result.totalRows,
              created: result.createdRows,
              rejected: result.rejectedRows,
            })}
          </Alert>
        ) : null}
        {result ? (
          <TableContainer sx={{ maxHeight: 240 }}>
            <Table stickyHeader size="small" aria-label={t('administration.exercises.csv.resultsTable')}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('administration.exercises.csv.columns.row')}</TableCell>
                  <TableCell>{t('common.fields.code')}</TableCell>
                  <TableCell>{t('administration.exercises.csv.columns.status')}</TableCell>
                  <TableCell>{t('administration.exercises.csv.columns.reason')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {result.rows.map((row) => (
                  <TableRow key={`${row.rowNumber}-${row.code ?? 'empty'}`}>
                    <TableCell>{row.rowNumber}</TableCell>
                    <TableCell>{row.code ?? '-'}</TableCell>
                    <TableCell>
                      {row.created
                        ? t('administration.exercises.csv.status.created')
                        : t('administration.exercises.csv.status.rejected')}
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
