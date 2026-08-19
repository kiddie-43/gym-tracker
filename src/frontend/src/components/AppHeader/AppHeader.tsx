import { type MouseEvent, useMemo, useState } from 'react';

import AppBar from '@mui/material/AppBar';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import Chip from '@mui/material/Chip';
import Divider from '@mui/material/Divider';
import Drawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded';
import ArrowBackRoundedIcon from '@mui/icons-material/ArrowBackRounded';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import MenuIcon from '@mui/icons-material/Menu';
import PersonRoundedIcon from '@mui/icons-material/PersonRounded';
import SportsGymnasticsRoundedIcon from '@mui/icons-material/SportsGymnasticsRounded';
import type { SelectChangeEvent } from '@mui/material/Select';
import type { SvgIconProps } from '@mui/material/SvgIcon';
import { useTranslation } from 'react-i18next';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';

import { useSelector } from 'react-redux';
import { useDispatch } from 'react-redux';

import { selectPreferencesState } from '../../redux/states/preferences/preferencesState';
import { requestPreferencesHeaderBack } from '../../redux/actions/preferences/preferencesActions';

interface NavigationLink {
  to: string;
  labelKey: string;
  icon: React.ComponentType<SvgIconProps>;
}

const links: NavigationLink[] = [
  { to: '/administration', labelKey: 'nav.administration', icon: AdminPanelSettingsRoundedIcon },
  { to: '/routines', labelKey: 'nav.routines', icon: SportsGymnasticsRoundedIcon },
];

