using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities;

public sealed class HistoricalExerciseRecord : AuditableEntity
{
    public Guid SourcePlannedExerciseId { get; private set; }
    public Guid UserId { get; private set; }
    public int WeekNumber { get; private set; }
    public int DayNumber { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset RecordedAt { get; private set; }
    public string SourceReason { get; private set; } = string.Empty;

    public static HistoricalExerciseRecord Create(Guid sourcePlannedExerciseId, Guid userId, int weekNumber, int dayNumber, string payload, string sourceReason)
    {
        if (sourcePlannedExerciseId == Guid.Empty) throw new ArgumentException("SourcePlannedExerciseId is required.", nameof(sourcePlannedExerciseId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (weekNumber < 1 || weekNumber > 4) throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));
        if (dayNumber < 1 || dayNumber > 7) throw new ArgumentException("DayNumber must be between 1 and 7.", nameof(dayNumber));
        if (string.IsNullOrWhiteSpace(sourceReason)) throw new ArgumentException("SourceReason is required.", nameof(sourceReason));

        return new HistoricalExerciseRecord
        {
            SourcePlannedExerciseId = sourcePlannedExerciseId,
            UserId = userId,
            WeekNumber = weekNumber,
            DayNumber = dayNumber,
            Payload = payload ?? "{}",
            SourceReason = sourceReason.Trim(),
            RecordedAt = DateTimeOffset.UtcNow,
        };
    }
}
