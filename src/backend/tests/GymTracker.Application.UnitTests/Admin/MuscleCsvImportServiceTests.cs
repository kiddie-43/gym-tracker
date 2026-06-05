using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class MuscleCsvImportServiceTests
{
    [Fact]
    public async Task ImportAsync_ShouldRejectDuplicateRowsAndMissingCodes()
    {
        var repository = new InMemoryMuscleRepository();
        var service = new MuscleCsvImportService();

        var result = await service.ImportAsync(new ImportMusclesRequest(new[]
        {
            new ImportMuscleRowRequest("Biceps braquial", "BICEPS", "ok"),
            new ImportMuscleRowRequest("Biceps repetido", "BICEPS", "dup"),
            new ImportMuscleRowRequest("Sin codigo", "", "missing"),
        }), repository);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.ImportedRows);
        Assert.Equal(2, result.RejectedRows);
        Assert.Equal(1, result.Results.Count(row => row.Imported));
    }

    private sealed class InMemoryMuscleRepository : IMuscleRepository
    {
        private readonly Dictionary<Guid, Muscle> _store = [];

        public Task<Muscle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var rows = includeDeleted
                ? _store.Values.ToArray()
                : _store.Values.Where(item => !item.IsDeleted).ToArray();

            return Task.FromResult<IReadOnlyCollection<Muscle>>(rows);
        }

        public Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
        {
            var exists = _store.Values.Any(item =>
                !item.IsDeleted
                && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)
                && item.Id != excludingId);

            return Task.FromResult(exists);
        }

        public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var row = await GetByIdAsync(id, cancellationToken);
            if (row is null)
            {
                return false;
            }

            row.Delete(null);
            return true;
        }

        public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var row = await GetByIdAsync(id, cancellationToken);
            if (row is null)
            {
                return false;
            }

            row.Restore();
            return true;
        }
    }
}
