namespace GymTracker.Application.TrainingLog;

using GymTracker.Domain.Entities;

public interface ITrainingLogRepository
{
    Task CreateGroupAsync(
        IReadOnlyCollection<TrainingLog> logs,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(
TrainingLog log,
CancellationToken cancellationToken = default);

    Task<TrainingLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TrainingLog>> ListByGroupIdAsync(
        Guid groupId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TrainingLog>> ListAsync(
        Guid userId,
        int? weekNumber,
        int? dayNumber,
        string? exerciseCode,
        DateOnly? date,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(
        TrainingLog log,
        CancellationToken cancellationToken = default);
}