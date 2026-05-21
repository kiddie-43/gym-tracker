using GymTracker.Application.Settings;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class UserPreferencesRepository : IUserPreferencesRepository
{
    private const string PreferencesId = "preferences";
    private const string ModuleName = "settings";

    private readonly SqlDocumentStore _store;

    public UserPreferencesRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task<UserPreferences?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<UserPreferences>(ModuleName, userId, PreferencesId, cancellationToken);
    }

    public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        var entity = new UserPreferences
        {
            Id = PreferencesId,
            UserId = preferences.UserId,
            CalorieTrackingEnabled = preferences.CalorieTrackingEnabled,
            DailyCalorieGoal = preferences.DailyCalorieGoal,
        };

        return _store.UpsertUserAsync(ModuleName, preferences.UserId, PreferencesId, entity, cancellationToken);
    }
}