export function AppHeader() {
  const dispatch = useDispatch();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [profileAnchor, setProfileAnchor] = useState<null | HTMLElement>(null);
  const location = useLocation();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { profile, headerTitle, headerShowBackButton } = useSelector(selectPreferencesState);

  const displayFirstName = profile.firstName || t('user.defaultName');
  const displayLastName = profile.lastName || t('user.defaultLastName');
  const userInitial = displayFirstName.trim().charAt(0).toUpperCase() || 'U';

  const handleLanguageChange = (event: SelectChangeEvent) => {
    void i18n.changeLanguage(event.target.value);
  };

  const openProfileMenu = (event: MouseEvent<HTMLElement>) => {
    setProfileAnchor(event.currentTarget);
  };

  const closeProfileMenu = () => {
    setProfileAnchor(null);
  };

  const goToProfile = () => {
    closeProfileMenu();
    void navigate('/profile');
  };

  const handleLogout = () => {
    closeProfileMenu();
    // TODO: integrar con auth
  };

  const activeLink = links.find((link) => {
    if (link.to === '/') return location.pathname === '/';
    return location.pathname.startsWith(link.to);
  });
  const routeTitle = activeLink ? t(activeLink.labelKey) : '';
  const effectiveHeaderTitle = useMemo(() => {
    const customTitle = headerTitle.trim();
    if (customTitle) {
      return customTitle;
    }

    return routeTitle || t('app.name');
  }, [headerTitle, routeTitle, t]);

  return (
    <>
      <AppBar
        position="static"
        elevation={0}
        sx={{
          borderBottom: '1px solid',
          borderColor: 'divider',
          background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
        }}
      >
        <Toolbar disableGutters sx={{ minHeight: 82, py: 1.25, pl: 1.25, pr: 1.5 }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" width="100%" gap={2}>
            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ minWidth: 0, flex: 1 }}>
              <IconButton
                aria-label={headerShowBackButton ? 'Volver atras' : t('header.menuLabel')}
                onClick={() => {
                  if (headerShowBackButton) {
                    dispatch(requestPreferencesHeaderBack());
                    return;
                  }

                  setIsMenuOpen(true);
                }}
                sx={{
                  color: '#ffffff',
                  border: '1px solid rgba(255,255,255,0.24)',
                  borderRadius: 1.5,
                }}
              >
                {headerShowBackButton ? <ArrowBackRoundedIcon /> : <MenuIcon />}
              </IconButton>

              <Box sx={{ minWidth: 0, flex: 1 }}>
                <Typography
                  variant="h4"
                  noWrap
                  sx={{
                    fontWeight: 800,
                    letterSpacing: 0.2,
                    fontSize: { xs: '1.55rem', md: '2rem' },
                    lineHeight: 1.1,
                  }}
                >
                  {effectiveHeaderTitle}
                </Typography>
              </Box>
            </Stack>

            <Stack direction="row" spacing={1} alignItems="center">
              <Chip
                label={
                  activeLink
                    ? t('header.open', { section: t(activeLink.labelKey) })
                    : t('header.livePlan')
                }
                size="small"
                sx={{
                  display: { xs: 'none', md: 'inline-flex' },
                  color: '#0f2f2a',
                  bgcolor: '#f8d089',
                  fontWeight: 700,
                  borderRadius: 1,
                  height: 24,
                }}
              />

              <Select
                value={i18n.resolvedLanguage ?? 'es'}
                onChange={handleLanguageChange}
                size="small"
                variant="outlined"
                aria-label={t('header.languageLabel')}
                MenuProps={{
                  PaperProps: {
                    sx: {
                      mt: 0.75,
                      borderRadius: 1.5,
                      border: '1px solid',
                      borderColor: 'rgba(255,255,255,0.14)',
                      bgcolor: 'primary.dark',
                      color: 'primary.contrastText',
                      '& .MuiMenuItem-root.Mui-selected': {
                        bgcolor: 'rgba(255,255,255,0.12)',
                      },
                    },
                  },
                }}
                sx={{
                  minWidth: 64,
                  height: 36,
                  px: 0.25,
                  borderRadius: 1.5,
                  bgcolor: 'primary.dark',
                  color: '#f4f8f7',
                  '.MuiSelect-select': {
                    py: 0.6,
                    pl: 1,
                    pr: '28px !important',
                    fontWeight: 700,
                    letterSpacing: 0.5,
                  },
                  '.MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255,255,255,0.2)',
                  },
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255,255,255,0.42)',
                  },
                  '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'secondary.main',
                  },
                  '.MuiSvgIcon-root': {
                    color: '#f4f8f7',
                  },
                }}
              >
                <MenuItem value="es">ES</MenuItem>
                <MenuItem value="en">EN</MenuItem>
              </Select>

              <ButtonBase
                onClick={openProfileMenu}
                aria-label={t('header.viewEditProfile')}
                sx={{
                  borderRadius: '999px',
                  border: '1px solid rgba(255,255,255,0.3)',
                  color: '#f4f8f7',
                }}
              >
                <Avatar src={profile.photoUrl || undefined} sx={{ width: 34, height: 34, fontSize: 15 }}>
                  {userInitial}
                </Avatar>
              </ButtonBase>
            </Stack>
          </Stack>
        </Toolbar>
      </AppBar>

      {/* Profile Menu */}
      <Menu
        anchorEl={profileAnchor}
        open={Boolean(profileAnchor)}
        onClose={closeProfileMenu}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        PaperProps={{
          sx: {
            width: 360,
            borderRadius: 2,
            border: '1px solid',
            borderColor: 'divider',
            background: (theme) =>
              `linear-gradient(180deg, ${theme.palette.primary.dark} 0%, ${theme.palette.primary.main} 100%)`,
            color: 'primary.contrastText',
            mt: 1,
            overflow: 'hidden',
          },
        }}
      >
        <Box sx={{ px: 2.25, pt: 2, pb: 1.5 }}>
          <Stack direction="row" spacing={1.5} alignItems="center">
            <Avatar src={profile.photoUrl || undefined} sx={{ width: 74, height: 74, fontSize: 28 }}>
              {userInitial}
            </Avatar>
            <Box>
              <Typography sx={{ color: 'secondary.main', fontWeight: 700 }}>{displayFirstName}</Typography>
              <Typography sx={{ color: 'rgba(255,255,255,0.9)', fontWeight: 600, mt: 0.35 }}>
                {displayLastName}
              </Typography>
            </Box>
          </Stack>
        </Box>

        <Divider sx={{ borderColor: 'rgba(255,255,255,0.18)' }} />

        <MenuItem onClick={goToProfile} sx={{ py: 1.5 }}>
          <ListItemIcon>
            <PersonRoundedIcon sx={{ color: 'secondary.main' }} />
          </ListItemIcon>
          <ListItemText primary={t('header.viewEditProfile')} primaryTypographyProps={{ fontWeight: 700 }} />
        </MenuItem>

        <Divider sx={{ borderColor: 'rgba(255,255,255,0.18)' }} />

        <MenuItem onClick={handleLogout} sx={{ py: 1.5 }}>
          <ListItemIcon>
            <LogoutRoundedIcon sx={{ color: 'secondary.main' }} />
          </ListItemIcon>
          <ListItemText primary={t('header.logout')} primaryTypographyProps={{ fontWeight: 700 }} />
        </MenuItem>
      </Menu>

      {/* Navigation Drawer */}
      <Drawer
        anchor="left"
        open={isMenuOpen}
        onClose={() => setIsMenuOpen(false)}
        PaperProps={{
          sx: { width: 280, bgcolor: 'background.paper' },
        }}
      >
        {/* Cabecera del drawer */}
        <Box
          sx={{
            px: 2,
            py: 2,
            background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
            color: '#f4f8f7',
          }}
        >
          <Typography variant="h6" sx={{ fontWeight: 800, letterSpacing: 0.3, lineHeight: 1.2 }}>
            {t('app.name')}
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.8 }}>
            {t('app.tagline')}
          </Typography>
        </Box>
        <Divider />
        <Box sx={{ pt: 1, pb: 1 }}>
          <List disablePadding>
            {links.map((link) => {
              const isActive =
                link.to === '/'
                  ? location.pathname === '/'
                  : location.pathname.startsWith(link.to);
              const Icon = link.icon;
              return (
                <NavLink
                  key={link.to}
                  to={link.to}
                  style={{ textDecoration: 'none', color: 'inherit' }}
                >
                  <ListItemButton
                    selected={isActive}
                    onClick={() => setIsMenuOpen(false)}
                    sx={{
                      borderRadius: 1,
                      mx: 1,
                      mb: 0.5,
                      '&.Mui-selected': {
                        bgcolor: 'primary.main',
                        color: 'primary.contrastText',
                        '& .MuiListItemIcon-root': { color: 'primary.contrastText' },
                        '&:hover': { bgcolor: 'primary.dark' },
                      },
                    }}
                  >
                    <ListItemIcon sx={{ minWidth: 36 }}>
                      <Icon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText
                      primary={t(link.labelKey)}
                      primaryTypographyProps={{ fontWeight: 600 }}
                    />
                  </ListItemButton>
                </NavLink>
              );
            })}
          </List>
        </Box>
      </Drawer>
    </>
  );
}
