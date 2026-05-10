using GymTracker.Domain.Entities;

namespace GymTracker.Application.Meals;

public sealed class MealLogService
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly Settings.IUserPreferencesRepository _userPreferencesRepository;

    public MealLogService(IMealLogRepository mealLogRepository, Settings.IUserPreferencesRepository userPreferencesRepository)
    {
        _mealLogRepository = mealLogRepository;
        _userPreferencesRepository = userPreferencesRepository;
    }

    public async Task<MealLogResponse> CreateAsync(string userId, CreateMealLogRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new ArgumentException("At least one meal item is required.");
        }

        var mealLog = new MealLog
        {
            UserId = userId,
            LoggedDate = request.LoggedDate,
            SlotType = request.SlotType,
            Items = request.Items.Select(item => new MealItem
            {
                ExternalFoodId = item.ExternalFoodId,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Calories = item.Calories,
            }).ToArray(),
        };

        await _mealLogRepository.AddAsync(mealLog, cancellationToken);

        var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? new Domain.Entities.UserPreferences { UserId = userId, CalorieTrackingEnabled = false, DailyCalorieGoal = null };

        return new MealLogResponse(
            mealLog.Id,
            mealLog.LoggedDate,
            mealLog.SlotType,
            request.Items,
            CalorieTrackingRules.ResolveTotalCalories(preferences.CalorieTrackingEnabled, request.Items));
    }

    public async Task<IReadOnlyCollection<MealLogResponse>> ListByDateAsync(string userId, DateOnly loggedDate, CancellationToken cancellationToken = default)
    {
        var mealLogs = await _mealLogRepository.ListByUserAndDateAsync(userId, loggedDate, cancellationToken);
        var preferences = await _userPreferencesRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? new Domain.Entities.UserPreferences { UserId = userId, CalorieTrackingEnabled = false, DailyCalorieGoal = null };

        return mealLogs
            .OrderBy(mealLog => mealLog.SlotType, StringComparer.OrdinalIgnoreCase)
            .Select(mealLog => new MealLogResponse(
                mealLog.Id,
                mealLog.LoggedDate,
                mealLog.SlotType,
                mealLog.Items.Select(item => new MealItemInput(item.ExternalFoodId, item.Quantity, item.Unit, item.Calories)).ToArray(),
                CalorieTrackingRules.ResolveTotalCalories(
                    preferences.CalorieTrackingEnabled,
                    mealLog.Items.Select(item => new MealItemInput(item.ExternalFoodId, item.Quantity, item.Unit, item.Calories)).ToArray())))
            .ToArray();
    }
}
