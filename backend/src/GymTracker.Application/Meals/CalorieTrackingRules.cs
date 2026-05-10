namespace GymTracker.Application.Meals;

public static class CalorieTrackingRules
{
    public static double? ResolveTotalCalories(bool calorieTrackingEnabled, IReadOnlyCollection<MealItemInput> items)
    {
        if (!calorieTrackingEnabled)
        {
            return null;
        }

        if (items.Any(item => !item.Calories.HasValue))
        {
            return null;
        }

        return items.Sum(item => item.Calories!.Value);
    }

    public static bool IsGoalConfigured(bool calorieTrackingEnabled, int? dailyCalorieGoal)
    {
        return calorieTrackingEnabled && dailyCalorieGoal.HasValue && dailyCalorieGoal.Value > 0;
    }
}
