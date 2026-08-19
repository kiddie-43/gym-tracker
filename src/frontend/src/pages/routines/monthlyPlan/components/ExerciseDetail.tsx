import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Alert from '@mui/material/Alert';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import Box from '@mui/material/Box';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import AddIcon from '@mui/icons-material/Add';
import HistoryIcon from '@mui/icons-material/History';
import LightbulbOutlinedIcon from '@mui/icons-material/LightbulbOutlined';

import type { IExercise } from '../../../../interfaces/IExercises/IExercises';
import type {
  IBlockTemplateItem,
  ICreateSessionBlockRequest,
  ICreateTrainingSessionRequest,
  ITrainingSession,
} from '../../../../interfaces/trainingSessions/trainingSessions';
import { ExerciseCategory } from '../../../../components/ExerciceCategory/ExerciceCategory';
import { resolveExerciseProfile } from '../../../../utils/exerciseProfile';
import { EditQuickSummaryDialog, type QuickSummaryDraft } from './EditQuickSummaryDialog/EditQuickSummaryDialog';
import { PopupDialog } from '../../../../components/PopupDialog/PopupDialog';
import { QuickSummaryCard } from './QuickSummaryCard/QuickSummaryCard';
import { SessionBlockFormDialog } from './SessionBlockFormDialog/SessionBlockFormDialog';
import { SessionBlocksList } from './SessionBlocksList/SessionBlocksList';
import { SessionHistoryList } from './SessionHistoryList/SessionHistoryList';

const NOTES_MAX_LENGTH = 150;
const DEFAULT_DURATION_UNIT_CODE = 'MIN';

