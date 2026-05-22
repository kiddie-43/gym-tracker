using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleCsvImportService
{
    private static readonly string[] DefaultMuscleGroupIds = ["general"];

    public async Task<ImportMusclesResult> ImportAsync(
        ImportMusclesRequest request,
        IMuscleRepository repository,
        CancellationToken cancellationToken = default)
    {
        var rows = request.Rows ?? Array.Empty<ImportMuscleRowRequest>();
        var results = new List<ImportMuscleRowResult>(rows.Count);
        var importedRows = 0;

        var existingActiveCodes = (await repository.ListAsync(includeDeleted: false, cancellationToken))
            .Select(item => item.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var batchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rowNumber = 1;
        foreach (var row in rows)
        {
            var normalizedCode = string.IsNullOrWhiteSpace(row.Code)
                ? null
                : row.Code.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                results.Add(new ImportMuscleRowResult(rowNumber, row.Code, false, "Code is required.", null));
                rowNumber++;
                continue;
            }

            if (!batchCodes.Add(normalizedCode))
            {
                results.Add(new ImportMuscleRowResult(rowNumber, normalizedCode, false, "Duplicate code in CSV payload.", null));
                rowNumber++;
                continue;
            }

            if (existingActiveCodes.Contains(normalizedCode))
            {
                results.Add(new ImportMuscleRowResult(rowNumber, normalizedCode, false, "Duplicate code against existing active records.", null));
                rowNumber++;
                continue;
            }

            var entity = Muscle.Create(
                name: string.IsNullOrWhiteSpace(row.Name) ? normalizedCode : row.Name.Trim(),
                code: normalizedCode,
                description: AdminRequestSanitizer.OptionalTrimmed(row.Description),
                muscleGroupIds: DefaultMuscleGroupIds);

            await repository.SaveAsync(entity, cancellationToken);
            existingActiveCodes.Add(normalizedCode);
            importedRows++;

            results.Add(new ImportMuscleRowResult(rowNumber, normalizedCode, true, null, Map(entity)));
            rowNumber++;
        }

        return new ImportMusclesResult(
            TotalRows: rows.Count,
            ImportedRows: importedRows,
            RejectedRows: rows.Count - importedRows,
            Results: results);
    }

    private static MuscleResponse Map(Muscle entity)
    {
        return new MuscleResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.MuscleGroupIds,
            entity.Active,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
