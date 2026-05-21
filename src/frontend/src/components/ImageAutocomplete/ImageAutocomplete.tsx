import FitnessCenterRoundedIcon from '@mui/icons-material/FitnessCenterRounded';
import Autocomplete from '@mui/material/Autocomplete';
import Avatar from '@mui/material/Avatar';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

type ImageAutocompleteProps<TOption> = {
  options: TOption[];
  value: TOption | null;
  inputValue: string;
  onSelect: (value: TOption | null) => void;
  onInputChange: (value: string) => void;
  placeholder: string;
  label?: string;
  getOptionKey: (option: TOption) => string;
  getOptionLabel: (option: TOption) => string;
  getOptionSubtitle?: (option: TOption) => string;
  getOptionImage?: (option: TOption) => string | undefined;
  noOptionsText?: string;
  autoFocus?: boolean;
};

export function ImageAutocomplete<TOption>({
  options,
  value,
  inputValue,
  onSelect,
  onInputChange,
  placeholder,
  label,
  getOptionKey,
  getOptionLabel,
  getOptionSubtitle,
  getOptionImage,
  noOptionsText = 'Sin resultados',
  autoFocus = false,
}: ImageAutocompleteProps<TOption>) {
  return (
    <Autocomplete
      options={options}
      value={value}
      inputValue={inputValue}
      autoFocus={autoFocus}
      onChange={(_, nextValue) => onSelect(nextValue)}
      onInputChange={(_, nextValue) => onInputChange(nextValue)}
      getOptionLabel={(option) => getOptionLabel(option)}
      isOptionEqualToValue={(option, currentValue) => getOptionKey(option) === getOptionKey(currentValue)}
      noOptionsText={noOptionsText}
      renderOption={(props, option) => (
        <li {...props} key={getOptionKey(option)}>
          <Stack direction="row" spacing={1.2} alignItems="center" sx={{ width: '100%' }}>
            <Avatar
              src={getOptionImage?.(option)}
              alt={getOptionLabel(option)}
              imgProps={{
                onError: (event) => {
                  event.currentTarget.style.display = 'none';
                },
              }}
              variant="rounded"
              sx={{
                width: 36,
                height: 36,
                borderRadius: 1.5,
                bgcolor: 'rgba(31,79,70,0.12)',
                color: '#1f4f46',
              }}
            >
              <FitnessCenterRoundedIcon fontSize="small" />
            </Avatar>
            <Stack spacing={0.2} sx={{ minWidth: 0 }}>
              <Typography sx={{ fontWeight: 700 }} noWrap>
                {getOptionLabel(option)}
              </Typography>
              {getOptionSubtitle && (
                <Typography variant="caption" color="text.secondary" noWrap>
                  {getOptionSubtitle(option)}
                </Typography>
              )}
            </Stack>
          </Stack>
        </li>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          placeholder={placeholder}
          InputProps={{
            ...params.InputProps,
            sx: {
              borderRadius: 2.5,
              '& input': {
                py: 1.05,
                fontSize: 16,
              },
            },
          }}
        />
      )}
    />
  );
}
