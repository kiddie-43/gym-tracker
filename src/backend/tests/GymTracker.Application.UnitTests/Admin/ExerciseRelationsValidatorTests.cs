using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class ExerciseRelationsValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldThrow_WhenPrimaryMuscleIsMissing()
    {
        var muscleRepository = new StubMuscleRepository(Array.Empty<Muscle>());
        var measurementTypeRepository = new StubExerciseFormTypeRepository(new[]
        {
            CreateMeasurementType("form-strength", "STRENGTH_BASIC"),
        });

        var validator = new ExerciseRelationsValidator(muscleRepository, measurementTypeRepository);

        var action = async () => await validator.ValidateAsync(CreateRequest(primaryMuscleIds: new[] { "missing" }));

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inactive or missing muscles*");
    }

    [Fact]
    public async Task ValidateAsync_ShouldThrow_WhenMeasurementTypeIsDeleted()
    {
        var activeMuscle = Muscle.Create("Chest", "chest", "", new[] { "upper-body" });
        var validator = new ExerciseRelationsValidator(
            new StubMuscleRepository(new[] { activeMuscle }),
            new StubExerciseFormTypeRepository(Array.Empty<AssignableMeasurementTypeResponse>()));

        var action = async () => await validator.ValidateAsync(CreateRequest(primaryMuscleIds: new[] { "chest" }));

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inactive or missing measurement type*");
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrow_WhenAnotherActiveExerciseUsesSameCode()
    {
        var repository = new ExerciseRepository();

        var request = CreateRequest(primaryMuscleIds: new[] { "chest" });
        var validator = new PassThroughRelationsValidator();

        var service = new ExerciseService(
            repository,
            new NoopStorageService(),
            new ExerciseMediaCompensationService(new NoopStorageService()),
            validator);

        var deleted = await service.CreateAsync(request);
        await service.DeleteAsync(deleted.Id);

        await service.CreateAsync(request);

        var action = async () => await service.ReactivateAsync(deleted.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Duplicate code*");
    }

    private static UpsertExerciseRequest CreateRequest(IReadOnlyCollection<string> primaryMuscleIds)
    {
        return new UpsertExerciseRequest(
            Name: "Press banca",
            Code: "BENCH_PRESS",
            Description: "Ejercicio compuesto",
            Category: "type-strength",
            Difficulty: "STRENGTH",
            MeasurementTypeIds: new[] { "form-strength" },
            MeasurementTypeCode: "STRENGTH_BASIC",
            PrimaryMuscleIds: primaryMuscleIds,
            SecondaryMuscleIds: Array.Empty<string>(),
            MuscleGroupIds: new[] { "upper-body" },
            Active: true);
    }

    private static AssignableMeasurementTypeResponse CreateMeasurementType(string id, string code)
    {
        return new AssignableMeasurementTypeResponse(
            Id: id,
            Code: code,
            Name: "Strength Basic",
            Description: null);
    }

    private sealed class StubMuscleRepository : IMuscleRepository
    {
        private readonly IReadOnlyCollection<Muscle> _items;

        public StubMuscleRepository(IReadOnlyCollection<Muscle> items)
        {
            _items = items;
        }

        public Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var rows = includeDeleted
                ? _items
                : _items.Where(item => !item.IsDeleted).ToArray();

            return Task.FromResult<IReadOnlyCollection<Muscle>>(rows.ToArray());
        }

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default)
        {
            var exists = _items.Any(item =>
                !item.IsDeleted
                && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(item.Id, excludingId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }

        public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class StubExerciseFormTypeRepository : IMeasurementTypeRepository
    {
        private readonly IReadOnlyCollection<AssignableMeasurementTypeResponse> _items;

        public StubExerciseFormTypeRepository(IReadOnlyCollection<AssignableMeasurementTypeResponse> items)
        {
            _items = items;
        }

        public Task<MeasurementTypesPageResponse> ListPageAsync(
            bool includeInactive = false,
            string? search = null,
            string? code = null,
            string sortBy = "name",
            string sortDirection = "asc",
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new MeasurementTypesPageResponse(Array.Empty<MeasurementTypeResponse>(), 0, page, pageSize));
        }

        public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items);
        }

        public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var row = _items.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
            if (row is null)
            {
                return Task.FromResult<MeasurementTypeResponse?>(null);
            }

            return Task.FromResult<MeasurementTypeResponse?>(new MeasurementTypeResponse(
                row.Id,
                row.Code,
                row.Name,
                row.Description,
                IsDeleted: false));
        }

        public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);
    }

    private sealed class PassThroughRelationsValidator : IExerciseRelationsValidator
    {
        public Task ValidateAsync(UpsertExerciseRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class NoopStorageService : IExerciseStorageService
    {
        public Task<string> GenerateUploadUrlAsync(string storagePath, TimeSpan expiresIn, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult("https://example.local/upload");

        public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
