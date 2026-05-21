using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesImportCsvVolumeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesImportCsvVolumeTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task ImportCsv_ShouldProcessAtLeast95PercentValidRows_WhenImporting200Rows()
    {
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        var rows = new List<object>(200);
        for (var i = 0; i < 190; i++)
        {
            rows.Add(new
            {
                key = $"VOL_{suffix}_{i:000}",
                name = $"Volumen {i}",
                unit = "kg",
                dataType = "decimal",
                category = "strength",
                description = "row",
            });
        }

        for (var i = 0; i < 10; i++)
        {
            rows.Add(new
            {
                key = $"INV_{suffix}_{i:000}",
                name = $"Invalida {i}",
                unit = "kg",
                dataType = "float",
                category = "strength",
                description = "invalid datatype",
            });
        }

        var response = await _client.PostAsJsonAsync("/api/admin/measurement-types/import-csv", new { rows });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("totalRows").GetInt32().Should().Be(200);

        var createdRows = payload.GetProperty("createdRows").GetInt32();
        createdRows.Should().BeGreaterThanOrEqualTo(190);

        var processedRate = createdRows / 200.0;
        processedRate.Should().BeGreaterThanOrEqualTo(0.95);
    }
}
