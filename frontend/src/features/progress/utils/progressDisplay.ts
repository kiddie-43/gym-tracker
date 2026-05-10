import type { WorkoutProgress } from '../../../shared/types/workouts';

export function getTrendLabel(trend: WorkoutProgress['trend']) {
  switch (trend) {
    case 'Improving':
      return 'Mejorando';
    case 'Declining':
      return 'Regresion detectada';
    case 'IncompleteSession':
      return 'Sesion incompleta';
    case 'NoReference':
      return 'Todavia sin referencia';
    default:
      return 'Estable';
  }
}

export function formatPercentage(value: number) {
  return `${value > 0 ? '+' : ''}${value.toFixed(2)}%`;
}
