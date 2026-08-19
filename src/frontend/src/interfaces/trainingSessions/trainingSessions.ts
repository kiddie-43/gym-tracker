export interface ISessionBlock {
  id: string;
  blockType: string;
  name: string;
  durationValue: number;
  durationUnitCode: string;
  description?: string | null;
  intensityRpe?: number | null;
  orderIndex: number;
}

export interface ITrainingSession {
  id: string;
  weekNumber: number;
  dayNumber: number;
  exerciseId: string;
  timestamp: string;
  durationMinutes: number;
  secondaryMetricValue: number;
  secondaryMetricUnitCode: string;
  tertiaryMetricValue: number;
  notes?: string | null;
  blocks: ISessionBlock[];
}

export interface ICreateSessionBlockRequest {
  blockType: string;
  name: string;
  durationValue: number;
  durationUnitCode: string;
  description?: string | null;
  intensityRpe?: number | null;
  orderIndex: number;
}

export interface ICreateTrainingSessionRequest {
  weekNumber: number;
  dayNumber: number;
  exerciseId: string;
  durationMinutes: number;
  secondaryMetricValue: number;
  secondaryMetricUnitCode: string;
  tertiaryMetricValue: number;
  notes?: string | null;
  blocks: ICreateSessionBlockRequest[];
}

export interface IBlockTemplateItem {
  blockType: string;
  name: string;
  defaultDurationMinutes: number;
  orderIndex: number;
}
