using GymTracker.Application.Admin.ExerciseFormTypes;
using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Application.Common;
using GymTracker.Domain.ValueObjects;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Admin;

[ApiController]
[Route(AdminRoutes.Exercises)]
public sealed class ExercisesController : ControllerBase
{
    private readonly ExerciseService _service;
    private readonly IExerciseFormTypeRepository _measurementTypeRepository;
    private readonly IMuscleRepository _muscleRepository;
    private readonly IMeasurementTypeRepository _measurementTypeQueryRepository;

    public ExercisesController(ExerciseService service, IExerciseFormTypeRepository measurementTypeRepository, IMuscleRepository muscleRepository, IMeasurementTypeRepository measurementTypeQueryRepository)
    {
        _service = service;
        _measurementTypeRepository = measurementTypeRepository;
        _muscleRepository = muscleRepository;
        _measurementTypeQueryRepository = measurementTypeQueryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ExercisesPageApiResponse>> List(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListPageAsync(includeDeleted, search, sortBy, sortDirection, page, pageSize, cancellationToken);
        var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
        return Ok(new ExercisesPageApiResponse(rows.Items.Select(row => Map(row, muscleNames, measurementTypeNames)).ToArray(), rows.TotalCount, rows.Page, rows.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseApiResponse>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
        return Ok(Map(row, muscleNames, measurementTypeNames));
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseApiResponse>> Create([FromBody] UpsertExerciseApiRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _service.CreateAsync(await MapAsync(request, cancellationToken), cancellationToken);
            var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created, muscleNames, measurementTypeNames));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseApiResponse>> Update(string id, [FromBody] UpsertExerciseApiRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, await MapAsync(request, cancellationToken), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
            return Ok(Map(updated, muscleNames, measurementTypeNames));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<ExerciseApiResponse>> Reactivate(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _service.ReactivateAsync(id, cancellationToken);
            if (row is null)
            {
                return NotFound();
            }

            var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
            return Ok(Map(row, muscleNames, measurementTypeNames));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPost("import-csv")]
    public async Task<ActionResult<ImportExercisesCsvResultResponse>> ImportCsv(
        [FromBody] ImportExercisesCsvRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = request.Rows ?? Array.Empty<ImportExerciseCsvRowRequest>();
        var normalizedRows = new List<ImportExerciseRowRequest>(rows.Count);

        foreach (var row in rows)
        {
            var rawId = row.MeasurementTypeId ?? row.FormTypeId;
            var singleIds = string.IsNullOrWhiteSpace(rawId) ? Array.Empty<string>() : new[] { rawId };
            var measurementTypes = await ResolveMeasurementTypesAsync(singleIds, cancellationToken);
            var firstId = measurementTypes.Count > 0 ? measurementTypes.First().Id : string.Empty;
            var firstCode = measurementTypes.Count > 0 ? measurementTypes.First().Code : string.Empty;

            normalizedRows.Add(new ImportExerciseRowRequest(
                Code: row.Code,
                Name: row.Name,
                Description: row.Description,
                Category: row.Category,
                Difficulty: row.Difficulty,
                MeasurementTypeId: firstId,
                MeasurementTypeCode: firstCode,
                PrimaryMuscleIds: row.PrimaryMuscleIds,
                SecondaryMuscleIds: row.SecondaryMuscleIds));
        }

        var result = await _service.ImportCsvAsync(new ImportExercisesRequest(normalizedRows), cancellationToken);

        var (muscleNames, measurementTypeNames) = await BuildLookupsAsync(cancellationToken);
        return Ok(new ImportExercisesCsvResultResponse(
            result.TotalRows,
            result.CreatedRows,
            result.RejectedRows,
            result.Rows.Select(row => new ImportExerciseCsvRowResultResponse(
                row.RowNumber,
                row.Code,
                row.Created,
                row.Reason,
                row.Exercise is null ? null : Map(row.Exercise, muscleNames, measurementTypeNames))).ToArray()));
    }

    private async Task<UpsertExerciseRequest> MapAsync(UpsertExerciseApiRequest request, CancellationToken cancellationToken)
    {
        var measurementTypes = await ResolveMeasurementTypesAsync(request.MeasurementTypeIds ?? Array.Empty<string>(), cancellationToken);
        var firstCode = measurementTypes.Count > 0 ? measurementTypes.First().Code : string.Empty;

        var primary = request.PrimaryMuscleIds ?? Array.Empty<string>();
        var secondary = request.SecondaryMuscleIds ?? Array.Empty<string>();

        return new UpsertExerciseRequest(
            Name: request.Name,
            Code: request.Code,
            Description: request.Description,
            Category: ResolveCategory(request),
            Difficulty: ResolveDifficulty(request),
            MeasurementTypeIds: measurementTypes.Select(m => m.Id).ToArray(),
            MeasurementTypeCode: firstCode,
            PrimaryMuscleIds: primary,
            SecondaryMuscleIds: secondary,
            MuscleGroupIds: primary,
            Active: true);
    }

    private static string ResolveCategory(UpsertExerciseApiRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            return request.Category;
        }

        return request.ExerciseTypeId
            ?? throw new ArgumentException("Category is required.", nameof(request.Category));
    }

    private static string ResolveDifficulty(UpsertExerciseApiRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            return request.Difficulty;
        }

        return request.ExerciseTypeCode
            ?? throw new ArgumentException("Difficulty is required.", nameof(request.Difficulty));
    }

    private async Task<IReadOnlyCollection<(string Id, string Code)>> ResolveMeasurementTypesAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<(string, string)>();
        }

        var assignable = await _measurementTypeRepository.ListAsync(cancellationToken);
        var byId = assignable.ToDictionary(m => m.Id, m => m, StringComparer.OrdinalIgnoreCase);
        var byCode = assignable.ToDictionary(m => m.Code, m => m, StringComparer.OrdinalIgnoreCase);

        var result = new List<(string Id, string Code)>(ids.Count);
        foreach (var raw in ids)
        {
            var normalized = raw.Trim();
            if (string.IsNullOrWhiteSpace(normalized)) continue;

            if (byId.TryGetValue(normalized, out var byIdMatch))
            {
                result.Add((byIdMatch.Id, byIdMatch.Code));
            }
            else if (byCode.TryGetValue(normalized, out var byCodeMatch))
            {
                result.Add((byCodeMatch.Id, byCodeMatch.Code));
            }
            else
            {
                result.Add((normalized, normalized));
            }
        }

        return result;
    }

