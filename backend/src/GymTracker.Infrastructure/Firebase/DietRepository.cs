using GymTracker.Application.Diets;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class DietRepository : UserScopedRepository<Diet>, IDietRepository
{
    public DietRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "diets")
    {
    }

    public Task AddAsync(Diet diet, CancellationToken cancellationToken = default)
    {
        return SaveAsync(diet.UserId, diet, cancellationToken);
    }

    public Task<IReadOnlyCollection<Diet>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return ListAsync(userId, cancellationToken);
    }

    public Task<Diet?> GetByUserAndIdAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        return GetAsync(userId, dietId, cancellationToken);
    }

    public Task UpdateAsync(Diet diet, CancellationToken cancellationToken = default)
    {
        diet.Touch();
        return SaveAsync(diet.UserId, diet, cancellationToken);
    }

    public new Task DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync(userId, dietId, cancellationToken);
    }
}
