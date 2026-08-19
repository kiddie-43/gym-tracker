namespace GymTracker.Application.TrainingSession;

using GymTracker.Domain.Entities;

public sealed class TrainingSessionService
{
    private readonly ITrainingSessionRepository _repository;
    private readonly IExerciceRepository _exerciceRepository;

    public TrainingSessionService(ITrainingSessionRepository repository, IExerciceRepository exerciceRepository)
    {
        _repository = repository;
        _exerciceRepository = exerciceRepository;
    }

    public async Task<TrainingSessionResponse> CreateAsync(
        CreateTrainingSessionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var exercice = await _exerciceRepository.GetByIdAsync(request.ExerciseId, cancellationToken)
            ?? throw new ArgumentException("Exercise not found.", nameof(request));

        var profile = ExerciseTypeProfileMapper.ResolveProfile(exercice.ExerciseType);

        var session = Domain.Entities.TrainingSession.Create(
            userId,
            request.WeekNumber,
            request.DayNumber,
            request.ExerciseId,
            DateTimeOffset.UtcNow,
            request.DurationMinutes,
            request.SecondaryMetricValue,
            request.SecondaryMetricUnitCode,
            request.TertiaryMetricValue,
            request.Notes);

        foreach (var blockRequest in request.Blocks)
        {
            var blockType = ParseBlockType(blockRequest.BlockType);

            if (!ExerciseTypeProfileMapper.IsValidForProfile(blockType, profile))
                throw new ArgumentException($"BlockType '{blockRequest.BlockType}' is not valid for this exercise profile.", nameof(request));

            var block = SessionBlock.Create(
                session.Id,
                blockType,
                blockRequest.Name,
                blockRequest.DurationValue,
                blockRequest.DurationUnitCode,
                blockRequest.Description,
                blockRequest.IntensityRpe,
                blockRequest.OrderIndex);

            session.AddBlock(block);
        }

        await _repository.AddAsync(session, cancellationToken);

        return Map(session);
    }

    public async Task<TrainingSessionResponse?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetByIdAsync(id, userId, cancellationToken);

        return session is null ? null : Map(session);
    }

    public async Task<IReadOnlyCollection<TrainingSessionResponse>> ListHistoryAsync(
        Guid userId,
        int weekNumber,
        int dayNumber,
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.ListHistoryAsync(userId, weekNumber, dayNumber, exerciseId, cancellationToken);

        return sessions
            .OrderByDescending(session => session.Timestamp)
            .Select(Map)
            .ToArray();
    }

    private static Domain.Enum.SessionBlockType ParseBlockType(string value)
    {
        if (!System.Enum.TryParse<Domain.Enum.SessionBlockType>(value, true, out var parsed)
            || !System.Enum.IsDefined(parsed))
        {
            throw new ArgumentException($"BlockType '{value}' is invalid.", nameof(value));
        }

        return parsed;
    }

    private static TrainingSessionResponse Map(Domain.Entities.TrainingSession session)
    {
        return new TrainingSessionResponse(
            session.Id,
            session.WeekNumber,
            session.DayNumber,
            session.ExerciseId,
            session.Timestamp,
            session.DurationMinutes,
            session.SecondaryMetricValue,
            session.SecondaryMetricUnitCode,
            session.TertiaryMetricValue,
            session.Notes,
            session.Blocks
                .OrderBy(block => block.OrderIndex)
                .Select(block => new SessionBlockResponse(
                    block.Id,
                    block.BlockType.ToString(),
                    block.Name,
                    block.DurationValue,
                    block.DurationUnitCode,
                    block.Description,
                    block.IntensityRpe,
                    block.OrderIndex))
                .ToArray());
    }
}
