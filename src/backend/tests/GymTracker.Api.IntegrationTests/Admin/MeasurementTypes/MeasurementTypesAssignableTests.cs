using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesAssignableTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesAssignableTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Assignable_ShouldReturnOnlyActiveMeasurementTypes()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var activeCreate = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
        {
            key = $"ASSIGN_A_{suffix}",
            name = $"Activa {suffix}",
            unit = "kg",
            dataType = "decimal",
            category = "strength",
            description = "activa",
        });
        activeCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        var activeId = (await activeCreate.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        var inactiveCreate = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
        {
            key = $"ASSIGN_B_{suffix}",
            name = $"Inactiva {suffix}",
            unit = "min",
            dataType = "time",
            category = "cardio",
            description = "inactiva",
        });
        inactiveCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        var inactiveId = (await inactiveCreate.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        var deleteInactive = await _client.DeleteAsync($"/api/admin/measurement-types/{inactiveId}");
        deleteInactive.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var assignableResponse = await _client.GetAsync("/api/admin/measurement-types/assignable");
        assignableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var rows = await assignableResponse.Content.ReadFromJsonAsync<JsonElement[]>();
        rows.Should().NotBeNull();

        var ids = rows!
            .Where(row => row.TryGetProperty("id", out _))
            .Select(row => row.GetProperty("id").GetString())
            .ToArray();

        ids.Should().Contain(activeId);
        ids.Should().NotContain(inactiveId);
    }
}
