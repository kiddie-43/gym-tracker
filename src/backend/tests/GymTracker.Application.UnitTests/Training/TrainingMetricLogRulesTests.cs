using FluentAssertions;

using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Validation;

namespace GymTracker.Application.UnitTests.Training;

public sealed class TrainingMetricLogRulesTests
{
    [Fact]
    public void EnsureCreateRequest_ShouldThrow_WhenMetricIdIsDuplicatedWithinGroup()
    {
        var request = new CreateTrainingMetricGroupRequestDto(
            "routine-1",
            "session-1",
            "training-1",
            "exercise-1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            new[]
            {
                new GroupMetricValueInputDto("metric-a", 10),
                new GroupMetricValueInputDto("metric-a", 12),
            });

        var action = () => TrainingMetricLogValidationRules.EnsureCreateRequest(request);

        action.Should().Throw<ArgumentException>().WithMessage("*duplicated*");
    }

    [Fact]
    public void EnsureOwnershipAndIdentifiers_ShouldThrow_WhenGroupIdIsMissing()
    {
        var action = () => TrainingMetricLogValidationRules.EnsureOwnershipAndIdentifiers("user-1", " ");

        action.Should().Throw<ArgumentException>().WithMessage("*GroupId is required*");
    }

    [Fact]
    public void EnsureOwnershipAndIdentifiers_ShouldThrow_WhenMetricIdIsMissingForValueUpdate()
    {
        var action = () => TrainingMetricLogValidationRules.EnsureOwnershipAndIdentifiers("user-1", "group-1", " ");

        action.Should().Throw<ArgumentException>().WithMessage("*MetricId is required*");
    }
}
