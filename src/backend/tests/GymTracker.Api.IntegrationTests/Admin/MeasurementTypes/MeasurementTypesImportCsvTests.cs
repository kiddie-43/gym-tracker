using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesImportCsvTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesImportCsvTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task ImportCsv_ShouldReturnPartialResults_WhenPayloadContainsDuplicatesAndInvalidRows()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var key = $"CSV_{suffix}";

        var response = await _client.PostAsJsonAsync("/api/admin/measurement-types/import-csv", new
        {
            rows = new object[]
            {
                new { key, name = $"Peso {suffix}", unit = "kg", dataType = "decimal", category = "strength", description = "ok" },
                new { key, name = $"Peso dup {suffix}", unit = "kg", dataType = "decimal", category = "strength", description = "dup" },
                new { key = $"BAD_{suffix}", name = "Invalida", unit = "kg", dataType = "float", category = "strength", description = "bad datatype" },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("totalRows").GetInt32().Should().Be(3);
        payload.GetProperty("createdRows").GetInt32().Should().Be(1);
        payload.GetProperty("rejectedRows").GetInt32().Should().Be(2);

        var rows = payload.GetProperty("rows").EnumerateArray().ToArray();
        rows.Should().HaveCount(3);
        rows.Count(item => item.GetProperty("created").GetBoolean()).Should().Be(1);
    }
}