interface ExerciseDetailProps {
  exercise: IExercise;
  form: ICreateTrainingSessionRequest;
  history: ITrainingSession[];
  loading: boolean;
  error: string | null;
  onFormChange: (form: ICreateTrainingSessionRequest) => void;
  onSubmit: () => void;
  onLoadBlockTemplate?: () => Promise<IBlockTemplateItem[]>;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`exercise-tabpanel-${index}`}
      aria-labelledby={`exercise-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 2 }}>{children}</Box>}
    </div>
  );
}

export function ExerciseDetail({ exercise, form, history, loading, error, onFormChange, onSubmit, onLoadBlockTemplate }: ExerciseDetailProps) {
  const { t } = useTranslation();
  const [tabValue, setTabValue] = useState(0);
  const [blockDialogOpen, setBlockDialogOpen] = useState(false);
  const [editingBlockIndex, setEditingBlockIndex] = useState<number | null>(null);
  const [summaryDialogOpen, setSummaryDialogOpen] = useState(false);
  const [templateConfirmOpen, setTemplateConfirmOpen] = useState(false);

  const profile = resolveExerciseProfile(exercise.exerciseType);
  const selfManagementTips = t('monthlyPlan.selfManagement.tips', { returnObjects: true }) as string[];

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleOpenAddBlock = () => {
    setEditingBlockIndex(null);
    setBlockDialogOpen(true);
  };

  const handleOpenEditBlock = (index: number) => {
    setEditingBlockIndex(index);
    setBlockDialogOpen(true);
  };

  const handleCloseBlockDialog = () => {
    setBlockDialogOpen(false);
    setEditingBlockIndex(null);
  };

  const handleSaveBlock = (block: ICreateSessionBlockRequest) => {
    const blocks = [...form.blocks];
    if (editingBlockIndex !== null) {
      blocks[editingBlockIndex] = block;
    } else {
      blocks.push({ ...block, orderIndex: blocks.length });
    }

    onFormChange({
      ...form,
      blocks,
      tertiaryMetricValue: profile === 'strength' ? blocks.length : form.tertiaryMetricValue,
    });
    handleCloseBlockDialog();
  };

  const handleDeleteBlock = (index: number) => {
    const blocks = form.blocks.filter((_, blockIndex) => blockIndex !== index);

    onFormChange({
      ...form,
      blocks,
      tertiaryMetricValue: profile === 'strength' ? blocks.length : form.tertiaryMetricValue,
    });
  };

  const applyTemplate = async () => {
    if (!onLoadBlockTemplate) return;

    const template = await onLoadBlockTemplate();
    const blocks: ICreateSessionBlockRequest[] = template.map((item) => ({
      blockType: item.blockType,
      name: item.name,
      durationValue: item.defaultDurationMinutes,
      durationUnitCode: DEFAULT_DURATION_UNIT_CODE,
      description: null,
      intensityRpe: null,
      orderIndex: item.orderIndex,
    }));

    onFormChange({
      ...form,
      blocks,
      tertiaryMetricValue: profile === 'strength' ? blocks.length : form.tertiaryMetricValue,
    });
  };

  const handleLoadTemplate = () => {
    if (form.blocks.length > 0) {
      setTemplateConfirmOpen(true);
      return;
    }
    void applyTemplate();
  };

  const handleConfirmLoadTemplate = () => {
    setTemplateConfirmOpen(false);
    void applyTemplate();
  };

  const handleNotesChange = (value: string) => {
    onFormChange({ ...form, notes: value.slice(0, NOTES_MAX_LENGTH) });
  };

  const handleSaveSummary = (draft: QuickSummaryDraft) => {
    onFormChange({ ...form, ...draft });
    setSummaryDialogOpen(false);
  };

  return (
    <Stack spacing={0} sx={{ minHeight: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Video Header Section */}
      <Box
        sx={{
          position: 'relative',
          width: '100%',
          height: 180,
          flexShrink: 0,
          bgcolor: 'action.hover',
          borderRadius: 2,
          overflow: 'hidden',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
        }}
      >
        {/* Video Placeholder */}
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            bgcolor: 'rgba(0, 0, 0, 0.3)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            '&:hover': {
              bgcolor: 'rgba(0, 0, 0, 0.5)',
            },
          }}
        >
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: 2,
            }}
          >
            <PlayArrowIcon
              sx={{
                fontSize: 64,
                color: 'white',
                opacity: 0.7,
              }}
            />
            <Typography sx={{ color: 'white', opacity: 0.7 }}>
              {t('monthlyPlan.exerciseVideo') || 'Video del ejercicio'}
            </Typography>
          </Box>
        </Box>

        {/* Exercise Info Overlay */}
        <Box
          sx={{
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            p: 2,
          }}
        >
          <Stack spacing={1.5}>
            <Typography
              variant="h5"
              sx={{
                color: 'white',
                fontWeight: 600,
              }}
            >
              {exercise.name}
            </Typography>

            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ flexWrap: 'wrap' }}>
              {exercise.exerciseType && (
                <ExerciseCategory exerciseType={exercise.exerciseType} />
              )}

              {exercise.primaryMuscles && exercise.primaryMuscles.length > 0 && (
                <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
                  {exercise.primaryMuscles.map((muscle) => (
                    <Box
                      key={muscle.id || muscle.name}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 0.5,
                        px: 1,
                        py: 0.5,
                        bgcolor: 'rgba(255, 255, 255, 0.2)',
                        borderRadius: 1,
                        color: 'white',
                      }}
                    >
                      <FitnessCenterOutlinedIcon sx={{ fontSize: 14 }} />
                      <Typography variant="caption">{muscle.name}</Typography>
                    </Box>
                  ))}
                </Stack>
              )}
            </Stack>
          </Stack>
        </Box>
      </Box>

      {/* Tabs Section */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2, flexShrink: 0 }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          aria-label="exercise tabs"
          sx={{
            '& .MuiTab-root': {
              textTransform: 'none',
              fontSize: '0.95rem',
            },
          }}
        >
          <Tab label={t('monthlyPlan.history') || 'Historial'} id="exercise-tab-0" />
          <Tab label={t('monthlyPlan.record') || 'Registrar'} id="exercise-tab-1" />
        </Tabs>
      </Box>

      {/* Tab Content */}
      <Box sx={{ mb: 3, flex: 1, minHeight: 0, overflowY: 'auto' }}>
        <TabPanel value={tabValue} index={0}>
          <SessionHistoryList sessions={history} />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Stack spacing={2.5}>
            {error ? <Alert severity="error">{error}</Alert> : null}

            <QuickSummaryCard
              profile={profile}
              durationMinutes={form.durationMinutes}
              secondaryMetricValue={form.secondaryMetricValue}
              tertiaryMetricValue={form.tertiaryMetricValue}
              onEditTarget={() => setSummaryDialogOpen(true)}
            />

            <Stack spacing={1}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle2">
                  {t('monthlyPlan.blocksTitle') || 'Estructura del entrenamiento'}
                </Typography>
                {onLoadBlockTemplate ? (
                  <Button size="small" onClick={handleLoadTemplate}>
                    {t('monthlyPlan.viewTemplates') || 'Ver plantillas'}
                  </Button>
                ) : null}
              </Stack>
              <SessionBlocksList
                blocks={form.blocks}
                onEdit={handleOpenEditBlock}
                onDelete={handleDeleteBlock}
              />
            </Stack>

            <Button
              variant="outlined"
              color="primary"
              startIcon={<AddIcon />}
              onClick={handleOpenAddBlock}
              sx={{ alignSelf: 'flex-start' }}
            >
              {t('monthlyPlan.addBlock') || '+ Añadir bloque'}
            </Button>

            <TextField
              label={t('monthlyPlan.notesLabel') || 'Notas (opcional)'}
              value={form.notes ?? ''}
              onChange={(event) => handleNotesChange(event.target.value)}
              helperText={`${(form.notes ?? '').length}/${NOTES_MAX_LENGTH}`}
              multiline
              minRows={2}
              fullWidth
            />

            <Stack
              spacing={1}
              sx={{ p: 2, borderRadius: 2, bgcolor: 'action.hover' }}
            >
              <Stack direction="row" spacing={1} alignItems="center">
                <LightbulbOutlinedIcon color="primary" fontSize="small" />
                <Typography variant="subtitle2">
                  {t('monthlyPlan.selfManagement.title') || 'Autogestión eficiente'}
                </Typography>
              </Stack>
              <Stack component="ul" spacing={0.5} sx={{ m: 0, pl: 3 }}>
                {selfManagementTips.map((tip) => (
                  <Typography key={tip} component="li" variant="body2" color="text.secondary">
                    {tip}
                  </Typography>
                ))}
              </Stack>
            </Stack>
          </Stack>
        </TabPanel>
      </Box>

      {/* Action Buttons */}
      <Stack
        direction="row"
        spacing={2}
        alignItems="center"
        sx={{
          position: 'sticky',
          bottom: 0,
          mt: 'auto',
          py: 2,
          borderTop: 1,
          borderColor: 'divider',
          bgcolor: 'background.default',
          flexShrink: 0,
        }}
      >
        <Button
          variant="contained"
          color="primary"
          fullWidth
          disabled={loading}
          onClick={onSubmit}
        >
          {t('monthlyPlan.saveSession') || 'Guardar entrenamiento'}
        </Button>
        <IconButton
          aria-label={t('monthlyPlan.history') || 'Historial'}
          onClick={() => setTabValue(0)}
          sx={{
            border: 1,
            borderColor: 'divider',
            borderRadius: '50%',
            flexShrink: 0,
          }}
        >
          <HistoryIcon />
        </IconButton>
      </Stack>

      <SessionBlockFormDialog
        open={blockDialogOpen}
        profile={profile}
        initialBlock={editingBlockIndex !== null ? form.blocks[editingBlockIndex] : null}
        onClose={handleCloseBlockDialog}
        onSave={handleSaveBlock}
      />

      <EditQuickSummaryDialog
        open={summaryDialogOpen}
        profile={profile}
        value={{
          durationMinutes: form.durationMinutes,
          secondaryMetricValue: form.secondaryMetricValue,
          tertiaryMetricValue: form.tertiaryMetricValue,
        }}
        onClose={() => setSummaryDialogOpen(false)}
        onSave={handleSaveSummary}
      />

      <PopupDialog
        open={templateConfirmOpen}
        title={t('monthlyPlan.templateConfirm.title') || 'Reemplazar bloques'}
        onClose={() => setTemplateConfirmOpen(false)}
        onSubmit={handleConfirmLoadTemplate}
        closeLabel={t('common.actions.cancel') || 'Cancelar'}
        saveLabel={t('common.actions.confirm') || 'Reemplazar'}
      >
        <Typography>
          {t('monthlyPlan.templateConfirm.message') || 'Ya tienes bloques creados. ¿Quieres reemplazarlos por la plantilla?'}
        </Typography>
      </PopupDialog>
    </Stack>
  );
}
