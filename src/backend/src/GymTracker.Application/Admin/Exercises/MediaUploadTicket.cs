namespace GymTracker.Application.Admin.Exercises;

public sealed record MediaUploadTicket(
    string ExerciseId,
    string MediaId,
    string StoragePath,
    string UploadUrl,
    DateTimeOffset ExpiresAt,
    string ContentType,
    long MaxSizeBytes);
