import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';

interface MonthlyWeekTabsProps {
  selectedWeek: number;
  onChange: (week: number) => void;
}

export function MonthlyWeekTabs({ selectedWeek, onChange }: MonthlyWeekTabsProps) {
  return (
    <Tabs
      value={selectedWeek}
      onChange={(_, value: number) => onChange(value)}
      variant="scrollable"
      scrollButtons
      allowScrollButtonsMobile
      aria-label="weekly tabs"
      sx={{
        width: '100%',
        maxWidth: '100%',
        minHeight: 44,
        '& .MuiTabs-scrollButtons.Mui-disabled': { opacity: 0.3 },
        '& .MuiTab-root': {
          minWidth: { xs: 88, sm: 120 },
          minHeight: 44,
          fontSize: { xs: '0.75rem', sm: '0.875rem' },
          px: { xs: 1, sm: 2 },
        },
      }}
    >
      <Tab label="Semana 1" value={1} />
      <Tab label="Semana 2" value={2} />
      <Tab label="Semana 3" value={3} />
      <Tab label="Semana 4" value={4} />
    </Tabs>
  );
}
