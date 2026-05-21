import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Chip from '@mui/material/Chip';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

export function HomePage() {
  const { t } = useTranslation();

  const cards = [
    { tag: t('home.card1Tag'), title: t('home.card1Title'), description: t('home.card1Description') },
    { tag: t('home.card2Tag'), title: t('home.card2Title'), description: t('home.card2Description') },
    { tag: t('home.card3Tag'), title: t('home.card3Title'), description: t('home.card3Description') },
  ];

  const flowSteps = [t('home.flow1'), t('home.flow2'), t('home.flow3'), t('home.flow4')];

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 3, md: 5 }, overflowX: 'hidden' }}>
      {/* Section 1: Hero */}
      <Box sx={{ py: { xs: 4, md: 6 }, textAlign: 'center' }}>
        <Chip
          label={t('home.chip')}
          size="small"
          sx={{
            mb: 2,
            bgcolor: 'secondary.main',
            color: '#ffffff',
            fontWeight: 700,
            borderRadius: 1,
          }}
        />
        <Typography
          variant="h3"
          sx={{ fontWeight: 800, lineHeight: 1.15, mb: 1, fontSize: { xs: '2rem', md: '3rem' } }}
        >
          {t('home.titleLine1')}
        </Typography>
        <Typography
          variant="h3"
          sx={{
            fontWeight: 800,
            lineHeight: 1.15,
            mb: 3,
            fontSize: { xs: '2rem', md: '3rem' },
            color: 'primary.main',
          }}
        >
          {t('home.titleLine2')}
        </Typography>
        <Typography
          variant="body1"
          sx={{ maxWidth: 640, mx: 'auto', opacity: 0.85, fontSize: { xs: '1rem', md: '1.1rem' } }}
        >
          {t('home.summary')}
        </Typography>
      </Box>

      {/* Sections 2 + 3: Cards y Flow — misma fila en md+, apilados en mobile */}
      <Grid container spacing={{ xs: 4, md: 6 }} sx={{ mt: 1 }}>

        {/* Izquierda: Que hace esta app */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
            {t('home.sectionTitle')}
          </Typography>
          <Typography variant="body2" sx={{ mb: 3, opacity: 0.8 }}>
            {t('home.sectionDescription')}
          </Typography>
          <Grid container spacing={2}>
            {cards.map((card) => (
              <Grid key={card.title} size={{ xs: 12, sm: 6, md: 12, lg: 6 }}>
                <Card
                  variant="outlined"
                  sx={{
                    height: '100%',
                    borderRadius: 2,
                    borderColor: 'divider',
                    bgcolor: 'background.paper',
                  }}
                >
                  <CardContent>
                    <Chip
                      label={card.tag}
                      size="small"
                      sx={{
                        mb: 1.5,
                        bgcolor: 'secondary.main',
                        color: '#ffffff',
                        fontWeight: 700,
                        borderRadius: 1,
                        fontSize: '0.7rem',
                      }}
                    />
                    <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
                      {card.title}
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.82 }}>
                      {card.description}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Grid>

        {/* Derecha: Flujo habitual */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Typography variant="h5" sx={{ fontWeight: 700, mb: 3 }}>
            {t('home.flowTitle')}
          </Typography>
          <Stack spacing={2.5}>
            {flowSteps.map((step, index) => (
              <Stack key={index} direction="row" spacing={2} alignItems="flex-start">
                <Box
                  sx={{
                    minWidth: 32,
                    height: 32,
                    borderRadius: '50%',
                    bgcolor: 'primary.main',
                    color: '#ffffff',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontWeight: 700,
                    fontSize: '0.875rem',
                    flexShrink: 0,
                  }}
                >
                  {index + 1}
                </Box>
                <Typography variant="body1" sx={{ pt: 0.4 }}>
                  {step}
                </Typography>
              </Stack>
            ))}
          </Stack>
        </Grid>

      </Grid>

      {/* Section 4: CTA */}
      <Box
        sx={{
          py: { xs: 4, md: 6 },
          textAlign: 'center',
          borderTop: '1px solid',
          borderColor: 'divider',
          mt: 2,
        }}
      >
        <Typography variant="body1" sx={{ opacity: 0.85, maxWidth: 560, mx: 'auto' }}>
          {t('home.cta')}
        </Typography>
      </Box>
    </Container>
  );
}
