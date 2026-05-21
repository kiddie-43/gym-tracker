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
  ImportMeasurementTypeCsvRowRequest,
  ImportMeasurementTypesResult,
} from '../../../interfaces/admin/measurementTypes/measurementTypes';
import { PopupDialog } from '../../../components/PopupDialog/PopupDialog';

export interface MeasurementTypesCsvImportDialogProps {
  open: boolean;
  loading: boolean;
  result: ImportMeasurementTypesResult | null;
  error: string | null;
  onClose: () => void;
  onImport: (rows: ImportMeasurementTypeCsvRowRequest[]) => Promise<void>;
}

function parseCsvRows(raw: string): ImportMeasurementTypeCsvRowRequest[] {
  const lines = raw
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length === 0) {
    return [];
  }

  const [header, ...dataLines] = lines;
  const normalizedHeader = header.replace(/^\uFEFF/, '');
  const delimiter = normalizedHeader.includes(';') && !normalizedHeader.includes(',') ? ';' : ',';
  const columns = normalizedHeader.split(delimiter).map((item) => item.trim().toLowerCase());

  const keyIndex = columns.indexOf('key');
  const nameIndex = columns.indexOf('name');
  const nombreIndex = columns.indexOf('nombre');
  const unitIndex = columns.indexOf('unit');
  const unidadIndex = columns.indexOf('unidad');
  const dataTypeIndex = columns.indexOf('datatype');
  const tipoDatoIndex = columns.indexOf('tipodato');
  const categoryIndex = columns.indexOf('category');
  const categoriaIndex = columns.indexOf('categoria');
  const descriptionIndex = columns.indexOf('description');
  const descripcionIndex = columns.indexOf('descripcion');

  return dataLines.map((line) => {
    const values = line.split(delimiter).map((item) => item.trim());

    return {
      key: keyIndex >= 0 ? values[keyIndex] ?? '' : '',
      name: nameIndex >= 0 ? values[nameIndex] ?? '' : nombreIndex >= 0 ? values[nombreIndex] ?? '' : '',
      unit: unitIndex >= 0 ? values[unitIndex] ?? '' : unidadIndex >= 0 ? values[unidadIndex] ?? '' : '',
      dataType:
        dataTypeIndex >= 0 ? values[dataTypeIndex] ?? '' : tipoDatoIndex >= 0 ? values[tipoDatoIndex] ?? '' : '',
      category:
        categoryIndex >= 0 ? values[categoryIndex] ?? '' : categoriaIndex >= 0 ? values[categoriaIndex] ?? '' : '',
      description:
        descriptionIndex >= 0
          ? values[descriptionIndex] || null
          : descripcionIndex >= 0
            ? values[descripcionIndex] || null
            : null,
    };
  });
}

export function MeasurementTypesCsvImportDialog({
  open,
  loading,
  result,
  error,
  onClose,
  onImport,
}: MeasurementTypesCsvImportDialogProps) {
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
      setFileReadError(t('administration.measurementTypes.csv.fileReadError'));
    }
  };

  const handleImport = async () => {
    await onImport(previewRows);
  };

  return (
    <PopupDialog
      open={open}
      title={t('administration.measurementTypes.csv.title')}
      onClose={onClose}
      onSubmit={() => {
        void handleImport();
      }}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('administration.measurementTypes.csv.importAction')}
      disableSave={loading || !selectedFileName || previewRows.length === 0}
      isSaving={loading}
      maxWidth="md"
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Typography variant="body2" color="text.secondary">
          {t('administration.measurementTypes.csv.hint')}
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }}>
          <Button variant="outlined" component="label">
            {t('administration.measurementTypes.csv.selectFileAction')}
            <input hidden type="file" accept=".csv,text/csv" onChange={(event) => void handleFileSelected(event)} />
          </Button>
          <Typography variant="body2" color="text.secondary">
            {selectedFileName ?? t('administration.measurementTypes.csv.noFileSelected')}
          </Typography>
        </Stack>
        {fileReadError ? <Alert severity="error">{fileReadError}</Alert> : null}
        {!fileReadError && selectedFileName ? (
          <Typography variant="body2" color="text.secondary">
            {t('administration.measurementTypes.csv.previewRows', { count: previewRows.length })}
          </Typography>
        ) : null}
        {error ? <Alert severity="error">{error}</Alert> : null}
        {result ? (
          <Alert severity="info">
            {t('administration.measurementTypes.csv.summary', {
              total: result.totalRows,
              created: result.createdRows,
              rejected: result.rejectedRows,
            })}
          </Alert>
        ) : null}
        {result ? (
          <TableContainer sx={{ maxHeight: 240 }}>
            <Table stickyHeader size="small" aria-label={t('administration.measurementTypes.csv.resultsTable')}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('administration.measurementTypes.csv.columns.row')}</TableCell>
                  <TableCell>{t('administration.measurementTypes.csv.columns.key')}</TableCell>
                  <TableCell>{t('administration.measurementTypes.csv.columns.status')}</TableCell>
                  <TableCell>{t('administration.measurementTypes.csv.columns.reason')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {result.rows.map((row) => (
                  <TableRow key={`${row.rowNumber}-${row.key ?? 'empty'}`}>
                    <TableCell>{row.rowNumber}</TableCell>
                    <TableCell>{row.key ?? '-'}</TableCell>
                    <TableCell>
                      {row.created
                        ? t('administration.measurementTypes.csv.status.created')
                        : t('administration.measurementTypes.csv.status.rejected')}
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
