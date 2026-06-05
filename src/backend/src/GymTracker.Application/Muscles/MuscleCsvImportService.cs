using GymTracker.Application.Common.Importing;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleCsvImportService
{
    private readonly CsvImportService _csvImportService;
    private readonly MuscleImportDefinition _definition;

    public MuscleCsvImportService(
        CsvImportService csvImportService,
        MuscleImportDefinition definition)
    {
        _csvImportService = csvImportService;
        _definition = definition;
    }

    public async Task<ImportMusclesResult> ImportAsync(
    Stream csvStream,
    CancellationToken cancellationToken = default)
{
    var result = await _csvImportService.ImportAsync(
        csvStream,
        _definition,
        cancellationToken);

    return new ImportMusclesResult(
        result.TotalRows,
        result.ImportedRows,
        result.RejectedRows,
        result.Results.Select(x => new ImportMuscleRowResult(
            x.RowNumber,
            x.Code,
            x.Success,
            x.Error,
            x.Entity)).ToArray());
}
}