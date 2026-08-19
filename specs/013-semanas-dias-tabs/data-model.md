# Data Model: Configuracion Semanas y Dias

## Entity: MonthlyPlan
- Description: Configuracion mensual de entrenamiento de un usuario.
- Fields:
  - monthlyPlanId (string, UUID)
  - userId (string)
  - activeDays (int, range 1..7)
  - createdAt (datetime)
  - updatedAt (datetime)
  - migrationVersion (string)
- Validation rules:
  - activeDays MUST be >= 1 and <= 7.
  - userId MUST match authenticated owner.
  - Exactly 4 week nodes MUST exist (1..4).

## Entity: PlanWeek
- Description: Semana fija dentro del plan mensual.
- Fields:
  - monthlyPlanId (string, FK -> MonthlyPlan)
  - weekNumber (int, allowed: 1..4)
  - createdAt (datetime)
  - updatedAt (datetime)
- Validation rules:
  - weekNumber MUST be unique per monthlyPlanId.
  - weekNumber outside 1..4 MUST be rejected.

## Entity: PlanDay
- Description: Dia configurable dentro de una semana.
- Fields:
  - monthlyPlanId (string, FK -> MonthlyPlan)
  - weekNumber (int, FK -> PlanWeek.weekNumber)
  - dayNumber (int, allowed: 1..activeDays)
  - status (enum: active, truncated)
  - truncatedAt (datetime, nullable)
- Validation rules:
  - dayNumber MUST be <= activeDays vigente del MonthlyPlan.
  - Duplicated (weekNumber, dayNumber) per monthlyPlanId MUST be rejected.

## Entity: PlannedExercise
- Description: Ejercicio configurado en una combinacion Semana > Dia del plan activo.
- Fields:
  - plannedExerciseId (string, UUID)
  - monthlyPlanId (string, FK -> MonthlyPlan)
  - weekNumber (int)
  - dayNumber (int)
  - exerciseId (string)
  - orderIndex (int)
  - notes (string, nullable)
  - createdAt (datetime)
  - updatedAt (datetime)
- Validation rules:
  - (weekNumber, dayNumber) MUST exist and be active.
  - exerciseId MUST reference valid catalog entry.
  - orderIndex MUST be >= 0.

## Entity: HistoricalExerciseRecord
- Description: Registro historico preservado para estadisticas cuando el plan activo se recorta.
- Fields:
  - historicalRecordId (string, UUID)
  - sourcePlannedExerciseId (string)
  - userId (string)
  - weekNumber (int)
  - dayNumber (int)
  - payload (object)
  - recordedAt (datetime)
  - sourceReason (enum: truncation, completion, migration)
- Validation rules:
  - Historical records MUST be immutable.
  - sourceReason MUST be provided.

## Entity: MigrationAudit
- Description: Trazabilidad de conversion de Rutina/Sesion a Semana/Dia.
- Fields:
  - migrationAuditId (string, UUID)
  - userId (string)
  - legacyRoutineId (string, nullable)
  - legacySessionId (string, nullable)
  - mappedWeekNumber (int)
  - mappedDayNumber (int)
  - migratedAt (datetime)
  - migratedBy (string)
- Validation rules:
  - mappedWeekNumber MUST be 1..4.
  - mappedDayNumber MUST be 1..7.
  - Every migrated legacy record MUST produce one audit entry.

## Relationships
- MonthlyPlan 1 -> N PlanWeek
- PlanWeek 1 -> N PlanDay
- PlanDay 1 -> N PlannedExercise
- PlannedExercise 1 -> 0..1 HistoricalExerciseRecord (when truncated/migrated/completed snapshot)
- MonthlyPlan 1 -> N MigrationAudit

## State Transitions
- MonthlyPlan:
  - draft -> active (first persisted with valid 4 weeks and activeDays)
  - active -> active (updates in days/exercises)
  - active -> migrated (legacy data conversion completed)
- PlanDay:
  - active -> truncated (when activeDays is reduced below dayNumber)
  - truncated -> active (only if activeDays increases and day is restored)

## Truncation Rule
- When activeDays decreases from N to M:
  - For each dayNumber where dayNumber > M:
    - PlannedExercise entries are removed from active planning view.
    - Statistical history is preserved as HistoricalExerciseRecord.
    - Day status becomes truncated.
