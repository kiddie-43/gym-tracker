import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded';
import InboxRoundedIcon from '@mui/icons-material/InboxRounded';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

interface FeedbackMessageProps {
  type: 'error' | 'empty';
  message: string;
}

export function FeedbackMessage({ type, message }: FeedbackMessageProps) {
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
