import { useState } from 'react';

import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import { useTranslation } from 'react-i18next';

import {  UnitsPanel} from './units/UnitsPanel';
import { MusclesPanel } from './muscles/MusclesPanel';
import { ExercisesPanel } from './exercises/ExercisesPanel';

const TABS = [
  { key: 'administration.tabs.muscles' },
  { key: 'administration.tabs.measurementTypes' },
  { key: 'administration.tabs.exercises' },
] as const;

export function AdministrationPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<number>(0);

  return (
    <Container
      maxWidth={false}
      sx={{
        py: { xs: 2, md: 3 },
        px: { xs: 1.5, sm: 2, md: 3, lg: 4 },
        height: '100%',
        minHeight: 0,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      <Tabs
        value={activeTab}
        onChange={(_event, newValue: number) => setActiveTab(newValue)}
        aria-label={t('administration.title')}
        sx={{ mb: 2 }}
      >
        {TABS.map((tab) => (
          <Tab key={tab.key} label={t(tab.key)} />
        ))}
      </Tabs>

      <Box sx={{ flex: 1, minHeight: 0, overflow: 'hidden' }}>
        {activeTab === 0 ? <MusclesPanel supportsImport={true} /> : null}
        {activeTab === 1 ? <UnitsPanel  supportsImport={true} /> : null}
        {activeTab === 2 ? <ExercisesPanel supportsImport={true} /> : null}
      </Box>
    </Container>
  );
}
