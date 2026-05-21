using GymTracker.Application.Contracts.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.Features.Routines;

public sealed class RoutineSessionsCommandHandlers
{
    private readonly IRoutineRepository _routineRepository;

    public RoutineSessionsCommandHandlers(IRoutineRepository routineRepository)
    {
        _routineRepository = routineRepository;
    }

    public async Task<RoutineSessionDto?> CreateAsync(string userId, string routineId, CreateRoutineSessionDto dto, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null || routine.IsDeleted)
        {
            return null;
        }

        var normalizedDays = RoutinesValidationRules.NormalizeDays(dto.DaysOfWeek);
        RoutinesValidationRules.EnsureNoSessionDayConflict(routine.Sessions, normalizedDays);

        var session = new RoutineSession
        {
            RoutineId = routine.Id,
            Name = dto.Name.Trim(),
            DaysOfWeek = normalizedDays.ToList(),
        };

        routine.Sessions.Add(session);
        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);

        return new RoutineSessionDto(session.Id, session.Name, session.DaysOfWeek, Array.Empty<SessionExerciseDto>());
    }

    public async Task<bool> SoftDeleteAsync(string userId, string routineId, string sessionId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return false;
        }

        var session = routine.Sessions.FirstOrDefault(item => item.Id == sessionId && !item.IsDeleted);
        if (session is null)
        {
            return false;
        }

        session.SoftDelete();
        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return true;
    }
}
