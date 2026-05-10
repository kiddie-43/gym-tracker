import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

export function AppFooter() {
  const year = new Date().getFullYear();
  const { t } = useTranslation();

  return (
    <Box
      component="footer"
      sx={{
        mt: 'auto',
        borderTop: '1px solid',
        borderColor: 'divider',
        background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
        color: '#f4f8f7',
      }}
    >
      <Container
        maxWidth="lg"
        sx={{
          py: 1.5,
        }}
      >
        <Stack
          direction="column"
          spacing={0.5}
          justifyContent="center"
          alignItems="center"
        >
          <Typography variant="body2" sx={{ fontWeight: 700, letterSpacing: 0.2, textAlign: 'center' }}>
            {t('app.name')}
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.88, textAlign: 'center' }}>
            {t('footer.description', { year })}
          </Typography>
        </Stack>
      </Container>
    </Box>
  );
}