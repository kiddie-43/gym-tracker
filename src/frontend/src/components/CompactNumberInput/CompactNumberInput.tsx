import TextField from '@mui/material/TextField';

type CompactNumberInputProps = {
  value: string;
  placeholder?: string;
  mode?: 'decimal' | 'numeric';
  onChange: (value: string) => void;
  onFocus?: () => void;
};

function normalizeValue(rawValue: string, mode: 'decimal' | 'numeric') {
  const normalized = rawValue.replace(',', '.').replace(/[^\d.]/g, '');

  if (mode === 'numeric') {
    return normalized.replace(/\./g, '');
  }

  const parts = normalized.split('.');
  if (parts.length <= 1) {
    return normalized;
  }

  return `${parts[0]}.${parts.slice(1).join('')}`;
}

export function CompactNumberInput({
  value,
  placeholder,
  mode = 'decimal',
  onChange,
  onFocus,
}: CompactNumberInputProps) {
  return (
    <TextField
      size="small"
      value={value}
      placeholder={placeholder}
      onFocus={(event) => {
        event.currentTarget.select();
        onFocus?.();
      }}
      onChange={(event) => onChange(normalizeValue(event.target.value, mode))}
      inputProps={{
        inputMode: mode,
        style: {
          textAlign: 'center',
          padding: '8px 6px',
        },
      }}
      sx={{
        '& .MuiOutlinedInput-root': {
          borderRadius: 1.5,
          bgcolor: 'background.paper',
          '& input::-webkit-outer-spin-button, & input::-webkit-inner-spin-button': {
            WebkitAppearance: 'none',
            margin: 0,
          },
          '& input[type=number]': {
            MozAppearance: 'textfield',
          },
        },
      }}
    />
  );
}
