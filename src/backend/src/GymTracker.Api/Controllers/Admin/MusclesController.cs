using GymTracker.Application.Admin.Common;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Application.Common;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Admin;

[ApiController]
[Route(AdminRoutes.Muscles)]
public sealed class MusclesController : ControllerBase
{
    private static readonly string[] DefaultMuscleGroupIds = ["general"];

    private readonly MuscleService _service;

    public MusclesController(MuscleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<MusclesPageItemResponse>> List(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string? code = null,
        [FromQuery] string? name = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListPageAsync(includeDeleted, search, code, name, sortBy, sortDirection, page, pageSize, cancellationToken);
        return Ok(new MusclesPageItemResponse(rows.Items.Select(Map).ToArray(), rows.TotalCount, rows.Page, rows.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MuscleItemResponse>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(Map(row));
    }

    [HttpPost]
    public async Task<ActionResult<MuscleItemResponse>> Create([FromBody] UpsertMuscleItemRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _service.CreateAsync(Map(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
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
    public async Task<ActionResult<MuscleItemResponse>> Update(string id, [FromBody] UpsertMuscleItemRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, Map(request), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(Map(updated));
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
    public async Task<ActionResult<MuscleItemResponse>> Reactivate(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _service.ReactivateAsync(id, cancellationToken);
            if (row is null)
            {
                return NotFound();
            }

            return Ok(Map(row));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
    }

    [HttpPost("import-csv")]
    public async Task<ActionResult<ImportMusclesCsvResultResponse>> ImportCsv(
        [FromBody] ImportMusclesCsvRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = request.Rows ?? Array.Empty<ImportMuscleCsvRowRequest>();
        var result = await _service.ImportCsvAsync(
            new ImportMusclesRequest(rows.Select(row => new ImportMuscleRowRequest(row.Name, row.Code, row.Description)).ToArray()),
            cancellationToken);

        return Ok(new ImportMusclesCsvResultResponse(
            result.TotalRows,
            result.ImportedRows,
            result.RejectedRows,
            result.Results.Select(row => new ImportMuscleCsvRowResultResponse(
                row.RowNumber,
                row.Code,
                row.Imported,
                row.Reason,
                row.Muscle is null ? null : Map(row.Muscle))).ToArray()));
    }

    private static UpsertMuscleRequest Map(UpsertMuscleItemRequest request)
    {
        var normalizedCode = AdminRequestSanitizer.RequiredUpperCode(request.Code, nameof(request.Code));
        var normalizedName = string.IsNullOrWhiteSpace(request.Name)
            ? normalizedCode
            : AdminRequestSanitizer.RequiredTrimmed(request.Name, nameof(request.Name));

        return new UpsertMuscleRequest(
            Name: normalizedName,
            Code: normalizedCode,
            Description: AdminRequestSanitizer.OptionalTrimmed(request.Description),
            MuscleGroupIds: request.MuscleGroupIds is { Count: > 0 } ? request.MuscleGroupIds : DefaultMuscleGroupIds,
            Active: true);
    }

    private static MuscleItemResponse Map(MuscleResponse row)
    {
        return new MuscleItemResponse(
            row.Id,
            row.Code,
            row.Description,
            row.IsDeleted,
            row.DeletedAt,
            row.Name,
            row.MuscleGroupIds);
    }
}

public sealed record UpsertMuscleItemRequest(
    string Code,
    string? Description,
    string? Name,
    IReadOnlyCollection<string>? MuscleGroupIds);

public sealed record MuscleItemResponse(
    string Id,
    string Code,
    string? Description,
    bool IsDeleted,
    DateTimeOffset? DeletedAt,
    string Name,
    IReadOnlyCollection<string> MuscleGroupIds);

public sealed record MusclesPageItemResponse(
    IReadOnlyCollection<MuscleItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ImportMusclesCsvRequest(
    IReadOnlyCollection<ImportMuscleCsvRowRequest>? Rows);

public sealed record ImportMuscleCsvRowRequest(
    string? Name,
    string? Code,
    string? Description);

public sealed record ImportMusclesCsvResultResponse(
    int TotalRows,
    int ImportedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportMuscleCsvRowResultResponse> Results);

public sealed record ImportMuscleCsvRowResultResponse(
    int RowNumber,
    string? Code,
    bool Imported,
    string? Reason,
    MuscleItemResponse? Muscle);
