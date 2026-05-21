using FluentAssertions;

using GymTracker.Application.Features.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.UnitTests.Routines;

public sealed class RoutineRulesTests
{
    [Fact]
    public async Task ArchiveAndReactivate_ShouldToggleSoftDeleteFlags()
    {
        var repository = new InMemoryRoutineRepository();
        var handlers = new RoutinesCommandHandlers(repository);

        var created = await handlers.CreateAsync("user-1", new("Push", null));

        await handlers.ArchiveAsync("user-1", created.Id);
        var archived = await handlers.GetByIdAsync("user-1", created.Id);
        archived!.IsDeleted.Should().BeTrue();

        await handlers.ReactivateAsync("user-1", created.Id);
        var active = await handlers.GetByIdAsync("user-1", created.Id);
        active!.IsDeleted.Should().BeFalse();
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
