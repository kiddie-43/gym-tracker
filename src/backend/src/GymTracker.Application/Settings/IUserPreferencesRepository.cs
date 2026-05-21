using GymTracker.Domain.Entities;

namespace GymTracker.Application.Settings;

public interface IUserPreferencesRepository
{
    Task<UserPreferences?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default);
}
