import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Chip from '@mui/material/Chip';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import { AsyncState } from '../shared/components/AsyncState';
import { PageHeader } from '../shared/components/PageHeader';

export function HomePage() {
  const { t } = useTranslation();

  const pillars = [
    {
      title: t('home.card1Title'),
      description: t('home.card1Description'),
      tag: t('home.card1Tag'),
    },
    {
      title: t('home.card2Title'),
      description: t('home.card2Description'),
      tag: t('home.card2Tag'),
    },
    {
      title: t('home.card3Title'),
      description: t('home.card3Description'),
      tag: t('home.card3Tag'),
    },
  ];

  const flow = [
    t('home.flow1'),
    t('home.flow2'),
    t('home.flow3'),
    t('home.flow4'),
  ];

  return (
    <Stack spacing={3}>
      <Box
        sx={{
          p: { xs: 2.5, md: 4 },
          borderRadius: 3,
          color: '#f3f8f7',
          background: 'linear-gradient(130deg, #0f2f2a 0%, #1f4f46 45%, #3b7f70 100%)',
          boxShadow: '0 20px 48px rgba(13, 40, 35, 0.32)',
        }}
      >
        <Chip
          label={t('home.chip')}
          sx={{
            mb: 1.5,
            bgcolor: '#f8d089',
            color: '#112f29',
            fontWeight: 700,
          }}
        />
        <Typography variant="h3" sx={{ fontWeight: 900, lineHeight: 1.05, mb: 1.25 }}>
          {t('home.titleLine1')}
          <br />
          {t('home.titleLine2')}
        </Typography>
        <Typography sx={{ maxWidth: 760, opacity: 0.9, fontSize: '1.02rem' }}>
          {t('home.summary')}
        </Typography>
      </Box>

      <PageHeader
        title={t('home.sectionTitle')}
        description={t('home.sectionDescription')}
      />

      <Grid container spacing={2}>
        {pillars.map((pillar) => (
          <Grid key={pillar.title} size={{ xs: 12, md: 4 }}>
            <Card sx={{ height: '100%', borderRadius: 2.5 }}>
              <CardContent>
                <Chip label={pillar.tag} size="small" sx={{ mb: 1.5 }} />
                <Typography variant="h6" sx={{ fontWeight: 800, mb: 0.75 }}>
                  {pillar.title}
                </Typography>
                <Typography color="text.secondary">{pillar.description}</Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Card sx={{ borderRadius: 2.5 }}>
        <CardContent>
          <Typography variant="h6" sx={{ fontWeight: 800, mb: 1.25 }}>
            {t('home.flowTitle')}
          </Typography>
          <Stack spacing={1}>
            {flow.map((step, index) => (
              <Stack key={step} direction="row" spacing={1.25} alignItems="flex-start">
                <Chip label={index + 1} size="small" color="secondary" />
                <Typography>{step}</Typography>
              </Stack>
            ))}
          </Stack>
        </CardContent>
      </Card>

      <AsyncState>
        <Typography>
          {t('home.cta')}
        </Typography>
      </AsyncState>
    </Stack>
  );
}

export function CatalogPage() {
  const { t } = useTranslation();

  return (
    <section>
      <PageHeader
        title={t('catalog.title')}
        description={t('catalog.description')}
      />
      <AsyncState>
        <p>{t('catalog.baseline')}</p>
      </AsyncState>
    </section>
  );
}
