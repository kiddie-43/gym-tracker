import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded';
import InboxRoundedIcon from '@mui/icons-material/InboxRounded';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface FeedbackMessageProps {
  type: 'error' | 'empty';
  message: string;
  variant?: 'default' | 'spotlight';
  title?: string;
  icon?: ReactNode;
  fullHeight?: boolean;
}

export function FeedbackMessageSpotlight({
  type,
  message,
  title,
  icon,
  fullHeight = false,
}: FeedbackMessageProps) {
  return (
    <Box
      sx={{
        width: '100%',
        height: fullHeight ? '100%' : 'auto',
        minHeight: fullHeight ? '100%' : '42vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        px: 2,
      }}
    >
      <Box sx={{ maxWidth: 560, width: '100%' }}>
        <Box
          sx={(theme) => ({
            width: 104,
            height: 104,
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            mx: 'auto',
            mb: 2,
            color: type === 'error' ? 'error.light' : 'warning.light',
            border: '1px solid',
            borderColor: 'grey.800',
            background: `radial-gradient(circle at 30% 25%, ${theme.palette.grey[800]} 0%, ${theme.palette.background.default} 75%)`,
            boxShadow: '0 18px 38px rgba(0,0,0,0.35)',
          })}
        >
          {icon ?? (type === 'error' ? <ErrorOutlineRoundedIcon sx={{ fontSize: 44 }} /> : <InboxRoundedIcon sx={{ fontSize: 44 }} />)}
        </Box>
        <Typography variant="h4" sx={{ fontSize: { xs: '1.6rem', md: '2rem' }, mb: 1 }}>
          {title ?? (type === 'error' ? 'Ha ocurrido un error' : 'No hay datos')}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {message}
        </Typography>
      </Box>
    </Box>
  );
}

export function FeedbackMessage({
  type,
  message,
  variant = 'default',
  title,
  icon,
  fullHeight = false,
}: FeedbackMessageProps) {
  if (variant === 'spotlight') {
    return <FeedbackMessageSpotlight type={type} message={message} title={title} icon={icon} fullHeight={fullHeight} />;
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 1,
        py: 4,
        px: 2,
        color: type === 'error' ? 'error.main' : 'text.secondary',
      }}
    >
      {type === 'error' ? (
        <ErrorOutlineRoundedIcon sx={{ fontSize: 40 }} />
      ) : (
        <InboxRoundedIcon sx={{ fontSize: 40 }} />
      )}
      <Typography variant="body2" textAlign="center">
        {message}
      </Typography>
    </Box>
  );
}
