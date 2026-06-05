namespace GymTracker.Domain.Entities;
using GymTracker.Domain.Common;
public sealed class TrainingLog : AuditableEntity
{
    public Guid GroupId { get; private set; }

    public string RoutineId { get; private set; } = string.Empty;

    public string SessionId { get; private set; } = string.Empty;

    public Guid ExerciseId { get; private set; }

    public Guid MetricId { get; private set; }

    public decimal Value { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public Guid UserId { get; private set; }

    private TrainingLog()
    {
    }

    public static TrainingLog Create(
        string routineId,
        string sessionId,
        Guid exerciseId,
        Guid metricId,
        decimal value,
        DateTimeOffset timestamp,
        Guid userId,
        Guid? groupId = null)
    {
        if (string.IsNullOrWhiteSpace(routineId))
            throw new ArgumentException("RoutineId is required.", nameof(routineId));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId is required.", nameof(sessionId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId is required.", nameof(exerciseId));

        if (metricId == Guid.Empty)
            throw new ArgumentException("MetricId is required.", nameof(metricId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", nameof(value));

        return new TrainingLog
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            RoutineId = routineId,
            SessionId = sessionId,
            ExerciseId = exerciseId,
            MetricId = metricId,
            Value = value,
            Timestamp = timestamp,
            UserId = userId
        };
    }

    public void UpdateValue(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", nameof(value));

        Value = value;
        Timestamp = DateTimeOffset.UtcNow;
    }
}