import type { ReactNode } from 'react';

import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import type { Breakpoint } from '@mui/material/styles';

type FormPopupDialogProps = {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  formId?: string;
  saveLabel?: string;
  closeLabel?: string;
  disableSave?: boolean;
  isSaving?: boolean;
  maxWidth?: Breakpoint;
};

export function FormPopupDialog({
  open,
  title,
  onClose,
  children,
  formId,
  saveLabel = 'Guardar',
  closeLabel = 'Cerrar',
  disableSave = false,
  isSaving = false,
  maxWidth = 'sm',
}: FormPopupDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth={maxWidth}>
      <DialogTitle
        sx={{
          pb: 1,
          borderBottom: '1px solid',
          borderColor: 'divider',
          bgcolor: 'primary.main',
          color: 'primary.contrastText',
        }}
      >
        <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={1}>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            {title}
          </Typography>
          <IconButton size="small" onClick={onClose} aria-label="Cerrar popup" sx={{ color: 'inherit' }}>
            <CloseRoundedIcon fontSize="small" />
          </IconButton>
        </Stack>
      </DialogTitle>

      <DialogContent
        sx={{
          pt: '24px !important',
        }}
      >
        {children}
      </DialogContent>

      <DialogActions
        sx={{
          justifyContent: 'flex-end',
          px: 3,
          pb: 2,
          pt: 1.5,
          borderTop: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
        }}
      >
        <Button size="small" variant="outlined" onClick={onClose}>
          {closeLabel}
        </Button>
        <Button
          size="small"
          variant="contained"
          type={formId ? 'submit' : 'button'}
          form={formId}
          disabled={disableSave || isSaving}
        >
          {saveLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}