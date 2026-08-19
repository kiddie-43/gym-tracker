import type { ReactNode } from 'react';
import WhatshotOutlinedIcon from '@mui/icons-material/WhatshotOutlined';
import TrendingUpOutlinedIcon from '@mui/icons-material/TrendingUpOutlined';
import FitnessCenterOutlinedIcon from '@mui/icons-material/FitnessCenterOutlined';
import SelfImprovementOutlinedIcon from '@mui/icons-material/SelfImprovementOutlined';
import PoolOutlinedIcon from '@mui/icons-material/PoolOutlined';
import RepeatOutlinedIcon from '@mui/icons-material/RepeatOutlined';
import SchoolOutlinedIcon from '@mui/icons-material/SchoolOutlined';

export interface SessionBlockTypeMeta {
  code: string;
  icon: ReactNode;
  color: string;
  labelKey: string;
  supportsIntensity: boolean;
}

export const STRENGTH_BLOCK_TYPES: SessionBlockTypeMeta[] = [
  {
    code: 'WARMUP',
    icon: <WhatshotOutlinedIcon fontSize="small" />,
    color: '#F59E0B',
    labelKey: 'monthlyPlan.blockTypes.WARMUP',
    supportsIntensity: false,
  },
  {
    code: 'APPROACH',
    icon: <TrendingUpOutlinedIcon fontSize="small" />,
    color: '#3B82F6',
    labelKey: 'monthlyPlan.blockTypes.APPROACH',
    supportsIntensity: false,
  },
  {
    code: 'WORK',
    icon: <FitnessCenterOutlinedIcon fontSize="small" />,
    color: '#EF4444',
    labelKey: 'monthlyPlan.blockTypes.WORK',
    supportsIntensity: false,
  },
  {
    code: 'REST_STRENGTH',
    icon: <SelfImprovementOutlinedIcon fontSize="small" />,
    color: '#10B981',
    labelKey: 'monthlyPlan.blockTypes.REST_STRENGTH',
    supportsIntensity: false,
  },
];

export const CARDIO_BLOCK_TYPES: SessionBlockTypeMeta[] = [
  {
    code: 'SWIM',
    icon: <PoolOutlinedIcon fontSize="small" />,
    color: '#0EA5E9',
    labelKey: 'monthlyPlan.blockTypes.SWIM',
    supportsIntensity: true,
  },
  {
    code: 'SERIES',
    icon: <RepeatOutlinedIcon fontSize="small" />,
    color: '#3B82F6',
    labelKey: 'monthlyPlan.blockTypes.SERIES',
    supportsIntensity: true,
  },
  {
    code: 'TECHNIQUE',
    icon: <SchoolOutlinedIcon fontSize="small" />,
    color: '#8B5CF6',
    labelKey: 'monthlyPlan.blockTypes.TECHNIQUE',
    supportsIntensity: true,
  },
  {
    code: 'REST_CARDIO',
    icon: <SelfImprovementOutlinedIcon fontSize="small" />,
    color: '#10B981',
    labelKey: 'monthlyPlan.blockTypes.REST_CARDIO',
    supportsIntensity: false,
  },
];

const ALL_BLOCK_TYPES = [...STRENGTH_BLOCK_TYPES, ...CARDIO_BLOCK_TYPES];

export function getBlockTypesForProfile(profile: 'strength' | 'cardio'): SessionBlockTypeMeta[] {
  return profile === 'cardio' ? CARDIO_BLOCK_TYPES : STRENGTH_BLOCK_TYPES;
}

export function findBlockTypeMeta(code: string): SessionBlockTypeMeta | undefined {
  return ALL_BLOCK_TYPES.find((type) => type.code === code);
}

export function blockTypeSupportsIntensity(code: string): boolean {
  return findBlockTypeMeta(code)?.supportsIntensity ?? false;
}
