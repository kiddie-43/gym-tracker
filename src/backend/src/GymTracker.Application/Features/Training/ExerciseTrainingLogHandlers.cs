using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.Features.Training;

public sealed class ExerciseTrainingLogHandlers
{
    private readonly ITrainingFlowRepository _repository;
    private readonly ProgressComparisonService _progressComparisonService;

    public ExerciseTrainingLogHandlers(ITrainingFlowRepository repository, ProgressComparisonService progressComparisonService)
    {
        _repository = repository;
        _progressComparisonService = progressComparisonService;
    }

    public async Task<ExerciseTrainingLogDto> SaveAsync(string userId, CreateExerciseTrainingLogDto dto, CancellationToken cancellationToken = default)
    {
        var log = new ExerciseTrainingLog
        {
            UserId = userId,
            RoutineId = dto.RoutineId,
            SessionId = dto.SessionId,
            ExerciseId = dto.ExerciseId,
            PerformedSets = dto.PerformedSets.Select(setItem => new PerformedSet
            {
                Order = setItem.Order,
                Repetitions = setItem.Repetitions,
                WeightKg = setItem.WeightKg,
            }).ToList(),
            Notes = dto.Notes,
            Attachments = dto.Attachments?.Select(attachment => new TrainingAttachment
            {
                Type = attachment.Type,
                Url = attachment.Url,
            }).ToList() ?? new List<TrainingAttachment>(),
        };

        RoutinesValidationRules.EnsureExerciseLog(log);

        await _repository.SaveLogAsync(log, cancellationToken);
        return ToDto(log);
    }

    public async Task<ExerciseTrainingLogDto?> GetAsync(string userId, string logId, CancellationToken cancellationToken = default)
    {
        var log = await _repository.GetLogByIdAsync(userId, logId, cancellationToken);
        return log is null ? null : ToDto(log);
    }

    public async Task<ProgressComparisonDto?> GetProgressComparisonAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.ListLogsByExerciseAsync(userId, exerciseId, cancellationToken);
        if (logs.Count == 0)
        {
            return null;
        }

        return _progressComparisonService.BuildComparison(exerciseId, logs);
    }

    private static ExerciseTrainingLogDto ToDto(ExerciseTrainingLog log)
    {
        return new ExerciseTrainingLogDto(
            log.Id,
            log.UserId,
            log.RoutineId,
            log.SessionId,
            log.ExerciseId,
            log.PerformedSets.Select(setItem => new PerformedSetDto(setItem.Repetitions, setItem.WeightKg, setItem.Order)).ToArray(),
            log.Notes,
            log.Attachments.Select(attachment => new TrainingAttachmentDto(attachment.Type, attachment.Url)).ToArray(),
            log.CreatedAt,
            log.UpdatedAt);
    }
}
