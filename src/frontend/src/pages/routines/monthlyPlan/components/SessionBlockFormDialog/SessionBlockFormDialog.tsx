import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import MenuItem from '@mui/material/MenuItem';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import { PopupDialog } from '../../../../../components/PopupDialog/PopupDialog';
import type { ICreateSessionBlockRequest } from '../../../../../interfaces/trainingSessions/trainingSessions';
import type { ExerciseTrainingProfile } from '../../../../../utils/exerciseProfile';
import { blockTypeSupportsIntensity, getBlockTypesForProfile } from '../sessionBlockTypes';

const DESCRIPTION_MAX_LENGTH = 100;
const DEFAULT_DURATION_UNIT_CODE = 'MIN';
const RPE_LEVELS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

export interface SessionBlockFormDialogProps {
  open: boolean;
  profile: ExerciseTrainingProfile;
  initialBlock: ICreateSessionBlockRequest | null;
  onClose: () => void;
  onSave: (block: ICreateSessionBlockRequest) => void;
}

function createEmptyBlockDraft(): ICreateSessionBlockRequest {
  return {
    blockType: '',
    name: '',
    durationValue: 0,
    durationUnitCode: DEFAULT_DURATION_UNIT_CODE,
    description: '',
    intensityRpe: null,
    orderIndex: 0,
  };
}

export function SessionBlockFormDialog({ open, profile, initialBlock, onClose, onSave }: SessionBlockFormDialogProps) {
  const { t } = useTranslation();
  const [draft, setDraft] = useState<ICreateSessionBlockRequest>(createEmptyBlockDraft());
  const blockTypes = getBlockTypesForProfile(profile);

  useEffect(() => {
    if (open) {
      setDraft(initialBlock ? { ...initialBlock } : createEmptyBlockDraft());
    }
  }, [open, initialBlock]);

  const handleSelectBlockType = (code: string) => {
    setDraft((prev) => ({
      ...prev,
      blockType: code,
      // BR-005: descartar la intensidad si el nuevo tipo no la soporta.
      intensityRpe: blockTypeSupportsIntensity(code) ? prev.intensityRpe : null,
    }));
  };

  const disableSave =
    draft.blockType.trim().length === 0 ||
    draft.name.trim().length === 0 ||
    !(draft.durationValue > 0);

  const onSubmit = () => {
    if (disableSave) {
      return;
    }
    onSave(draft);
  };

  return (
    <PopupDialog
      open={open}
      title={initialBlock
        ? (t('monthlyPlan.blockForm.editTitle') || 'Editar bloque')
        : (t('monthlyPlan.blockForm.createTitle') || 'Añadir bloque')}
      onClose={onClose}
      onSubmit={onSubmit}
      closeLabel={t('common.actions.cancel') || 'Cancelar'}
      saveLabel={t('common.actions.save') || 'Guardar'}
      disableSave={disableSave}
    >
      <Stack spacing={2}>
        <Stack spacing={1}>
          <Typography variant="subtitle2">
            {t('monthlyPlan.blockForm.typeLabel') || 'Tipo de bloque'}
          </Typography>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(120px, 1fr))',
              gap: 1,
            }}
          >
            {blockTypes.map((type) => {
              const selected = draft.blockType === type.code;
              return (
                <ButtonBase
                  key={type.code}
                  onClick={() => handleSelectBlockType(type.code)}
                  sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 0.5,
                    p: 1.25,
                    borderRadius: 2,
                    border: 2,
                    borderColor: selected ? type.color : 'divider',
                    bgcolor: selected ? `${type.color}1A` : 'transparent',
                  }}
                >
                  <Box sx={{ color: type.color }}>{type.icon}</Box>
                  <Typography variant="caption" sx={{ fontWeight: selected ? 700 : 400 }}>
                    {t(type.labelKey) || type.code}
                  </Typography>
                </ButtonBase>
              );
            })}
          </Box>
        </Stack>

        <TextField
          label={t('monthlyPlan.blockForm.nameLabel') || 'Nombre del bloque'}
          value={draft.name}
          onChange={(event) => setDraft((prev) => ({ ...prev, name: event.target.value }))}
          fullWidth
        />

        <TextField
          label={t('monthlyPlan.blockForm.durationLabel') || 'Duración (min)'}
          type="number"
          value={draft.durationValue === 0 ? '' : draft.durationValue}
          onChange={(event) => setDraft((prev) => ({ ...prev, durationValue: Number(event.target.value) || 0 }))}
          slotProps={{ htmlInput: { min: 0 } }}
          fullWidth
        />

        <TextField
          label={t('monthlyPlan.blockForm.descriptionLabel') || 'Descripción'}
          value={draft.description ?? ''}
          onChange={(event) => {
            const value = event.target.value.slice(0, DESCRIPTION_MAX_LENGTH);
            setDraft((prev) => ({ ...prev, description: value }));
          }}
          helperText={`${(draft.description ?? '').length}/${DESCRIPTION_MAX_LENGTH}`}
          multiline
          minRows={2}
          fullWidth
        />

        {blockTypeSupportsIntensity(draft.blockType) ? (
          <TextField
            select
            label={t('monthlyPlan.blockForm.intensityLabel') || 'Intensidad (RPE)'}
            value={draft.intensityRpe ?? ''}
            onChange={(event) => setDraft((prev) => ({ ...prev, intensityRpe: Number(event.target.value) }))}
            fullWidth
          >
            {RPE_LEVELS.map((level) => (
              <MenuItem key={level} value={level}>
                {level} — {t(`monthlyPlan.rpeLevels.${level}`)}
              </MenuItem>
            ))}
          </TextField>
        ) : null}
      </Stack>
    </PopupDialog>
  );
}
