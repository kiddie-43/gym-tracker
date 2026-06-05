namespace GymTracker.Infrastructure.TrainingLogs;

using GymTracker.Application.TrainingLog;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore;

public sealed class SqlTrainingLogRepository : ITrainingLogRepository
{
    private readonly AdminDbContext _context;

    public SqlTrainingLogRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task CreateGroupAsync(
        IReadOnlyCollection<TrainingLog> logs,
        CancellationToken cancellationToken = default)
    {
        _context.TrainingLogs.AddRange(logs);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrainingLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrainingLogs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrainingLog>> ListByGroupIdAsync(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrainingLogs
            .Where(x => x.GroupId == groupId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrainingLog>> ListBySessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrainingLogs
            .Where(x => x.SessionId == sessionId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        TrainingLog log,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        TrainingLog log,
        CancellationToken cancellationToken = default)
    {
        _context.TrainingLogs.Remove(log);

        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<TrainingLog>> ListAsync(
        Guid userId,
        string? routineId,
        string? sessionId,
        string? exerciseId,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TrainingLogs
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(routineId))
        {
            query = query.Where(x => x.RoutineId == routineId);
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            query = query.Where(x => x.SessionId == sessionId);
        }

        if (!string.IsNullOrWhiteSpace(exerciseId) && Guid.TryParse(exerciseId, out var parsedExerciseId))
        {
            query = query.Where(x => x.ExerciseId == parsedExerciseId);
        }

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = date.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            query = query.Where(x =>
                x.Timestamp >= start &&
                x.Timestamp < end);
        }

        return await query
            .OrderByDescending(x => x.Timestamp)
            .ToArrayAsync(cancellationToken);
    }
}