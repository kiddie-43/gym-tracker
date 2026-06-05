using GymTracker.Application.Contracts.Training;

namespace GymTracker.Application.Validation;

public static class TrainingMetricLogValidationRules
{
    public static void EnsureCreateRequest(CreateTrainingMetricGroupRequestDto request)
    {
        if (request is null)
        {
            throw new ArgumentException("Request is required.");
        }

        EnsureRequired(request.RoutineId, "RoutineId");
        EnsureRequired(request.SessionId, "SessionId");
        EnsureRequired(request.TrainingId, "TrainingId");
        EnsureRequired(request.ExerciseId, "ExerciseId");

        if (request.Metrics is null || request.Metrics.Count == 0)
        {
            throw new ArgumentException("At least one metric value is required.");
        }

        var metricIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var metric in request.Metrics)
        {
            EnsureRequired(metric.MetricId, "MetricId");
            if (!metricIds.Add(metric.MetricId.Trim()))
            {
                throw new ArgumentException($"MetricId '{metric.MetricId}' is duplicated in the same group.");
            }
        }
    }

    public static void EnsureUpdateValueRequest(UpdateMetricValueRequestDto request)
    {
        if (request is null)
        {
            throw new ArgumentException("Request is required.");
        }
    }

    public static void EnsureOwnershipAndIdentifiers(string userId, string groupId, string? metricId = null)
    {
        EnsureRequired(userId, "UserId");
        EnsureRequired(groupId, "GroupId");

        if (metricId is not null)
        {
            EnsureRequired(metricId, "MetricId");
        }
    }

    private static void EnsureRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} is required.");
        }
    }
}