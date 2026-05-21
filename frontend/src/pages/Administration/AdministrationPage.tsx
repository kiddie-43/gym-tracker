import { useState } from 'react';

import Box from '@mui/material/Box';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import { useTranslation } from 'react-i18next';

import { ExercisesPanel } from '../../components/administration/ExercisesPanel';
import { MeasurementTypesPanel } from '../../components/administration/MeasurementTypesPanel';
import { MusclesPanel } from '../../components/administration/MusclesPanel';

const ADMIN_TABS = [
  { key: 'administration.tabs.muscles' },
  { key: 'administration.tabs.measurementTypes' },
  { key: 'administration.tabs.exercises' },
] as const;

export function AdministrationPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<number>(0);

  return (
    <Box
      sx={{
        width: '100%',
        height: '100%',
        minHeight: 0,
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <Tabs
        value={activeTab}
        onChange={(_event, value: number) => setActiveTab(value)}
        aria-label={t('administration.title')}
        variant="fullWidth"
        sx={{
          minHeight: { xs: 72, md: 80 },
          borderBottom: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
          '& .MuiTab-root': {
            minHeight: { xs: 72, md: 80 },
            fontSize: { xs: '0.875rem', md: '0.95rem' },
            fontWeight: 600,
            letterSpacing: 0,
          },
        }}
      >
        {ADMIN_TABS.map((tab) => (
          <Tab key={tab.key} label={t(tab.key)} />
        ))}
      </Tabs>

      <Box sx={{ flex: 1, minHeight: 0, p: { xs: 2, md: 3 } }}>
        {activeTab === 0 ? <MusclesPanel /> : null}
        {activeTab === 1 ? <MeasurementTypesPanel /> : null}
        {activeTab === 2 ? <ExercisesPanel /> : null}
      </Box>
    </Box>
  );
}
