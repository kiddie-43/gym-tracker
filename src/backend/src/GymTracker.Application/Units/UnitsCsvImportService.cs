using GymTracker.Application.Common.Importing;

namespace GymTracker.Application.Units;

public sealed class UnitsCsvImportService
{
    private readonly CsvImportService _csvImportService;
    private readonly UnitsImportDefinition _definition;

    public UnitsCsvImportService(
        CsvImportService csvImportService,
        UnitsImportDefinition definition)
    {
        _csvImportService = csvImportService;
        _definition = definition;
    }

    public async Task<ImportUnitsResult> ImportAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var result = await _csvImportService.ImportAsync(
            csvStream,
            _definition,
            cancellationToken);

        return new ImportUnitsResult(
            result.TotalRows,
            result.ImportedRows,
            result.RejectedRows,
            result.Results.Select(x => new ImportUnitRowResult(
                x.RowNumber,
                x.Code,
                x.Success,
                x.Error,
                x.Entity)).ToArray());
    }
}