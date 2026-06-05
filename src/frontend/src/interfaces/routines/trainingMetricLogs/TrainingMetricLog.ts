export interface TrainingMetricLog {
  logId: string;
  userId: string;
  routineId: string;
  sessionId: string;
  trainingId: string;
  exerciseId: string;
  metricId: string;
  groupId: string;
  date: string;
  value: number;
  createdAt: string;
  updatedAt: string;
}

export interface TrainingMetricLogsPageResponse {
  items: TrainingMetricLog[];
  total: number;
  page: number;
  pageSize: number;
}

export interface MetricDefinition {
  metricId: string;
  name: string;
  dataType: string;
  unit?: string | null;
}

export interface GroupMetricValueInput {
  metricId: string;
  value: number;
}

export interface CreateTrainingMetricGroupRequest {
  routineId: string;
  sessionId: string;
  trainingId: string;
  exerciseId: string;
  date: string;
  metrics: GroupMetricValueInput[];
}

export interface TrainingMetricGroupResponse {
  groupId: string;
  items: TrainingMetricLog[];
}

export interface UpdateMetricValueRequest {
  value: number;
}

export interface TrainingMetricLogsQuery {
  routineId?: string;
  sessionId?: string;
  trainingId?: string;
  exerciseId?: string;
  metricId?: string;
  page?: number;
  pageSize?: number;
}

export interface AvailableMetricsResponse {
  metrics: MetricDefinition[];
  degraded: boolean;
}