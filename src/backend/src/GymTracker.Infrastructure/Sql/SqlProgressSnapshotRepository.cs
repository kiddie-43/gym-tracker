using GymTracker.Application.Progress;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlProgressSnapshotRepository : IProgressSnapshotRepository
{
    public Task SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<ProgressSnapshot?> GetLatestAsync(string userId, string exerciseId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
