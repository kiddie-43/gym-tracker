using GymTracker.Application.Settings;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlUserPreferencesRepository : IUserPreferencesRepository
{
    public Task<UserPreferences?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
