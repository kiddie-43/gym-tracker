using FluentAssertions;

using GymTracker.Application.Features.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.UnitTests.Routines;

public sealed class RoutineSessionDayConflictTests
{
    [Fact]
    public async Task CreateSession_ShouldThrow_WhenDayAlreadyUsed()
    {
        var repository = new InMemoryRoutineRepository();
        var routine = new Routine { UserId = "user-1", Description = "Routine" };
        routine.Sessions.Add(new RoutineSession { RoutineId = routine.Id, Name = "A", DaysOfWeek = new List<string> { "monday" } });
        await repository.UpsertAsync(routine);

        var handlers = new RoutineSessionsCommandHandlers(repository);

        var action = async () => await handlers.CreateAsync("user-1", routine.Id, new("B", new[] { "monday" }));

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class InMemoryRoutineRepository : IRoutineRepository
    {
        private readonly List<Routine> _items = new();

        public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Routine>>(_items.Where(item => item.UserId == userId).ToArray());

        public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(item => item.UserId == userId && item.Id == routineId));

        public Task UpsertAsync(Routine routine, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(item => item.UserId == routine.UserId && item.Id == routine.Id);
            if (index >= 0)
            {
                _items[index] = routine;
            }
            else
            {
                _items.Add(routine);
            }

            return Task.CompletedTask;
        }
    }
}
