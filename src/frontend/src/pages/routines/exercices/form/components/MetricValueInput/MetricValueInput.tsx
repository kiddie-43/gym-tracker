import AddRoundedIcon from '@mui/icons-material/AddRounded';
import RemoveRoundedIcon from '@mui/icons-material/RemoveRounded';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import Paper from '@mui/material/Paper';
import Slider from '@mui/material/Slider';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { alpha, useTheme } from '@mui/material/styles';
import { useEffect, useRef, useState } from 'react';

interface MetricValueInputProps {
  metricCode: string;
  label: string;
  value: number | string;
  disabled?: boolean;
  onChange: (value: number) => void;
}

interface MetricInputConfig {
  helperText: string;
  unitLabel: string;
  mode: 'slider' | 'time' | 'number';
  step: number;
  min: number;
  max?: number;
  decimalPlaces: number;
  inputMode: 'numeric' | 'decimal';
  accentLabel: string;
  sliderMax?: number;
}

interface MetricConfigEntry {
  codes: string[];
  config: MetricInputConfig;
}

type NumericInputMode = MetricInputConfig['inputMode'];

const METRIC_CONFIGS: MetricConfigEntry[] = [
  {
    codes: ['weight', 'weightkg', 'kg', 'load', 'carga'],
    config: {
      helperText: 'Peso en kilogramos.',
      unitLabel: 'kg',
      mode: 'slider',
      step: 0.5,
      min: 0,
      sliderMax: 200,
      decimalPlaces: 1,
      inputMode: 'decimal',
      accentLabel: 'PESO',
    },
  },
  {
    codes: ['reps', 'rep', 'repetitions', 'repeticiones'],
    config: {
      helperText: 'Numero de repeticiones.',
      unitLabel: 'reps',
      mode: 'slider',
      step: 1,
      min: 0,
      sliderMax: 30,
      decimalPlaces: 0,
      inputMode: 'numeric',
      accentLabel: 'REPS',
    },
  },
  {
    codes: ['distance', 'distancia', 'km', 'meters', 'meter', 'm'],
    config: {
      helperText: 'Distancia recorrida.',
      unitLabel: 'km',
      mode: 'slider',
      step: 0.1,
      min: 0,
      sliderMax: 50,
      decimalPlaces: 1,
      inputMode: 'decimal',
      accentLabel: 'DISTANCIA',
    },
  },
  {
    codes: ['time', 'tiempo', 'duration', 'min', 'minutes'],
    config: {
      helperText: 'Introduce horas y minutos.',
      unitLabel: 'h min',
      mode: 'time',
      step: 1,
      min: 0,
      decimalPlaces: 0,
      inputMode: 'numeric',
      accentLabel: 'TIEMPO',
    },
  },
  {
    codes: ['resttime', 'tiempodedescanso'],
    config: {
      helperText: 'Introduce el descanso entre series en horas y minutos.',
      unitLabel: 'h min',
      mode: 'time',
      step: 1,
      min: 0,
      decimalPlaces: 0,
      inputMode: 'numeric',
      accentLabel: 'DESCANSO',
    },
  },
  {
    codes: ['kcal', 'calories', 'caloria', 'calorias', 'calorie'],
    config: {
      helperText: 'Calorias registradas.',
      unitLabel: 'kcal',
      mode: 'number',
      step: 1,
      min: 0,
      decimalPlaces: 0,
      inputMode: 'numeric',
      accentLabel: 'CALORIAS',
    },
  },
  {
    codes: ['rpe'],
    config: {
      helperText: 'Escala de esfuerzo percibido.',
      unitLabel: '/10',
      mode: 'slider',
      step: 0.5,
      min: 0,
      max: 10,
      decimalPlaces: 1,
      inputMode: 'decimal',
      accentLabel: 'RPE',
    },
  },
];

const DEFAULT_CONFIG: MetricInputConfig = {
  helperText: 'Introduce el valor de la metrica.',
  unitLabel: '',
  mode: 'slider',
  step: 0.1,
  min: 0,
  decimalPlaces: 1,
  inputMode: 'decimal',
  accentLabel: 'METRICA',
  sliderMax: 100,
};

