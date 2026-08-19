import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';

import type { ICreateSessionBlockRequest } from '../../../../../interfaces/trainingSessions/trainingSessions';
import { findBlockTypeMeta } from '../sessionBlockTypes';

export interface SessionBlocksListProps {
  blocks: ICreateSessionBlockRequest[];
  onEdit: (index: number) => void;
  onDelete: (index: number) => void;
}

export function SessionBlocksList({ blocks, onEdit, onDelete }: SessionBlocksListProps) {
  const { t } = useTranslation();

  if (blocks.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
        {t('monthlyPlan.blocksEmpty') || 'Todavía no has añadido ningún bloque.'}
      </Typography>
    );
  }

  return (
    <Stack spacing={1}>
      {blocks.map((block, index) => {
        const meta = findBlockTypeMeta(block.blockType);
        return (
          <Stack
            key={`${block.blockType}-${block.name}-${index}`}
            direction="row"
            alignItems="center"
            spacing={1.5}
            onClick={() => onEdit(index)}
            sx={{
              p: 1.5,
              border: 1,
              borderColor: 'divider',
              borderRadius: 2,
              cursor: 'pointer',
              '&:hover': { bgcolor: 'action.hover' },
            }}
          >
            <Box sx={{ color: meta?.color ?? 'text.secondary', display: 'flex' }}>
              {meta?.icon}
            </Box>
            <Stack sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="subtitle2" noWrap>
                {block.name} · {block.durationValue} {t('monthlyPlan.quickSummary.durationUnit') || 'min'}
              </Typography>
              {block.description ? (
                <Typography variant="body2" color="text.secondary" noWrap>
                  {block.description}
                </Typography>
              ) : null}
            </Stack>
            <IconButton
              size="small"
              aria-label={t('common.actions.delete') || 'Eliminar'}
              onClick={(event) => {
                event.stopPropagation();
                onDelete(index);
              }}
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          </Stack>
        );
      })}
    </Stack>
  );
}