    private static ExerciseApiResponse Map(ExerciseResponse row, Dictionary<string, string> muscleNames, Dictionary<string, string> measurementTypeNames)
    {
        return new ExerciseApiResponse(
            row.Id,
            row.Code,
            row.Name,
            row.Description,
            row.ExerciseTypeCode,
            row.ExerciseTypeId,
            row.PrimaryMuscleIds.Select(id => new MuscleSummaryApiResponse(id, muscleNames.GetValueOrDefault(id, id))).ToArray(),
            row.SecondaryMuscleIds.Select(id => new MuscleSummaryApiResponse(id, muscleNames.GetValueOrDefault(id, id))).ToArray(),
            row.MeasurementTypeIds.ToArray(),
            row.MeasurementTypeIds.Select(id => measurementTypeNames.GetValueOrDefault(id, id)).ToArray(),
            row.Media.Where(item => item.MediaType == ExerciseMediaType.Image).Select(item => item.StoragePath).ToArray(),
            row.Media.Where(item => item.MediaType == ExerciseMediaType.Video).Select(item => item.StoragePath).ToArray(),
            row.IsDeleted,
            row.DeletedAt);
    }

    private async Task<(Dictionary<string, string> MuscleNames, Dictionary<string, string> MeasurementTypeNames)> BuildLookupsAsync(CancellationToken cancellationToken)
    {
        var muscles = await _muscleRepository.ListAsync(false, cancellationToken);
        var muscleNames = muscles.ToDictionary(m => m.Id, m => m.Name);

        var measurementTypes = await _measurementTypeQueryRepository.ListAssignableAsync(cancellationToken);
        var measurementTypeNames = measurementTypes.ToDictionary(mt => mt.Id, mt => mt.Name);

        return (muscleNames, measurementTypeNames);
    }
}

public sealed record UpsertExerciseApiRequest(
    string Code,
    string Name,
    string? Description,
    string? Difficulty,
    string? Category,
    IReadOnlyCollection<string>? PrimaryMuscleIds,
    IReadOnlyCollection<string>? SecondaryMuscleIds,
    IReadOnlyCollection<string>? MeasurementTypeIds,
    IReadOnlyCollection<string>? Images,
    IReadOnlyCollection<string>? Videos,
    [property: JsonPropertyName("exerciseTypeId")]
    string? ExerciseTypeId = null,
    [property: JsonPropertyName("exerciseTypeCode")]
    string? ExerciseTypeCode = null,
    [property: JsonPropertyName("formTypeId")]
    string? FormTypeId = null,
    [property: JsonPropertyName("formTypeCode")]
    string? FormTypeCode = null);

public sealed record MuscleSummaryApiResponse(string Id, string Name);

public sealed record ExerciseApiResponse(
    string Id,
    string Code,
    string Name,
    string? Description,
    string Difficulty,
    string Category,
    IReadOnlyCollection<MuscleSummaryApiResponse> PrimaryMuscles,
    IReadOnlyCollection<MuscleSummaryApiResponse> SecondaryMuscles,
    IReadOnlyCollection<string> MeasurementTypeIds,
    IReadOnlyCollection<string> MeasurementTypeNames,
    IReadOnlyCollection<string> Images,
    IReadOnlyCollection<string> Videos,
    bool IsDeleted,
    DateTimeOffset? DeletedAt);

public sealed record ExercisesPageApiResponse(
    IReadOnlyCollection<ExerciseApiResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ImportExercisesCsvRequest(
    IReadOnlyCollection<ImportExerciseCsvRowRequest>? Rows);

public sealed record ImportExerciseCsvRowRequest(
    string? Code,
    string? Name,
    string? Description,
    string? Category,
    string? Difficulty,
    string? MeasurementTypeId,
    IReadOnlyCollection<string>? PrimaryMuscleIds,
    IReadOnlyCollection<string>? SecondaryMuscleIds,
    [property: JsonPropertyName("formTypeId")]
    string? FormTypeId = null,
    [property: JsonPropertyName("formTypeCode")]
    string? FormTypeCode = null);

public sealed record ImportExercisesCsvResultResponse(
    int TotalRows,
    int CreatedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportExerciseCsvRowResultResponse> Rows);

public sealed record ImportExerciseCsvRowResultResponse(
    int RowNumber,
    string? Code,
    bool Created,
    string? Reason,
    ExerciseApiResponse? Exercise);
