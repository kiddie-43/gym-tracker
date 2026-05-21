using FluentAssertions;

using GymTracker.Application.Meals;

namespace GymTracker.Application.UnitTests.Meals;

public sealed class CalorieTrackingRulesTests
{
    [Fact]
    public void ResolveTotalCalories_ShouldReturnNull_WhenTrackingDisabled()
    {
        var result = CalorieTrackingRules.ResolveTotalCalories(false, new[]
        {
            new MealItemInput("food-1", 1, "portion", 320),
        });

        result.Should().BeNull();
    }

    [Fact]
    public void ResolveTotalCalories_ShouldReturnNull_WhenAnyItemHasUnknownCalories()
    {
        var result = CalorieTrackingRules.ResolveTotalCalories(true, new[]
        {
            new MealItemInput("food-1", 1, "portion", 300),
            new MealItemInput("food-2", 1, "portion", null),
        });

        result.Should().BeNull();
    }

    [Fact]
    public void ResolveTotalCalories_ShouldSumCalories_WhenTrackingEnabledAndAllKnown()
    {
        var result = CalorieTrackingRules.ResolveTotalCalories(true, new[]
        {
            new MealItemInput("food-1", 1, "portion", 300),
            new MealItemInput("food-2", 1, "portion", 250),
        });

        result.Should().Be(550);
    }
}
