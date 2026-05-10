using FluentAssertions;

using GymTracker.Application.Diets;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Diets;

public sealed class DietServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldStoreDietWithDays_WhenRequestIsValid()
    {
        var repository = new InMemoryDietRepository();
        var service = new DietService(repository);

        var request = new CreateDietRequest(
            "Lean week",
            new[]
            {
                new DietDayInput("monday", new[]
                {
                    new MealSlotInput("breakfast", new[]
                    {
                        new MealItemInput("oats", 80, "g", 300),
                    }),
                }),
            });

        var created = await service.CreateAsync("user-1", request);

        created.Name.Should().Be("Lean week");
        created.Days.Should().HaveCount(1);
        repository.Stored.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDaysAreMissing()
    {
        var service = new DietService(new InMemoryDietRepository());

        var action = async () => await service.CreateAsync("user-1", new CreateDietRequest("Plan", Array.Empty<DietDayInput>()));

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one diet day is required*");
    }

    private sealed class InMemoryDietRepository : IDietRepository
    {
        public List<Diet> Stored { get; } = new();

        public Task AddAsync(Diet diet, CancellationToken cancellationToken = default)
        {
            Stored.Add(diet);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default)
        {
            Stored.RemoveAll(d => d.UserId == userId && d.Id == dietId);
            return Task.CompletedTask;
        }

        public Task<Diet?> GetByUserAndIdAsync(string userId, string dietId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stored.FirstOrDefault(d => d.UserId == userId && d.Id == dietId));
        }

        public Task<IReadOnlyCollection<Diet>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Diet>>(Stored.Where(d => d.UserId == userId).ToArray());
        }

        public Task UpdateAsync(Diet diet, CancellationToken cancellationToken = default)
        {
            var index = Stored.FindIndex(d => d.UserId == diet.UserId && d.Id == diet.Id);
            if (index >= 0)
            {
                Stored[index] = diet;
            }

            return Task.CompletedTask;
        }
    }
}