function normalizeMetricCode(metricCode: string): string {
  return metricCode.trim().toLowerCase();
}

function getMetricInputConfig(metricCode: string): MetricInputConfig {
  const normalizedCode = normalizeMetricCode(metricCode);
  const matchedConfig = METRIC_CONFIGS.find((entry) => entry.codes.includes(normalizedCode));

  return matchedConfig?.config ?? DEFAULT_CONFIG;
}

function formatMetricValue(value: number | string, decimalPlaces: number): string {
  if (value === '') {
    return '';
  }

  const numericValue = typeof value === 'number' ? value : Number(value);

  if (!Number.isFinite(numericValue)) {
    return '';
  }

  if (decimalPlaces <= 0) {
    return String(Math.round(numericValue));
  }

  return numericValue.toFixed(decimalPlaces);
}

function sanitizeMetricInput(rawValue: string, inputMode: NumericInputMode): string {
  const normalizedValue = rawValue.replace(',', '.');

  if (inputMode === 'numeric') {
    return normalizedValue.replace(/\D+/g, '');
  }

  const cleanedValue = normalizedValue.replace(/[^\d.]+/g, '');
  const [integerPart = '', ...decimalParts] = cleanedValue.split('.');
  const decimalPart = decimalParts.join('');

  if (cleanedValue.startsWith('.')) {
    return decimalPart.length > 0 ? `0.${decimalPart}` : '0.';
  }

  return decimalParts.length > 0 ? `${integerPart}.${decimalPart}` : integerPart;
}

function parseMetricValue(rawValue: string, inputMode: NumericInputMode): number {
  const sanitizedValue = sanitizeMetricInput(rawValue, inputMode);

  if (!sanitizedValue.trim()) {
    return 0;
  }

  const parsedValue = Number(sanitizedValue);

  return Number.isFinite(parsedValue) ? parsedValue : 0;
}

function shouldPreventNumericKey(key: string, inputMode: NumericInputMode): boolean {
  if (['e', 'E', '+', '-'].includes(key)) {
    return true;
  }

  if (inputMode === 'numeric' && [',', '.'].includes(key)) {
    return true;
  }

  return false;
}

function clampMetricValue(value: number, min: number, max?: number): number {
  const boundedMin = Math.max(value, min);

  if (typeof max === 'number') {
    return Math.min(boundedMin, max);
  }

  return boundedMin;
}

function formatTimeParts(totalMinutes: number): { hours: string; minutes: string } {
  const safeMinutes = Math.max(0, Math.round(totalMinutes));
  const hours = Math.floor(safeMinutes / 60);
  const minutes = safeMinutes % 60;

  return {
    hours: String(hours),
    minutes: String(minutes).padStart(2, '0'),
  };
}

function buildSliderMarks(maxValue: number): Array<{ value: number; label: string }> {
  const quarter = maxValue / 4;

  return [
    { value: 0, label: '0' },
    { value: quarter, label: String(Math.round(quarter)) },
    { value: maxValue / 2, label: String(Math.round(maxValue / 2)) },
    { value: maxValue, label: String(Math.round(maxValue)) },
  ];
}

