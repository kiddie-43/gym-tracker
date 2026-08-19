namespace GymTracker.Domain.Entities;
using GymTracker.Domain.Common;
public sealed class TrainingLog : AuditableEntity
{
    public Guid GroupId { get; private set; }

    public int WeekNumber { get; private set; }

    public int DayNumber { get; private set; }

    public string ExerciseCode { get; private set; } = string.Empty;

    public string UnitCode { get; private set; } = string.Empty;

    public decimal Value { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public Guid UserId { get; private set; }

    private TrainingLog()
    {
    }

    public static TrainingLog Create(
        int weekNumber,
        int dayNumber,
        string exerciseCode,
        string unitCode,
        decimal value,
        DateTimeOffset timestamp,
        Guid userId,
        Guid? groupId = null)
    {
        if (weekNumber is < 1 or > 4)
            throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));

        if (dayNumber is < 1 or > 7)
            throw new ArgumentException("DayNumber must be between 1 and 7.", nameof(dayNumber));

        if (string.IsNullOrWhiteSpace(exerciseCode))
            throw new ArgumentException("ExerciseCode is required.", nameof(exerciseCode));

        if (string.IsNullOrWhiteSpace(unitCode))
            throw new ArgumentException("UnitCode is required.", nameof(unitCode));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", nameof(value));

        return new TrainingLog
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            WeekNumber = weekNumber,
            DayNumber = dayNumber,
            ExerciseCode = exerciseCode,
            UnitCode = unitCode,
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