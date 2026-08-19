namespace GymTracker.Domain.Entities;

using System.Collections.ObjectModel;
using GymTracker.Domain.Common;

public sealed class TrainingSession : AuditableEntity
{
    public Guid UserId { get; private set; }

    public int WeekNumber { get; private set; }

    public int DayNumber { get; private set; }

    public Guid ExerciseId { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public decimal DurationMinutes { get; private set; }

    public decimal SecondaryMetricValue { get; private set; }

    public string SecondaryMetricUnitCode { get; private set; } = string.Empty;

    public decimal TertiaryMetricValue { get; private set; }

    public string? Notes { get; private set; }

    public Collection<SessionBlock> Blocks { get; private set; } = new();

    private TrainingSession()
    {
    }

    public static TrainingSession Create(
        Guid userId,
        int weekNumber,
        int dayNumber,
        Guid exerciseId,
        DateTimeOffset timestamp,
        decimal durationMinutes,
        decimal secondaryMetricValue,
        string secondaryMetricUnitCode,
        decimal tertiaryMetricValue,
        string? notes)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId is required.", nameof(exerciseId));

        if (weekNumber is < 1 or > 4)
            throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));

        if (dayNumber is < 1 or > 7)
            throw new ArgumentException("DayNumber must be between 1 and 7.", nameof(dayNumber));

        if (durationMinutes < 0)
            throw new ArgumentException("DurationMinutes cannot be negative.", nameof(durationMinutes));

        if (secondaryMetricValue < 0)
            throw new ArgumentException("SecondaryMetricValue cannot be negative.", nameof(secondaryMetricValue));

        if (string.IsNullOrWhiteSpace(secondaryMetricUnitCode))
            throw new ArgumentException("SecondaryMetricUnitCode is required.", nameof(secondaryMetricUnitCode));

        if (tertiaryMetricValue < 0)
            throw new ArgumentException("TertiaryMetricValue cannot be negative.", nameof(tertiaryMetricValue));

        return new TrainingSession
        {
            UserId = userId,
            WeekNumber = weekNumber,
            DayNumber = dayNumber,
            ExerciseId = exerciseId,
            Timestamp = timestamp,
            DurationMinutes = durationMinutes,
            SecondaryMetricValue = secondaryMetricValue,
            SecondaryMetricUnitCode = secondaryMetricUnitCode.Trim(),
            TertiaryMetricValue = tertiaryMetricValue,
            Notes = NormalizeNotes(notes),
        };
    }

    public void AddBlock(SessionBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        Blocks.Add(block);
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        var trimmed = notes.Trim();

        if (trimmed.Length > 150)
            throw new ArgumentException("Notes cannot exceed 150 characters.", nameof(notes));

        return trimmed;
    }
}
