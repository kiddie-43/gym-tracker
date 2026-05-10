using FluentAssertions;

using GymTracker.Application.Catalog;
using GymTracker.Application.Routines;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Routines;

public sealed class RoutineServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldComposeRoutineDaysAndExercises_WhenRequestIsValid()
    {
        var repository = new InMemoryRoutineRepository();
        var catalog = new FakeCatalogService();
        var service = new RoutineService(repository, catalog);

        var request = new CreateRoutineRequest(
            "Push Split",
            "Upper body push focus",
            new[] { "push", "upper" },
            new[]
            {
                new CreateRoutineDayRequest(
                    "Monday",
                    new[]
                    {
                        new CreatePlannedExerciseRequest("bench-press", "Bench press", new[] { "chest" }, 4, 8, 120, null, null),
                    }),
            });

        var response = await service.CreateAsync("user-1", request);

        response.Name.Should().Be("Push Split");
        response.Days.Should().HaveCount(1);
        response.Days.Single().Exercises.Single().ExternalExerciseId.Should().Be("bench-press");
        repository.Stored.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDuplicateDayLabelsExist()
    {
        var service = new RoutineService(new InMemoryRoutineRepository(), new FakeCatalogService());

        var request = new CreateRoutineRequest(
            "Split",
            null,
            Array.Empty<string>(),
            new[]
            {
                new CreateRoutineDayRequest("Monday", new[] { new CreatePlannedExerciseRequest("bench", "Bench", new[] { "chest" }, 3, 8, null, null, null) }),
                new CreateRoutineDayRequest("monday", new[] { new CreatePlannedExerciseRequest("ohp", "Overhead press", new[] { "shoulders" }, 3, 8, null, null, null) }),
            });

        var action = async () => await service.CreateAsync("user-1", request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Duplicate day labels*");
    }

    [Fact]
    public async Task GetSuggestedExercisesAsync_ShouldRequestCatalogUsingMuscleGroupFilter()
    {
        var catalog = new FakeCatalogService();
        var service = new RoutineService(new InMemoryRoutineRepository(), catalog);

        var response = await service.GetSuggestedExercisesAsync(new[] { "chest", "triceps" }, "press");

        catalog.LastMuscleGroupIds.Should().BeEquivalentTo(new[] { "chest", "triceps" });
        catalog.LastQuery.Should().Be("press");
        response.Exercises.Should().HaveCount(1);
    }

    private sealed class InMemoryRoutineRepository : IRoutineRepository
    {
        public List<Routine> Stored { get; } = new();

        public Task AddAsync(Routine routine, CancellationToken cancellationToken = default)
        {
            Stored.Add(routine);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default)
        {
            var index = Stored.FindIndex(r => r.Id == routine.Id && r.UserId == routine.UserId);
            if (index >= 0)
            {
                Stored[index] = routine;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default)
        {
            Stored.RemoveAll(r => r.UserId == userId && r.Id == routineId);
            return Task.CompletedTask;
        }

        public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stored.FirstOrDefault(r => r.UserId == userId && r.Id == routineId));
        }

        public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Routine>>(Stored.Where(r => r.UserId == userId).ToArray());
        }
    }

    private sealed class FakeCatalogService : ICatalogService
    {
        public string? LastQuery { get; private set; }

        public IReadOnlyCollection<string>? LastMuscleGroupIds { get; private set; }

        public Task<CatalogAvailabilityDto> GetAvailabilityAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CatalogAvailabilityDto(false, DateTimeOffset.UtcNow));
        }

        public Task<IReadOnlyCollection<ExerciseDto>> GetExercisesAsync(string? languageCode, string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            LastMuscleGroupIds = muscleGroupIds;

            return Task.FromResult<IReadOnlyCollection<ExerciseDto>>(new[]
            {
                new ExerciseDto("bench-press", "Bench press", new[] { "chest" }, null),
            });
        }

        public Task<IReadOnlyCollection<FoodDto>> GetFoodsAsync(string query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<FoodDto>>(Array.Empty<FoodDto>());
        }

        public Task<IReadOnlyCollection<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<MuscleGroupDto>>(new[]
            {
                new MuscleGroupDto("chest", "Chest"),
            });
        }
    }
}
