using GymTracker.Application.Common.Importing;

public sealed class ExerciceCsvImportService
{
    private readonly CsvImportService _csvImportService;
    private readonly ExerciceImportDefinition _definition;

    public ExerciceCsvImportService(CsvImportService csvImportService, ExerciceImportDefinition definition)
    {
        _csvImportService = csvImportService;
        _definition = definition;
    }

    public async Task<ImportExercicesResult> ImportAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var result = await _csvImportService.ImportAsync(csvStream, _definition, cancellationToken);

        return new ImportExercicesResult(
            result.TotalRows,
            result.ImportedRows,
            result.RejectedRows,
            result.Results.Select(x => new ImportExerciceRowResult(
                x.RowNumber,
                x.Code,
                x.Success,
                x.Error,
                x.Entity)).ToArray());
    }
}
