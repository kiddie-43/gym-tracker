import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';

type HistoryFiltersProps = {
  from: string;
  to: string;
  onFromChange: (value: string) => void;
  onToChange: (value: string) => void;
};

export function HistoryFilters({ from, to, onFromChange, onToChange }: HistoryFiltersProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr 1fr', md: 'auto auto' },
        gap: 1.5,
        p: 1.5,
        backgroundColor: '#f5f5f5',
        borderRadius: '8px',
        border: '1px solid #e0e0e0',
      }}
    >
      <TextField 
        label="Desde" 
        type="date"
        value={from} 
        onChange={(event) => onFromChange(event.target.value)}
        size="small"
        InputLabelProps={{ shrink: true }}
        sx={{
          '& .MuiOutlinedInput-root': {
            backgroundColor: '#fff',
          }
        }}
      />
      <TextField 
        label="Hasta" 
        type="date"
        value={to} 
        onChange={(event) => onToChange(event.target.value)}
        size="small"
        InputLabelProps={{ shrink: true }}
        sx={{
          '& .MuiOutlinedInput-root': {
            backgroundColor: '#fff',
          }
        }}
      />
    </Box>
  );
}
