namespace GymTracker.Domain.Entities.Training;

public sealed class TrainingAttachment
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Type { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
}
