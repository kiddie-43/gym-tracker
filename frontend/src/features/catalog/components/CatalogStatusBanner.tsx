import Alert from '@mui/material/Alert';

import type { CatalogAvailability } from '../../../shared/types/catalog';

type CatalogStatusBannerProps = {
  availability: CatalogAvailability;
};

export function CatalogStatusBanner({ availability }: CatalogStatusBannerProps) {
  if (!availability.isStale) {
    return null;
  }

  return (
    <Alert severity="warning">
      El catalogo esta funcionando en modo degradado usando datos en cache.
    </Alert>
  );
}
