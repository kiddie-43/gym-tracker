namespace GymTracker.Domain.ValueObjects;

public enum ExerciseMediaType
{
    Image = 0,
    Video = 1,
}

public sealed record ExerciseMedia(
    string MediaId,
    ExerciseMediaType MediaType,
    string Title,
    string StoragePath,
    string? ThumbnailPath,
    string ContentType,
    string FileName,
    long SizeBytes,
    int SortOrder,
    bool IsPrimary,
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt)
{
    public static ExerciseMedia Create(
        string mediaId,
        ExerciseMediaType mediaType,
        string title,
        string storagePath,
        string? thumbnailPath,
        string contentType,
        string fileName,
        long sizeBytes,
        int sortOrder,
        bool isPrimary,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media id is required.", nameof(mediaId));
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new ArgumentException("Storage path is required.", nameof(storagePath));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentException("Sort order must be >= 0.", nameof(sortOrder));
        }

        return new ExerciseMedia(
            MediaId: mediaId.Trim(),
            MediaType: mediaType,
            Title: string.IsNullOrWhiteSpace(title) ? fileName : title.Trim(),
            StoragePath: storagePath.Trim(),
            ThumbnailPath: string.IsNullOrWhiteSpace(thumbnailPath) ? null : thumbnailPath.Trim(),
            ContentType: contentType.Trim(),
            FileName: fileName.Trim(),
            SizeBytes: sizeBytes,
            SortOrder: sortOrder,
            IsPrimary: isPrimary,
            Active: true,
            IsDeleted: false,
            CreatedAt: now,
            UpdatedAt: now,
            DeletedAt: null);
    }
}
