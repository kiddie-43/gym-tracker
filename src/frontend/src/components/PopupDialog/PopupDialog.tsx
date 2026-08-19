import { forwardRef, type ReactElement, type ReactNode, type Ref } from 'react';

import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import IconButton from '@mui/material/IconButton';
import Slide from '@mui/material/Slide';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme, type Breakpoint } from '@mui/material/styles';
import type { TransitionProps } from '@mui/material/transitions';

type PopupDialogProps = {
  open: boolean;
  title: string;
  onClose: () => void;
  onSubmit?: () => void;
  children: ReactNode;
  formId?: string;
  saveLabel?: string;
  closeLabel?: string;
  disableSave?: boolean;
  isSaving?: boolean;
  maxWidth?: Breakpoint;
};

const MobileTransition = forwardRef(function MobileTransition(
  props: TransitionProps & { children: ReactElement },
  ref: Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

export function PopupDialog({
  open,
  title,
  onClose,
  onSubmit,
  children,
  formId,
  saveLabel = 'Guardar',
  closeLabel = 'Cerrar',
  disableSave = false,
  isSaving = false,
  maxWidth = 'sm',
}: PopupDialogProps) {
  const theme = useTheme();
  const isCompactViewport = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth={isCompactViewport ? false : maxWidth}
      TransitionComponent={isCompactViewport ? MobileTransition : undefined}
      sx={
        isCompactViewport
          ? {
            '& .MuiDialog-container': {
              alignItems: 'flex-end',
            },
            '& .MuiDialog-paper': {
              m: 0,
              width: '100%',
              maxWidth: '100%',
              minHeight: '50dvh',
              maxHeight: '90dvh',
              borderTopLeftRadius: 20,
              borderTopRightRadius: 20,
              borderBottomLeftRadius: 0,
              borderBottomRightRadius: 0,
              overflow: 'hidden',
            },
          }
          : undefined
      }
    >
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
          <IconButton
            size={isCompactViewport ? 'medium' : 'small'}
            onClick={onClose}
            aria-label={closeLabel}
            sx={{
              color: 'inherit',
              width: isCompactViewport ? 40 : 32,
              height: isCompactViewport ? 40 : 32,
            }}
          >
            <CloseRoundedIcon sx={{ fontSize: isCompactViewport ? 24 : 18 }} />
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
          gap: isCompactViewport ? 1.25 : 1,
          px: 3,
          pb: 2,
          pt: 1.5,
          borderTop: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
        }}
      >
        <Button
          size={isCompactViewport ? 'medium' : 'small'}
          variant="outlined"
          onClick={onClose}
          sx={{
            minWidth: isCompactViewport ? 108 : 80,
            fontSize: isCompactViewport ? '0.95rem' : undefined,
          }}
        >
          {closeLabel}
        </Button>
        <Button
          size={isCompactViewport ? 'medium' : 'small'}
          variant="contained"
          type={formId ? 'submit' : 'button'}
          form={formId}
          onClick={formId ? undefined : onSubmit}
          disabled={disableSave || isSaving}
          sx={{
            minWidth: isCompactViewport ? 108 : 80,
            fontSize: isCompactViewport ? '0.95rem' : undefined,
          }}
        >
          {saveLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
