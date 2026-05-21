using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesHistoricalTraceabilityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesHistoricalTraceabilityTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Delete_ShouldKeepHistoricalRecordInIncludeInactive_AndExcludeFromAssignable()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var createResponse = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
        {
            key = $"TRACE_{suffix}",
            name = $"Trazable {suffix}",
            unit = "kg",
            dataType = "decimal",
            category = "strength",
            description = "historico",
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await _client.DeleteAsync($"/api/admin/measurement-types/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var includeInactiveResponse = await _client.GetAsync("/api/admin/measurement-types?includeInactive=true");
        includeInactiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var includeInactiveBody = await includeInactiveResponse.Content.ReadFromJsonAsync<JsonElement>();
        includeInactiveBody.TryGetProperty("items", out var includeInactiveRows).Should().BeTrue();

        includeInactiveRows
            .EnumerateArray()
            .Any(row => row.TryGetProperty("id", out var rowId) && string.Equals(rowId.GetString(), id, StringComparison.Ordinal))
            .Should().BeTrue();

        var assignableResponse = await _client.GetAsync("/api/admin/measurement-types/assignable");
        assignableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignableRows = await assignableResponse.Content.ReadFromJsonAsync<JsonElement[]>();

        assignableRows!
            .Any(row => row.TryGetProperty("id", out var rowId) && string.Equals(rowId.GetString(), id, StringComparison.Ordinal))
            .Should().BeFalse();
    }
}
