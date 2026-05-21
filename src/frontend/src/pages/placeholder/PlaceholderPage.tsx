import Chip from '@mui/material/Chip';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

interface PlaceholderPageProps {
  labelKey: string;
}

export function PlaceholderPage({ labelKey }: PlaceholderPageProps) {
  const { t } = useTranslation();

  return (
    <Container maxWidth="sm" sx={{ textAlign: 'center', py: { xs: 6, md: 10 } }}>
      <Stack spacing={3} alignItems="center">
        <Typography variant="h4" fontWeight={700}>
          {t('placeholder.title', { module: t(labelKey) })}
        </Typography>
        <Chip label={t('placeholder.status')} color="warning" />
        <Typography variant="body1" color="text.secondary">
          {t('placeholder.message')}
        </Typography>
      </Stack>
    </Container>
  );
}
