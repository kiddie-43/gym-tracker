namespace GymTracker.Domain.Entities;

using GymTracker.Domain.Common;
using GymTracker.Domain.Enum;

public sealed class SessionBlock : AuditableEntity
{
    public Guid TrainingSessionId { get; private set; }

    public SessionBlockType BlockType { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal DurationValue { get; private set; }

    public string DurationUnitCode { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int? IntensityRpe { get; private set; }

    public int OrderIndex { get; private set; }

    private SessionBlock()
    {
    }

    public static bool SupportsIntensity(SessionBlockType blockType) =>
        blockType is SessionBlockType.SWIM or SessionBlockType.SERIES or SessionBlockType.TECHNIQUE;

    public static SessionBlock Create(
        Guid trainingSessionId,
        SessionBlockType blockType,
        string name,
        decimal durationValue,
        string durationUnitCode,
        string? description,
        int? intensityRpe,
        int orderIndex)
    {
        if (trainingSessionId == Guid.Empty)
            throw new ArgumentException("TrainingSessionId is required.", nameof(trainingSessionId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (durationValue <= 0)
            throw new ArgumentException("DurationValue must be greater than zero.", nameof(durationValue));

        if (string.IsNullOrWhiteSpace(durationUnitCode))
            throw new ArgumentException("DurationUnitCode is required.", nameof(durationUnitCode));

        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be >= 0.", nameof(orderIndex));

        return new SessionBlock
        {
            TrainingSessionId = trainingSessionId,
            BlockType = blockType,
            Name = name.Trim(),
            DurationValue = durationValue,
            DurationUnitCode = durationUnitCode.Trim(),
            Description = NormalizeDescription(description),
            IntensityRpe = NormalizeIntensityRpe(intensityRpe, blockType),
            OrderIndex = orderIndex,
        };
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > 100)
            throw new ArgumentException("Description cannot exceed 100 characters.", nameof(description));

        return trimmed;
    }

    private static int? NormalizeIntensityRpe(int? intensityRpe, SessionBlockType blockType)
    {
        if (!intensityRpe.HasValue)
            return null;

        // BR-005: discard intensity when the block type does not support it.
        if (!SupportsIntensity(blockType))
            return null;

        if (intensityRpe.Value is < 1 or > 10)
            throw new ArgumentException("IntensityRpe must be between 1 and 10.", nameof(intensityRpe));

        return intensityRpe;
    }
}
