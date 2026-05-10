import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';

import type { MuscleGroup } from '../../../shared/types/catalog';

type MuscleGroupSelectorProps = {
  value: string[];
  options: MuscleGroup[];
  onChange: (value: string[]) => void;
};

export function MuscleGroupSelector({ value, options, onChange }: MuscleGroupSelectorProps) {
  return (
    <FormControl fullWidth>
      <InputLabel id="muscle-groups-label">Grupos musculares</InputLabel>
      <Select
        labelId="muscle-groups-label"
        multiple
        label="Grupos musculares"
        value={value}
        onChange={(event) => onChange(event.target.value as string[])}
      >
        {options.map((option) => (
          <MenuItem key={option.id} value={option.id}>{option.name}</MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
