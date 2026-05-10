using GymTracker.Application.Settings;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class UserPreferencesRepository : UserScopedRepository<UserPreferences>, IUserPreferencesRepository
{
    private const string PreferencesId = "preferences";

    public UserPreferencesRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "settings")
    {
    }

    public Task<UserPreferences?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return GetAsync(userId, PreferencesId, cancellationToken);
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

        return base.SaveAsync(preferences.UserId, entity, cancellationToken);
    }
}
