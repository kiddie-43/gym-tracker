namespace GymTracker.Application.TrainingSession;

using GymTracker.Domain.Entities;

public interface ITrainingSessionRepository
{
    Task AddAsync(TrainingSession session, CancellationToken cancellationToken = default);

    Task<TrainingSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TrainingSession>> ListHistoryAsync(
        Guid userId,
        int weekNumber,
        int dayNumber,
        Guid exerciseId,
        CancellationToken cancellationToken = default);
}
