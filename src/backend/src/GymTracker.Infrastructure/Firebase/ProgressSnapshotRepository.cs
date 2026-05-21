using GymTracker.Application.Progress;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class ProgressSnapshotRepository : IProgressSnapshotRepository
{
    private const string ModuleName = "progressSnapshots";
    private readonly SqlDocumentStore _store;

    public ProgressSnapshotRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, snapshot.UserId, snapshot.Id, snapshot, cancellationToken);
    }

    public async Task<ProgressSnapshot?> GetLatestAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var snapshots = await _store.ListUserAsync<ProgressSnapshot>(ModuleName, userId, cancellationToken);

        return snapshots
            .Where(snapshot => string.Equals(snapshot.ExerciseId, exerciseId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(snapshot => snapshot.Result.CalculatedAt)
            .FirstOrDefault();
    }
}
