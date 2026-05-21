import Alert from '@mui/material/Alert';

interface CatalogDegradedModeAlertProps {
  visible: boolean;
}

export function CatalogDegradedModeAlert({ visible }: CatalogDegradedModeAlertProps) {
  if (!visible) {
    return null;
  }

  return (
    <Alert severity="warning" variant="outlined">
      El catalogo externo esta en modo degradado. Se muestran datos en cache y podrian no estar actualizados.
    </Alert>
  );
}
