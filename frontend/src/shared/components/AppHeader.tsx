import { type MouseEvent, useState } from 'react';

import AppBar from '@mui/material/AppBar';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import Chip from '@mui/material/Chip';
import Drawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Switch from '@mui/material/Switch';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import FitnessCenterRoundedIcon from '@mui/icons-material/FitnessCenterRounded';
import InsightsRoundedIcon from '@mui/icons-material/InsightsRounded';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import RestaurantMenuRoundedIcon from '@mui/icons-material/RestaurantMenuRounded';
import SettingsRoundedIcon from '@mui/icons-material/SettingsRounded';
import PersonRoundedIcon from '@mui/icons-material/PersonRounded';
import WbSunnyRoundedIcon from '@mui/icons-material/WbSunnyRounded';
import SportsGymnasticsRoundedIcon from '@mui/icons-material/SportsGymnasticsRounded';
import HomeRoundedIcon from '@mui/icons-material/HomeRounded';
import MenuIcon from '@mui/icons-material/Menu';
import type { SelectChangeEvent } from '@mui/material/Select';
import { useTranslation } from 'react-i18next';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';

import { useAppPreferences } from '../../app/context/AppPreferencesContext';

const links = [
  { to: '/', labelKey: 'nav.overview', icon: HomeRoundedIcon },
  { to: '/workouts', labelKey: 'nav.workout', icon: FitnessCenterRoundedIcon },
  { to: '/progress/bench-press', labelKey: 'nav.progress', icon: InsightsRoundedIcon },
  { to: '/routines/new', labelKey: 'nav.routines', icon: SportsGymnasticsRoundedIcon },
  { to: '/diets/new', labelKey: 'nav.diets', icon: RestaurantMenuRoundedIcon },
  { to: '/meals/log', labelKey: 'nav.meals', icon: RestaurantMenuRoundedIcon },
  { to: '/settings/preferences', labelKey: 'nav.settings', icon: SettingsRoundedIcon },
];

export function AppHeader() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [profileAnchor, setProfileAnchor] = useState<null | HTMLElement>(null);
  const location = useLocation();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { profile, themeMode, toggleThemeMode } = useAppPreferences();

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
  };

  const handleToggleTheme = () => {
    toggleThemeMode();
  };

  const activeLink = links.find((link) => {
    if (link.to === '/') {
      return location.pathname === '/';
    }

    return location.pathname.startsWith(link.to.split('/:')[0]);
  });

  return (
    <>
      <AppBar
        position="sticky"
        elevation={0}
        sx={{
          borderBottom: '1px solid',
          borderColor: 'divider',
          background: 'linear-gradient(120deg, #123630 0%, #1f4f46 48%, #2b675b 100%)',
        }}
      >
        <Toolbar disableGutters sx={{ minHeight: 82, py: 1.25, pl: 1.25, pr: 1.5 }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" width="100%" gap={2}>
            <Stack direction="row" spacing={1.5} alignItems="center">
              <IconButton
                aria-label="Open navigation menu"
                onClick={() => setIsMenuOpen(true)}
                sx={{
                  color: '#ffffff',
                  border: '1px solid rgba(255,255,255,0.24)',
                  borderRadius: 1.5,
                }}
              >
                <MenuIcon />
              </IconButton>

              <Box>
                <Typography variant="h6" sx={{ fontWeight: 800, letterSpacing: 0.3 }}>
                  {t('app.name')}
                </Typography>
                <Typography variant="body2" sx={{ opacity: 0.85 }}>
                  {t('app.tagline')}
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

        <MenuItem sx={{ py: 1.1 }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" width="100%" spacing={2}>
            <Stack direction="row" alignItems="center" spacing={1.5}>
              <WbSunnyRoundedIcon sx={{ color: 'secondary.main' }} />
              <Typography sx={{ fontWeight: 700 }}>{t('header.darkMode')}</Typography>
            </Stack>
            <Switch
              checked={themeMode === 'dark'}
              onChange={handleToggleTheme}
              sx={{
                '& .MuiSwitch-switchBase': {
                  color: 'rgba(255,255,255,0.9)',
                },
                '& .MuiSwitch-track': {
                  backgroundColor: 'rgba(255,255,255,0.35)',
                  opacity: 1,
                },
                '& .MuiSwitch-switchBase.Mui-checked': {
                  color: 'secondary.main',
                },
                '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                  backgroundColor: 'secondary.main',
                  opacity: 1,
                },
              }}
            />
          </Stack>
        </MenuItem>

        <Divider sx={{ borderColor: 'rgba(255,255,255,0.18)' }} />

        <MenuItem onClick={handleLogout} sx={{ py: 1.5 }}>
          <ListItemIcon>
            <LogoutRoundedIcon sx={{ color: 'secondary.main' }} />
          </ListItemIcon>
          <ListItemText primary={t('header.logout')} primaryTypographyProps={{ fontWeight: 700 }} />
        </MenuItem>
      </Menu>

      <Drawer
        anchor="left"
        open={isMenuOpen}
        onClose={() => setIsMenuOpen(false)}
        PaperProps={{
          sx: {
            width: 300,
            color: '#f4f8f7',
            background: 'linear-gradient(180deg, #123630 0%, #1f4f46 52%, #2b675b 100%)',
          },
        }}
      >
        <Box sx={{ pt: 2 }} role="navigation" aria-label={t('header.menuLabel')}>
          <Box sx={{ px: 2.25, pb: 1.5 }}>
            <Typography variant="h5" sx={{ fontWeight: 800, letterSpacing: 0.25 }}>
              {t('app.name')}
            </Typography>
            <Typography variant="body2" sx={{ opacity: 0.86 }}>
              {t('app.tagline')}
            </Typography>
          </Box>

          <Typography
            variant="overline"
            sx={{ px: 2.25, display: 'block', opacity: 0.78, letterSpacing: 1.2 }}
          >
            {t('header.menuTitle')}
          </Typography>

          <Divider
            sx={{
              mx: 2.25,
              mt: 0.5,
              mb: 0.75,
              borderColor: 'rgba(255,255,255,0.24)',
            }}
          />

          <List>
            {links.map((link) => (
              <ListItemButton
                key={link.to}
                component={NavLink}
                to={link.to}
                selected={location.pathname === link.to || (link.to !== '/' && location.pathname.startsWith(link.to))}
                onClick={() => setIsMenuOpen(false)}
                sx={{
                  mx: 1,
                  mt: 0.5,
                  borderRadius: 1.5,
                  '&.Mui-selected': {
                    bgcolor: '#f8d089',
                    color: '#10312b',
                    '& .MuiListItemIcon-root': {
                      color: '#10312b',
                    },
                  },
                  '&:hover': {
                    bgcolor: 'rgba(255,255,255,0.12)',
                  },
                }}
              >
                <ListItemIcon sx={{ color: 'rgba(255,255,255,0.9)', minWidth: 36 }}>
                  <link.icon fontSize="small" />
                </ListItemIcon>
                <ListItemText primary={t(link.labelKey)} />
              </ListItemButton>
            ))}
          </List>
        </Box>
      </Drawer>
    </>
  );
}
