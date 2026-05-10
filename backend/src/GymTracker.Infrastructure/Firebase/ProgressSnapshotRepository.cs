using GymTracker.Application.Progress;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class ProgressSnapshotRepository : UserScopedRepository<ProgressSnapshot>, IProgressSnapshotRepository
{
    public ProgressSnapshotRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "progressSnapshots")
    {
    }

    public Task SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return base.SaveAsync(snapshot.UserId, snapshot, cancellationToken);
    }

    public async Task<ProgressSnapshot?> GetLatestAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var snapshots = await ListAsync(userId, cancellationToken);

        return snapshots
            .Where(snapshot => string.Equals(snapshot.ExerciseId, exerciseId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(snapshot => snapshot.Result.CalculatedAt)
            .FirstOrDefault();
    }
}
