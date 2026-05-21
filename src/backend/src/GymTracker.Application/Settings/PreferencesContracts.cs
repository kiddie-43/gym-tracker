namespace GymTracker.Application.Settings;

public sealed record UpdatePreferencesRequest(bool CalorieTrackingEnabled, int? DailyCalorieGoal);

public sealed record PreferencesResponse(bool CalorieTrackingEnabled, int? DailyCalorieGoal);
