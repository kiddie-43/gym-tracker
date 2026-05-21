using FluentAssertions;

using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.UnitTests.Training;

public sealed class ExerciseTrainingLogRulesTests
{
    [Fact]
    public void EnsureExerciseLog_ShouldThrow_WhenAttachmentsExceedLimit()
    {
        var log = new ExerciseTrainingLog
        {
            UserId = "user-1",
            RoutineId = "r1",
            SessionId = "s1",
            ExerciseId = "e1",
            PerformedSets = new List<PerformedSet> { new() { Order = 1, Repetitions = 8, WeightKg = 20 } },
            Attachments = Enumerable.Range(1, 6).Select(index => new TrainingAttachment { Type = "photo", Url = $"https://test/{index}" }).ToList(),
        };

        var action = () => RoutinesValidationRules.EnsureExerciseLog(log);

        action.Should().Throw<ArgumentException>();
    }
}
