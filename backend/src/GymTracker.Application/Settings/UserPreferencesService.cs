using GymTracker.Domain.Entities;

namespace GymTracker.Application.Settings;

public sealed class UserPreferencesService
{
    private readonly IUserPreferencesRepository _userPreferencesRepository;

    public UserPreferencesService(IUserPreferencesRepository userPreferencesRepository)
    {
        _userPreferencesRepository = userPreferencesRepository;
    }

    public async Task<PreferencesResponse> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? new UserPreferences { UserId = userId, CalorieTrackingEnabled = false, DailyCalorieGoal = null };

        return new PreferencesResponse(preferences.CalorieTrackingEnabled, preferences.DailyCalorieGoal);
    }

    public async Task<PreferencesResponse> UpdateAsync(string userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var preferences = new UserPreferences
        {
            UserId = userId,
            CalorieTrackingEnabled = request.CalorieTrackingEnabled,
            DailyCalorieGoal = request.DailyCalorieGoal,
        };

        await _userPreferencesRepository.SaveAsync(preferences, cancellationToken);
        return new PreferencesResponse(preferences.CalorieTrackingEnabled, preferences.DailyCalorieGoal);
    }
}
