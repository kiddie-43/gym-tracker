namespace GymTracker.Application.Admin.Exercises;

public interface IExerciseRelationsValidator
{
    Task ValidateAsync(UpsertExerciseRequest request, CancellationToken cancellationToken = default);
}