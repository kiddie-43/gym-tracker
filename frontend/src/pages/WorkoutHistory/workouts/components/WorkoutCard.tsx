import Card from '@mui/material/Card';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Avatar from '@mui/material/Avatar';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';

import type { WorkoutSummary } from '../../../../shared/types/workouts';

type WorkoutCardProps = {
  workout: WorkoutSummary;
  onEdit: (workout: WorkoutSummary) => void;
  onDelete: (workout: WorkoutSummary) => void;
  formatDate: (date: string) => string;
};

function resolveImageUrl(imageUrl?: string | null): string | undefined {
  if (!imageUrl) {
    return undefined;
  }

  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl;
  }

  if (imageUrl.startsWith('/')) {
    return `https://wger.de${imageUrl}`;
  }

  return `https://wger.de/${imageUrl}`;
}

function resolveExerciseLabel(workout: WorkoutSummary): string {
  const snapshotName = workout.exerciseEntries?.[0]?.exerciseNameSnapshot?.trim();
  if (snapshotName) {
    return snapshotName;
  }

  const exerciseName = workout.exerciseEntries?.[0]?.exerciseName?.trim();
  if (exerciseName) {
    return exerciseName;
  }

  const fallbackName = workout.exerciseName?.trim();
  if (fallbackName) {
    return fallbackName;
  }

  return 'Sin ejercicio';
}

export function WorkoutCard({ workout, onEdit, onDelete, formatDate }: WorkoutCardProps) {
  const exerciseLabel = resolveExerciseLabel(workout);
  const exerciseImageUrl = resolveImageUrl(workout.exerciseEntries?.[0]?.imageUrl);

  return (
    <Card 
      variant="outlined"
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
        backgroundColor: '#fafafa',
        '&:hover': {
          boxShadow: 2,
        }
      }}
    >
      {/* Image Section */}
      <Box
        sx={{
          height: 150,
          display: 'flex',
          alignItems: 'flex-start',
          justifyContent: 'center',
          bgcolor: 'action.hover',
          overflow: 'hidden',
        }}
      >
        <Avatar
          src={exerciseImageUrl}
          alt={exerciseLabel}
          variant="rounded"
          sx={{ width: 80, height: 80, bgcolor: 'background.paper', color: 'text.secondary' }}
        >
          {exerciseLabel.slice(0, 2).toUpperCase()}
        </Avatar>
      </Box>

      {/* Content Section */}
      <Stack spacing={1} sx={{ flex: 1, p: 2 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          {exerciseLabel}
        </Typography>
        
        <Typography variant="body2" color="text.secondary">
          {formatDate(workout.performedAt)}
        </Typography>
        
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography variant="body2" sx={{ fontWeight: 500 }}>Estado:</Typography>
          <Box
            component="span"
            sx={{
              display: 'inline-block',
              px: 1,
              py: 0.5,
              borderRadius: '0.5rem',
              fontSize: '0.75rem',
              fontWeight: 600,
              backgroundColor: workout.status === 'completed' ? '#d4edda' : '#fff3cd',
              color: workout.status === 'completed' ? '#155724' : '#856404',
              textTransform: 'uppercase',
            }}
          >
            {workout.status === 'completed' ? 'Completado' : 'Incompleto'}
          </Box>
        </Stack>
        
        {workout.exerciseEntries && workout.exerciseEntries.length > 0 && (
          <Typography variant="caption" color="text.secondary">
            {workout.exerciseEntries.length} ejercicio{workout.exerciseEntries.length > 1 ? 's' : ''} - {workout.exerciseEntries[0]?.sets?.length ?? 0} series
          </Typography>
        )}
      </Stack>

      {/* Buttons Section */}
      <Stack 
        direction="row" 
        spacing={1} 
        sx={{ 
          p: 2, 
          pt: 1.5,
          borderTop: '1px solid rgba(0, 0, 0, 0.12)',
          justifyContent: 'center',
          gap: 1.5,
        }}
      >
        <Button
          variant="outlined"
          size="medium"
          startIcon={<EditRoundedIcon />}
          onClick={() => onEdit(workout)}
          sx={{ flex: 1 }}
        >
          Editar
        </Button>
        <Button
          variant="outlined"
          size="medium"
          color="error"
          startIcon={<DeleteOutlineRoundedIcon />}
          onClick={() => onDelete(workout)}
          sx={{ flex: 1 }}
        >
          Borrar
        </Button>
      </Stack>
    </Card>
  );
}
