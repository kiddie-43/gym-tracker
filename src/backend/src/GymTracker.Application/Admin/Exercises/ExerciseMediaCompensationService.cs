namespace GymTracker.Application.Admin.Exercises;

public sealed class ExerciseMediaCompensationService
{
    private readonly IExerciseStorageService _storageService;

    public ExerciseMediaCompensationService(IExerciseStorageService storageService)
    {
        _storageService = storageService;
    }

    public Task CompensateDeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        return _storageService.DeleteAsync(storagePath, cancellationToken);
    }
}
