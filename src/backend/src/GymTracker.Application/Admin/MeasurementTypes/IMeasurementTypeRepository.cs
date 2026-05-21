namespace GymTracker.Application.Admin.MeasurementTypes;

public interface IMeasurementTypeRepository
{
    Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeInactive = false,
        string? search = null,
        string? code = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default);

    Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default);

    Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default);
}
