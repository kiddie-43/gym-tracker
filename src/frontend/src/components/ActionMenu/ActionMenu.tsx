import type { ReactNode, Ref } from 'react';
import { useMemo, useState } from 'react';

import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';
import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { alpha, useTheme, type SxProps, type Theme } from '@mui/material/styles';
import { useTranslation } from 'react-i18next';

export interface ActionMenuItem {
  id: string;
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  disabled?: boolean;
  hidden?: boolean;
  keepOpenOnClick?: boolean;
}

interface ActionMenuProps {
  actions: ActionMenuItem[];
  ariaLabel?: string;
  disabled?: boolean;
  iconSize?: 'small' | 'medium' | 'large';
  triggerButtonRef?: Ref<HTMLButtonElement>;
  triggerSx?: SxProps<Theme>;
}

export function ActionMenu({
  actions,
  ariaLabel,
  disabled = false,
  iconSize = 'small',
  triggerButtonRef,
  triggerSx,
}: ActionMenuProps) {
  const theme = useTheme();
  const { t } = useTranslation();
  const isCompactViewport = useMediaQuery(theme.breakpoints.down('md'));
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [isSheetOpen, setIsSheetOpen] = useState(false);
  const baseButtonSize = iconSize === 'large' ? 44 : iconSize === 'medium' ? 40 : 36;
  const baseIconSize = iconSize === 'large' ? 24 : iconSize === 'medium' ? 22 : 20;

  const visibleActions = useMemo(
    () => actions.filter((action) => !action.hidden),
    [actions],
  );

  const isMenuDisabled = disabled || visibleActions.length === 0;

  const closeDesktopMenu = () => setAnchorEl(null);
  const closeMobileSheet = () => setIsSheetOpen(false);

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    if (isMenuDisabled) {
      return;
    }

    if (isCompactViewport) {
      setIsSheetOpen(true);
      return;
    }

    setAnchorEl(event.currentTarget);
  };

  const handleActionClick = (action: ActionMenuItem) => {
    if (!action.keepOpenOnClick) {
      closeDesktopMenu();
      closeMobileSheet();
    }

    action.onClick();
  };

  return (
    <>
      <IconButton
        ref={triggerButtonRef}
        size={iconSize}
        aria-label={ariaLabel ?? t('common.actionsMenu')}
        onClick={handleOpen}
        disabled={isMenuDisabled}
        sx={[
          {
            width: baseButtonSize,
            height: baseButtonSize,
            color: 'text.primary',
            bgcolor: 'action.hover',
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: '0 4px 14px rgba(0,0,0,0.16)',
            '&:hover': {
              bgcolor: 'action.selected',
            },
          },
          ...(Array.isArray(triggerSx) ? triggerSx : triggerSx ? [triggerSx] : []),
        ]}
      >
        <MoreVertRoundedIcon sx={{ fontSize: baseIconSize }} />
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl) && !isCompactViewport}
        onClose={closeDesktopMenu}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        PaperProps={{
          sx: {
            mt: 0.5,
            minWidth: { xs: 188, sm: 210, md: 220 },
            maxWidth: 'calc(100vw - 24px)',
            borderRadius: 2,
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: '0 18px 38px rgba(0,0,0,0.28)',
            backdropFilter: 'blur(8px)',
            overflow: 'hidden',
          },
        }}
      >
        {visibleActions.map((action) => (
          <MenuItem
            key={action.id}
            disabled={action.disabled}
            onClick={() => handleActionClick(action)}
            sx={{
              py: 1.3,
              px: 1.75,
              gap: 0.5,
              '& .MuiListItemText-primary': {
                fontWeight: 600,
                letterSpacing: 0.2,
                fontSize: '0.98rem',
              },
            }}
          >
            {action.icon ? (
              <ListItemIcon sx={{ minWidth: 30, mr: 0.5, color: 'primary.main' }}>{action.icon}</ListItemIcon>
            ) : null}
            <ListItemText>{action.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>

      <Drawer
        anchor="bottom"
        open={isSheetOpen && isCompactViewport}
        onClose={closeMobileSheet}
        PaperProps={{
          sx: {
            borderTopLeftRadius: 16,
            borderTopRightRadius: 16,
            pb: 2,
          },
        }}
      >
        <Box
          sx={{
            px: 2,
            py: 1,
            borderBottom: '1px solid',
            borderColor: 'divider',
            bgcolor: 'primary.main',
            color: 'primary.contrastText',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
            <Typography variant="h6" sx={{ fontWeight: 700 }}>
              {t('common.actions.actions')}
            </Typography>
            <IconButton
              size="medium"
              onClick={closeMobileSheet}
              aria-label={t('common.actions.cancel')}
              sx={{
                color: 'inherit',
                width: 40,
                height: 40,
              }}
            >
              <CloseRoundedIcon sx={{ fontSize: 24 }} />
            </IconButton>
          </Box>
        </Box>

        <List sx={{ pt: 0 }}>
          {visibleActions.map((action) => (
            <ListItemButton
              key={action.id}
              disabled={action.disabled}
              onClick={() => handleActionClick(action)}
              sx={{
                py: 1.45,
                px: 2,
                '& .MuiListItemText-primary': {
                  fontWeight: 600,
                  fontSize: '1rem',
                },
                '&:hover': {
                  bgcolor: alpha(theme.palette.primary.main, 0.08),
                },
              }}
            >
              {action.icon ? (
                <ListItemIcon sx={{ minWidth: 30, mr: 0.5, color: 'primary.main' }}>{action.icon}</ListItemIcon>
              ) : null}
              <ListItemText primary={action.label} />
            </ListItemButton>
          ))}
        </List>

      </Drawer>
    </>
  );
}
