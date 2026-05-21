using GymTracker.Domain.Entities;

namespace GymTracker.Application.Progress;

public interface IProgressSnapshotRepository
{
    Task SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken = default);

    Task<ProgressSnapshot?> GetLatestAsync(string userId, string exerciseId, CancellationToken cancellationToken = default);
}
