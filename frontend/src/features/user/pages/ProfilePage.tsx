import { ChangeEvent, useState } from 'react';

import Alert from '@mui/material/Alert';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';

import { useAppPreferences } from '../../../app/context/AppPreferencesContext';
import { PageHeader } from '../../../shared/components/PageHeader';

export function ProfilePage() {
  const { t } = useTranslation();
  const { profile, updateProfile } = useAppPreferences();

  const [firstName, setFirstName] = useState(profile.firstName);
  const [lastName, setLastName] = useState(profile.lastName);
  const [email, setEmail] = useState(profile.email);
  const [birthDate, setBirthDate] = useState(profile.birthDate);
  const [photoUrl, setPhotoUrl] = useState(profile.photoUrl);
  const [saved, setSaved] = useState(false);

  function handlePhotoChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        setPhotoUrl(reader.result);
      }
    };

    reader.readAsDataURL(file);
  }

  function handleSave() {
    updateProfile({
      firstName,
      lastName,
      email,
      birthDate,
      photoUrl,
    });
    setSaved(true);
  }

  return (
    <Stack spacing={2.5}>
      <PageHeader title={t('profile.title')} description={t('profile.description')} />

      <Card>
        <CardContent>
          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ xs: 'flex-start', sm: 'center' }}>
              <Avatar src={photoUrl || undefined} sx={{ width: 88, height: 88 }} />
              <Box>
                <Button component="label" variant="outlined">
                  {t('profile.uploadPhoto')}
                  <input hidden accept="image/*" type="file" onChange={handlePhotoChange} />
                </Button>
              </Box>
            </Stack>

            <TextField label={t('profile.firstName')} value={firstName} onChange={(event) => setFirstName(event.target.value)} />
            <TextField label={t('profile.lastName')} value={lastName} onChange={(event) => setLastName(event.target.value)} />
            <TextField label={t('profile.email')} type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
            <TextField
              label={t('profile.birthDate')}
              type="date"
              value={birthDate}
              onChange={(event) => setBirthDate(event.target.value)}
              InputLabelProps={{ shrink: true }}
            />

            <Button variant="contained" onClick={handleSave}>
              {t('profile.save')}
            </Button>
            {saved ? <Alert severity="success">{t('profile.saved')}</Alert> : null}
          </Stack>
        </CardContent>
      </Card>
    </Stack>
  );
}