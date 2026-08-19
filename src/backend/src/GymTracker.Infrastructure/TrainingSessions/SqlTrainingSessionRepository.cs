namespace GymTracker.Infrastructure.TrainingSessions;

using GymTracker.Application.TrainingSession;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore;

public sealed class SqlTrainingSessionRepository : ITrainingSessionRepository
{
    private readonly AdminDbContext _context;

    public SqlTrainingSessionRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TrainingSession session, CancellationToken cancellationToken = default)
    {
        _context.TrainingSessions.Add(session);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrainingSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrainingSessions
            .Include(session => session.Blocks)
            .FirstOrDefaultAsync(
                session => session.Id == id && session.UserId == userId && session.DeletedAt == null,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrainingSession>> ListHistoryAsync(
        Guid userId,
        int weekNumber,
        int dayNumber,
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrainingSessions
            .Include(session => session.Blocks)
            .Where(session =>
                session.UserId == userId &&
                session.WeekNumber == weekNumber &&
                session.DayNumber == dayNumber &&
                session.ExerciseId == exerciseId &&
                session.DeletedAt == null)
            .OrderByDescending(session => session.Timestamp)
            .ToArrayAsync(cancellationToken);
    }
}
