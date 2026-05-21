import Checkbox from '@mui/material/Checkbox';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import ListItemText from '@mui/material/ListItemText';
import MenuItem from '@mui/material/MenuItem';
import OutlinedInput from '@mui/material/OutlinedInput';
import Select from '@mui/material/Select';

interface MuscleGroupOption {
  id: string;
  name: string;
}

interface MuscleGroupSelectorProps {
  value: string[];
  options: MuscleGroupOption[];
  onChange: (value: string[]) => void;
}

export function MuscleGroupSelector({ value, options, onChange }: MuscleGroupSelectorProps) {
  return (
    <FormControl fullWidth>
      <InputLabel id="muscle-groups-label">Muscle groups</InputLabel>
      <Select
        labelId="muscle-groups-label"
        multiple
        value={value}
        onChange={(event) => onChange(event.target.value as string[])}
        input={<OutlinedInput label="Muscle groups" />}
        renderValue={(selected) => selected.join(', ')}
        inputProps={{ 'aria-label': 'Muscle groups' }}
      >
        {options.map((option) => (
          <MenuItem key={option.id} value={option.id}>
            <Checkbox checked={value.includes(option.id)} />
            <ListItemText primary={option.name} />
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
