using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MusclesCsvImportEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MusclesCsvImportEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task ImportCsv_ShouldReturnPartialResults_WhenPayloadContainsDuplicates()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var code = $"CSV_{suffix}";

        var response = await _client.PostAsJsonAsync("/api/admin/muscles/import-csv", new
        {
            rows = new object[]
            {
                new { code, description = "Ok row" },
                new { code, description = "Duplicate row" },
                new { code = "", description = "Missing code" },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("totalRows").GetInt32().Should().Be(3);
        payload.GetProperty("importedRows").GetInt32().Should().Be(1);
        payload.GetProperty("rejectedRows").GetInt32().Should().Be(2);

        var results = payload.GetProperty("results").EnumerateArray().ToArray();
        results.Should().HaveCount(3);
        results.Count(item => item.GetProperty("imported").GetBoolean()).Should().Be(1);
    }
}