export function MetricValueInput({ metricCode, label, value, disabled = false, onChange }: MetricValueInputProps) {
  const theme = useTheme();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [isEditingValue, setIsEditingValue] = useState(false);
  const [draftValue, setDraftValue] = useState('');
  const config = getMetricInputConfig(metricCode);
  const formattedValue = formatMetricValue(value, config.decimalPlaces);
  const numericValue = formattedValue === '' ? 0 : parseMetricValue(formattedValue);
  const sliderMax = config.sliderMax ?? Math.max(config.min + config.step, 100);
  const sliderValue = clampMetricValue(numericValue, config.min, config.max ?? sliderMax);
  const timeParts = formatTimeParts(numericValue);

  useEffect(() => {
    if (!isEditingValue) {
      setDraftValue(formattedValue);
    }
  }, [formattedValue, isEditingValue]);

  useEffect(() => {
    if (isEditingValue) {
      window.requestAnimationFrame(() => {
        inputRef.current?.focus();
        inputRef.current?.select();
      });
    }
  }, [isEditingValue]);

  const applyDelta = (delta: number) => {
    const nextValue = clampMetricValue(numericValue + delta, config.min, config.max ?? sliderMax);
    const fixedValue = config.decimalPlaces > 0 ? Number(nextValue.toFixed(config.decimalPlaces)) : Math.round(nextValue);

    onChange(fixedValue);
  };

  const commitDraftValue = () => {
    const parsedValue = parseMetricValue(draftValue, config.inputMode);
    const nextValue = clampMetricValue(parsedValue, config.min, config.max ?? sliderMax);
    const fixedValue = config.decimalPlaces > 0 ? Number(nextValue.toFixed(config.decimalPlaces)) : Math.round(nextValue);

    onChange(fixedValue);
    setIsEditingValue(false);
  };

  const setTimeValue = (hoursText: string, minutesText: string) => {
    const hours = Number(sanitizeMetricInput(hoursText, 'numeric') || 0);
    const minutes = Math.min(59, Math.max(0, Number(sanitizeMetricInput(minutesText, 'numeric') || 0)));
    const safeHours = Number.isFinite(hours) ? Math.max(0, hours) : 0;

    onChange((safeHours * 60) + minutes);
  };

  const renderSliderBody = () => (
    <Stack spacing={1.75}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'baseline',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography variant="caption" sx={{ color: 'text.secondary', letterSpacing: 1, fontWeight: 700 }}>
          {config.accentLabel}
        </Typography>
        <Typography variant="caption" sx={{ color: 'text.secondary' }}>
          Arrastra para ajustar
        </Typography>
      </Box>

      <Stack direction="row" alignItems="center" spacing={1.25}>
        <IconButton
          disabled={disabled}
          onClick={() => applyDelta(-config.step)}
          aria-label={`Disminuir ${label}`}
          sx={{
            width: 56,
            height: 56,
            borderRadius: 2.5,
            flexShrink: 0,
            border: '1px solid',
            borderColor: alpha(theme.palette.divider, 0.8),
            bgcolor: alpha(theme.palette.common.white, 0.04),
            boxShadow: `0 8px 18px ${alpha(theme.palette.common.black, 0.08)}`,
            '&:hover': {
              bgcolor: alpha(theme.palette.primary.main, 0.08),
            },
          }}
        >
          <RemoveRoundedIcon />
        </IconButton>

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Box
            sx={{
              textAlign: 'center',
              pb: 1,
            }}
          >
            {isEditingValue ? (
              <TextField
                inputRef={inputRef}
                value={draftValue}
                onChange={(event) => {
                  setDraftValue(sanitizeMetricInput(event.target.value, config.inputMode));
                }}
                onBlur={commitDraftValue}
                onKeyDown={(event) => {
                  if (shouldPreventNumericKey(event.key, config.inputMode)) {
                    event.preventDefault();
                    return;
                  }

                  if (event.key === 'Enter') {
                    event.preventDefault();
                    commitDraftValue();
                  }

                  if (event.key === 'Escape') {
                    event.preventDefault();
                    setIsEditingValue(false);
                    setDraftValue(formattedValue);
                  }
                }}
                type="text"
                inputMode={config.inputMode}
                variant="standard"
                autoComplete="off"
                sx={{
                  width: 'min(220px, 100%)',
                  '& .MuiInput-root': {
                    fontSize: 'clamp(2rem, 6vw, 3.25rem)',
                    fontWeight: 800,
                    justifyContent: 'center',
                    color: 'text.primary',
                  },
                  '& .MuiInput-input': {
                    textAlign: 'center',
                    padding: 0,
                  },
                  '& .MuiInput-underline:before, & .MuiInput-underline:after': {
                    borderBottom: 'none',
                  },
                }}
              />
            ) : (
              <Box
                component="button"
                type="button"
                onClick={() => {
                  if (!disabled) {
                    setIsEditingValue(true);
                  }
                }}
                disabled={disabled}
                sx={{
                  border: 'none',
                  background: 'transparent',
                  p: 0,
                  color: 'inherit',
                  cursor: disabled ? 'default' : 'text',
                  textAlign: 'center',
                }}
              >
                <Typography
                  variant="h3"
                  sx={{
                    fontWeight: 800,
                    lineHeight: 1,
                    color: 'text.primary',
                    fontSize: 'clamp(2rem, 6vw, 3.25rem)',
                  }}
                >
                  {formattedValue === '' ? '0' : formattedValue}
                </Typography>
              </Box>
            )}
            {config.unitLabel ? (
              <Typography
                variant="caption"
                sx={{
                  color: 'text.secondary',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  display: 'block',
                  mt: 0.75,
                }}
              >
                {config.unitLabel}
              </Typography>
            ) : null}
          </Box>

          <Box
            sx={{
              px: 0.75,
              pt: 0.75,
              pb: 0.25,
              borderRadius: 999,
              bgcolor: alpha(theme.palette.common.black, 0.08),
            }}
          >
            <Slider
              value={sliderValue}
              min={config.min}
              max={config.max ?? sliderMax}
              step={config.step}
              marks={buildSliderMarks(config.max ?? sliderMax)}
              disabled={disabled}
              onChange={(_, nextValue) => {
                if (typeof nextValue === 'number') {
                  const fixedValue = config.decimalPlaces > 0 ? Number(nextValue.toFixed(config.decimalPlaces)) : Math.round(nextValue);
                  onChange(fixedValue);
                }
              }}
              sx={{
                color: theme.palette.primary.main,
                '& .MuiSlider-thumb': {
                  width: 24,
                  height: 24,
                  boxShadow: `0 0 0 8px ${alpha(theme.palette.primary.main, 0.12)}`,
                },
                '& .MuiSlider-track': {
                  border: 'none',
                  height: 10,
                },
                '& .MuiSlider-rail': {
                  opacity: 1,
                  height: 10,
                  bgcolor: alpha(theme.palette.text.primary, 0.1),
                },
                '& .MuiSlider-mark': {
                  width: 2,
                  height: 10,
                  bgcolor: alpha(theme.palette.text.primary, 0.18),
                },
                '& .MuiSlider-markLabel': {
                  color: theme.palette.text.secondary,
                  fontSize: '0.7rem',
                },
              }}
            />
          </Box>
        </Box>

        <IconButton
          disabled={disabled}
          onClick={() => applyDelta(config.step)}
          aria-label={`Aumentar ${label}`}
          sx={{
            width: 56,
            height: 56,
            borderRadius: 2.5,
            flexShrink: 0,
            border: '1px solid',
            borderColor: alpha(theme.palette.divider, 0.8),
            bgcolor: alpha(theme.palette.primary.main, 0.08),
            boxShadow: `0 8px 18px ${alpha(theme.palette.common.black, 0.08)}`,
            '&:hover': {
              bgcolor: alpha(theme.palette.primary.main, 0.16),
            },
          }}
        >
          <AddRoundedIcon />
        </IconButton>
      </Stack>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography variant="caption" sx={{ color: 'text.secondary' }}>
          Arrastra o usa los botones laterales
        </Typography>
        {config.unitLabel ? (
          <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 700 }}>
            {config.helperText}
          </Typography>
        ) : null}
      </Box>

      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
        {config.helperText}
      </Typography>
    </Stack>
  );

  const renderTimeBody = () => (
    <Stack spacing={1.5}>
      <Box sx={{ textAlign: 'center' }}>
        <Typography variant="caption" sx={{ color: 'text.secondary', letterSpacing: 1, fontWeight: 700 }}>
          {config.accentLabel}
        </Typography>
        <Typography
          variant="h3"
          sx={{
            fontWeight: 800,
            lineHeight: 1,
            color: 'text.primary',
            fontSize: 'clamp(2rem, 6vw, 3.25rem)',
          }}
        >
          {timeParts.hours}:{timeParts.minutes}
        </Typography>
      </Box>

      <Stack direction="row" spacing={1.25}>
        <TextField
          label="Hora"
          value={timeParts.hours}
          onChange={(event) => {
            setTimeValue(sanitizeMetricInput(event.target.value, 'numeric'), timeParts.minutes);
          }}
          type="number"
          inputMode="numeric"
          onKeyDown={(event) => {
            if (shouldPreventNumericKey(event.key, 'numeric')) {
              event.preventDefault();
            }
          }}
          disabled={disabled}
          fullWidth
          inputProps={{ min: 0, step: 1 }}
          InputProps={{
            endAdornment: <InputAdornment position="end">h</InputAdornment>,
          }}
        />
        <TextField
          label="Minutos"
          value={timeParts.minutes}
          onChange={(event) => {
            setTimeValue(timeParts.hours, sanitizeMetricInput(event.target.value, 'numeric'));
          }}
          type="number"
          inputMode="numeric"
          onKeyDown={(event) => {
            if (shouldPreventNumericKey(event.key, 'numeric')) {
              event.preventDefault();
            }
          }}
          disabled={disabled}
          fullWidth
          inputProps={{ min: 0, max: 59, step: 1 }}
          InputProps={{
            endAdornment: <InputAdornment position="end">min</InputAdornment>,
          }}
        />
      </Stack>

      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
        {config.helperText}
      </Typography>
    </Stack>
  );

  const renderNumberBody = () => (
    <Stack spacing={1.5}>
      <Box sx={{ textAlign: 'center' }}>
        <Typography variant="caption" sx={{ color: 'text.secondary', letterSpacing: 1, fontWeight: 700 }}>
          {config.accentLabel}
        </Typography>
        <Typography
          variant="h3"
          sx={{
            fontWeight: 800,
            lineHeight: 1,
            color: 'text.primary',
            fontSize: 'clamp(2rem, 6vw, 3.25rem)',
          }}
        >
          {formattedValue === '' ? '0' : formattedValue}
        </Typography>
      </Box>

      <TextField
        value={formattedValue}
        onChange={(event) => {
          const sanitizedValue = sanitizeMetricInput(event.target.value, config.inputMode);
          onChange(parseMetricValue(sanitizedValue, config.inputMode));
        }}
        label={label}
        type="text"
        inputMode={config.inputMode}
        onKeyDown={(event) => {
          if (shouldPreventNumericKey(event.key, config.inputMode)) {
            event.preventDefault();
          }
        }}
        disabled={disabled}
        fullWidth
        inputProps={{
          min: config.min,
          max: config.max,
          step: config.step,
          inputMode: config.inputMode,
          style: {
            textAlign: 'center',
            fontSize: '1.25rem',
            fontWeight: 700,
          },
        }}
        InputProps={{
          endAdornment: config.unitLabel ? (
            <InputAdornment position="end">{config.unitLabel}</InputAdornment>
          ) : undefined,
        }}
      />

      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
        {config.helperText}
      </Typography>
    </Stack>
  );

  return (
    <Paper
      variant="outlined"
      sx={{
        p: 2,
        borderRadius: 3,
        borderColor: alpha(theme.palette.primary.main, 0.18),
        background: `linear-gradient(180deg, ${alpha(theme.palette.background.paper, 0.98)} 0%, ${alpha(theme.palette.primary.main, 0.03)} 100%)`,
        boxShadow: `0 12px 30px ${alpha(theme.palette.common.black, 0.08)}`,
      }}
    >
      <Stack spacing={1.5}>
        <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={1}>
          <Stack spacing={0.25}>
            <Typography variant="subtitle1" sx={{ fontWeight: 800, lineHeight: 1.1 }}>
              {label}
            </Typography>
          </Stack>

          {config.unitLabel ? (
            <Box
              sx={{
                px: 1.25,
                py: 0.5,
                borderRadius: 999,
                bgcolor: alpha(theme.palette.primary.main, 0.14),
                color: theme.palette.primary.main,
                fontSize: '0.75rem',
                fontWeight: 800,
                textTransform: 'uppercase',
              }}
            >
              {config.unitLabel}
            </Box>
          ) : null}
        </Stack>

        {config.mode === 'time' ? renderTimeBody() : null}
        {config.mode === 'number' ? renderNumberBody() : null}
        {config.mode === 'slider' ? renderSliderBody() : null}
      </Stack>
    </Paper>
  );
}
