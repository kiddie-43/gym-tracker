using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Admin.Exercises;

public sealed record RequestUploadUrlRequest(
    ExerciseMediaType MediaType,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Title = null);

public sealed record ConfirmExerciseMediaRequest(
    string MediaId,
    ExerciseMediaType MediaType,
    string StoragePath,
    string? ThumbnailPath,
    string ContentType,
    string FileName,
    long SizeBytes,
    string? Title,
    int SortOrder,
    bool IsPrimary);

public sealed record ReorderExerciseMediaItemRequest(string MediaId, int SortOrder);

public sealed record ReorderExerciseMediaRequest(IReadOnlyCollection<ReorderExerciseMediaItemRequest> Items);

public sealed record SetPrimaryExerciseMediaRequest(string MediaId);

public sealed record UpdateExerciseMediaRequest(string? Title, bool? Active);
