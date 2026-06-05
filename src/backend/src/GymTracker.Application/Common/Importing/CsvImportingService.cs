using System.Text;

namespace GymTracker.Application.Common.Importing;

public sealed class CsvImportService
{
    private const int MaxRows = 10_000;

    public async Task<ImportResult<TResponse>> ImportAsync<TEntity, TResponse>(
        Stream csvStream,
        ICsvImportDefinition<TEntity, TResponse> definition,
        CancellationToken cancellationToken = default)
    {
        var rows = await ReadCsvAsync(csvStream, cancellationToken);

        return await ImportRowsAsync(rows, definition, cancellationToken);
    }

    private async Task<ImportResult<TResponse>> ImportRowsAsync<TEntity, TResponse>(
        IReadOnlyCollection<Dictionary<string, string?>> rows,
        ICsvImportDefinition<TEntity, TResponse> definition,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ImportRowResult<TResponse>>(rows.Count);
        var importedRows = 0;

        var existingCodes = await definition.GetExistingCodesAsync(cancellationToken);
        var batchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rowNumber = 1;

        foreach (var row in rows)
        {
            var code = CsvValueSanitizer.Clean(row.GetValueOrDefault(definition.CodeColumn));

            if (string.IsNullOrWhiteSpace(code))
            {
                results.Add(new(rowNumber, code, false, "Code is required.", default));
                rowNumber++;
                continue;
            }

            code = code.ToUpperInvariant();

            if (!CsvValueSanitizer.IsSafe(code))
            {
                results.Add(new(rowNumber, code, false, "Code contains invalid characters.", default));
                rowNumber++;
                continue;
            }

            if (!batchCodes.Add(code))
            {
                results.Add(new(rowNumber, code, false, "Duplicate code in CSV.", default));
                rowNumber++;
                continue;
            }

            if (existingCodes.Contains(code))
            {
                results.Add(new(rowNumber, code, false, "Duplicate code in database.", default));
                rowNumber++;
                continue;
            }

            var cleanRow = CsvValueSanitizer.CleanRow(row);
            cleanRow[definition.CodeColumn] = code;

            try
            {
                var entity = definition.CreateEntity(cleanRow);

                await definition.SaveAsync(entity, cancellationToken);

                existingCodes.Add(code);
                importedRows++;

                results.Add(new(rowNumber, code, true, null, definition.Map(entity)));
            }
            catch (Exception ex)
            {
                results.Add(new(rowNumber, code, false, ex.Message, default));
            }

            rowNumber++;
        }

        return new ImportResult<TResponse>(
            rows.Count,
            importedRows,
            rows.Count - importedRows,
            results);
    }

    private static async Task<IReadOnlyCollection<Dictionary<string, string?>>> ReadCsvAsync(
        Stream csvStream,
        CancellationToken cancellationToken)
    {
        if (csvStream.CanSeek)
            csvStream.Position = 0;

        using var reader = new StreamReader(
            csvStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(headerLine))
            return [];

        var headers = ParseCsvLine(headerLine)
            .Select(x => CsvValueSanitizer.Clean(x)?.ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var rows = new List<Dictionary<string, string?>>();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (rows.Count >= MaxRows)
                throw new InvalidOperationException($"CSV exceeds maximum allowed rows: {MaxRows}.");

            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);

            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                if (string.IsNullOrWhiteSpace(header))
                    continue;

                row[header] = i < values.Count ? values[i] : null;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<string?> ParseCsvLine(string line)
    {
        var values = new List<string?>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }

                continue;
            }

            if (c == ',' && !insideQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        values.Add(current.ToString());

        return values;
    }
}