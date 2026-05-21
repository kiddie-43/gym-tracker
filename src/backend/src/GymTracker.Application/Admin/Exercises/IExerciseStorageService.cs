namespace GymTracker.Application.Admin.Exercises;

public interface IExerciseStorageService
{
    Task<string> GenerateUploadUrlAsync(
        string storagePath,
        TimeSpan expiresIn,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
