import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';

import type { RoutineSummary } from '../../../../shared/types/routines';

type StartFromRoutineDialogProps = {
  open: boolean;
  routines: RoutineSummary[];
  onPick: (routineId: string) => void;
  onClose: () => void;
};

export function StartFromRoutineDialog({ open, routines, onPick, onClose }: StartFromRoutineDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth>
      <DialogTitle>Comenzar desde rutina</DialogTitle>
      <DialogContent>
        <List>
          {routines.map((routine) => (
            <ListItemButton key={routine.id} onClick={() => onPick(routine.id)}>
              {routine.name}
            </ListItemButton>
          ))}
        </List>
      </DialogContent>
    </Dialog>
  );
}
