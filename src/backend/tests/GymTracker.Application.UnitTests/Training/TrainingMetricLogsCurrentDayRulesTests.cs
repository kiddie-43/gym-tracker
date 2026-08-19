using FluentAssertions;

using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.UnitTests.Training;

public sealed class TrainingMetricLogsCurrentDayRulesTests
{
    [Fact]
    public void SelectLatestGroupIdForDate_ShouldReturnNull_WhenThereAreNoLogs()
    {
        var selected = TrainingMetricLogsCurrentDayRules.SelectLatestGroupIdForDate([], DateOnly.FromDateTime(DateTime.UtcNow));

        selected.Should().BeNull();
    }

    [Fact]
    public void SelectLatestGroupIdForDate_ShouldReturnMostRecentGroupForCurrentDate()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;

        var logs = new[]
        {
            new TrainingMetricLog
            {
                GroupId = "group-old",
                OperationalDate = today,
                CreatedAt = now.AddMinutes(-10),
            },
            new TrainingMetricLog
            {
                GroupId = "group-new",
                OperationalDate = today,
                CreatedAt = now,
            },
            new TrainingMetricLog
            {
                GroupId = "group-other-day",
                OperationalDate = today.AddDays(-1),
                CreatedAt = now.AddHours(1),
            },
        };

        var selected = TrainingMetricLogsCurrentDayRules.SelectLatestGroupIdForDate(logs, today);

        selected.Should().Be("group-new");
    }

    [Fact]
    public void SelectLatestGroupIdForDate_ShouldIgnoreSoftDeletedLogs()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;

        var logs = new[]
        {
            new TrainingMetricLog
            {
                GroupId = "group-active",
                OperationalDate = today,
                CreatedAt = now.AddMinutes(-5),
                IsDeleted = false,
            },
            new TrainingMetricLog
            {
                GroupId = "group-deleted",
                OperationalDate = today,
                CreatedAt = now,
                IsDeleted = true,
            },
        };

        var selected = TrainingMetricLogsCurrentDayRules.SelectLatestGroupIdForDate(logs, today);

        selected.Should().Be("group-active");
    }
}
