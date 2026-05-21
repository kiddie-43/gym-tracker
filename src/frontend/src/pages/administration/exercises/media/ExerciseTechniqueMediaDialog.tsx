import { useMemo, useState } from 'react';
import type { ChangeEvent } from 'react';

import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';

import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import type { ExerciseDto, ExerciseMediaType } from '../../../../interfaces/admin/exercises/exercises';
import { confirmExerciseMedia, requestExerciseUploadUrl } from '../../../../services/api/admin/exercises/exercisesApi';

interface ExerciseTechniqueMediaDialogProps {
  open: boolean;
  exercise: ExerciseDto | null;
  loading: boolean;
  error: string | null;
  onClose: () => void;
  onUploaded: () => Promise<void>;
  onLoadingChange: (loading: boolean) => void;
  onErrorChange: (error: string | null) => void;
}

function detectMediaType(file: File): ExerciseMediaType | null {
  if (file.type.startsWith('image/')) {
    return 0;
  }

  if (file.type.startsWith('video/')) {
    return 1;
  }

  return null;
}

export function ExerciseTechniqueMediaDialog({
  open,
  exercise,
  loading,
  error,
  onClose,
  onUploaded,
  onLoadingChange,
  onErrorChange,
}: ExerciseTechniqueMediaDialogProps) {
  const { t } = useTranslation();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [title, setTitle] = useState('');

  const currentImages = useMemo(() => exercise?.images ?? [], [exercise]);
  const currentVideos = useMemo(() => exercise?.videos ?? [], [exercise]);

  const resetLocalState = () => {
    setSelectedFile(null);
    setTitle('');
    onErrorChange(null);
  };

  const handleClose = () => {
    if (loading) {
      return;
    }

    resetLocalState();
    onClose();
  };

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    setSelectedFile(file);
    onErrorChange(null);

    if (file && !title.trim()) {
      const dotIndex = file.name.lastIndexOf('.');
      setTitle(dotIndex > 0 ? file.name.slice(0, dotIndex) : file.name);
    }
  };

  const handleSubmit = async () => {
    if (!exercise || !selectedFile) {
      return;
    }

    const mediaType = detectMediaType(selectedFile);
    if (mediaType === null) {
      onErrorChange(t('administration.exercises.media.unsupportedFileType'));
      return;
    }

    onLoadingChange(true);
    onErrorChange(null);

    try {
      const uploadTicket = await requestExerciseUploadUrl(exercise.id, {
        mediaType,
        fileName: selectedFile.name,
        contentType: selectedFile.type || 'application/octet-stream',
        sizeBytes: selectedFile.size,
        title: title.trim() || selectedFile.name,
      });

      if (!uploadTicket.uploadUrl.includes('dev_signed=true')) {
        const uploadResponse = await fetch(uploadTicket.uploadUrl, {
          method: 'PUT',
          headers: {
            'Content-Type': selectedFile.type || 'application/octet-stream',
          },
          body: selectedFile,
        });

        if (!uploadResponse.ok) {
          throw new Error(t('administration.exercises.media.uploadTransferError'));
        }
      }

      const currentCount = mediaType === 0 ? currentImages.length : currentVideos.length;
      await confirmExerciseMedia(exercise.id, {
        mediaId: uploadTicket.mediaId,
        mediaType,
        storagePath: uploadTicket.storagePath,
        thumbnailPath: null,
        contentType: selectedFile.type || 'application/octet-stream',
        fileName: selectedFile.name,
        sizeBytes: selectedFile.size,
        title: title.trim() || selectedFile.name,
        sortOrder: currentCount,
        isPrimary: currentCount === 0,
      });

      await onUploaded();
      resetLocalState();
      onClose();
    } catch (submitError) {
      const message = submitError instanceof Error
        ? submitError.message
        : t('administration.exercises.media.uploadError');
      onErrorChange(message);
    } finally {
      onLoadingChange(false);
    }
  };

  return (
    <PopupDialog
      open={open}
      title={exercise ? t('administration.exercises.media.title', { name: exercise.name }) : t('administration.exercises.media.genericTitle')}
      onClose={handleClose}
      onSubmit={() => {
        void handleSubmit();
      }}
      closeLabel={t('common.actions.cancel')}
      saveLabel={t('administration.exercises.media.saveAction')}
      disableSave={loading || !exercise || !selectedFile}
      isSaving={loading}
      maxWidth="md"
    >
      <Stack spacing={2} sx={{ mt: 1 }}>
        <Typography variant="body2" color="text.secondary">
          {t('administration.exercises.media.description')}
        </Typography>

        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <Stack spacing={1.5} sx={{ flex: 1 }}>
            <Typography variant="subtitle2">{t('administration.exercises.media.currentImages')}</Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              {currentImages.length > 0
                ? currentImages.map((item) => <Chip key={item} size="small" label={item.split('/').pop() ?? item} />)
                : <Typography variant="body2" color="text.secondary">{t('administration.exercises.media.emptyImages')}</Typography>}
            </Stack>
          </Stack>
          <Stack spacing={1.5} sx={{ flex: 1 }}>
            <Typography variant="subtitle2">{t('administration.exercises.media.currentVideos')}</Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              {currentVideos.length > 0
                ? currentVideos.map((item) => <Chip key={item} size="small" label={item.split('/').pop() ?? item} />)
                : <Typography variant="body2" color="text.secondary">{t('administration.exercises.media.emptyVideos')}</Typography>}
            </Stack>
          </Stack>
        </Stack>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }}>
          <Button variant="outlined" component="label">
            {t('administration.exercises.media.selectFileAction')}
            <input
              hidden
              type="file"
              accept="image/*,video/*"
              onChange={handleFileChange}
            />
          </Button>
          <Typography variant="body2" color="text.secondary">
            {selectedFile?.name ?? t('administration.exercises.media.noFileSelected')}
          </Typography>
        </Stack>

        <TextField
          label={t('administration.exercises.media.titleField')}
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          fullWidth
        />

        {selectedFile ? (
          <Alert severity="info">
            {t('administration.exercises.media.selectedSummary', {
              type: detectMediaType(selectedFile) === 1 ? t('administration.exercises.media.videoLabel') : t('administration.exercises.media.imageLabel'),
              size: Math.ceil(selectedFile.size / 1024),
            })}
          </Alert>
        ) : null}

        {error ? <Alert severity="error">{error}</Alert> : null}
      </Stack>
    </PopupDialog>
  );
}
