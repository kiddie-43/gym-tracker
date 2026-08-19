import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';

interface MonthlyDayTabsProps {
  selectedDay: number;
  activeDays: number;
  onChange: (day: number) => void;
}

export function MonthlyDayTabs({ selectedDay, activeDays, onChange }: MonthlyDayTabsProps) {
  const days = Array.from({ length: activeDays }, (_, index) => index + 1);

  return (
    <Tabs
      value={selectedDay}
      onChange={(_, value: number) => onChange(value)}
      variant="scrollable"
      scrollButtons
      allowScrollButtonsMobile
      aria-label="day tabs"
      sx={{
        width: '100%',
        maxWidth: '100%',
        minHeight: 44,
        '& .MuiTabs-scrollButtons.Mui-disabled': { opacity: 0.3 },
        '& .MuiTab-root': {
          minWidth: { xs: 64, sm: 90 },
          minHeight: 44,
          fontSize: { xs: '0.75rem', sm: '0.875rem' },
          px: { xs: 1, sm: 2 },
        },
      }}
    >
      {days.map((day) => (
        <Tab key={day} label={`Dia ${day}`} value={day} />
      ))}
    </Tabs>
  );
}
