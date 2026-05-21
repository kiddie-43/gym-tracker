using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesCrudTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesCrudTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateDeleteAndReactivateMeasurementType()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var createRequest = new
        {
            key = $"WEIGHT_{suffix}",
            name = $"Peso {suffix}",
            unit = "kg",
            dataType = "decimal",
            category = "strength",
            description = "Carga principal",
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/measurement-types", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.TryGetProperty("id", out var idProperty).Should().BeTrue();
        var id = idProperty.GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var updateRequest = new
        {
            key = createRequest.key,
            name = $"Peso actualizado {suffix}",
            unit = "kg",
            dataType = "decimal",
            category = "strength",
            description = "Carga secundaria",
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/measurement-types/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/measurement-types/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reactivateResponse = await _client.PostAsync($"/api/admin/measurement-types/{id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await _client.GetAsync("/api/admin/measurement-types?includeInactive=true");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("items", out var items).Should().BeTrue();
        items.ValueKind.Should().Be(JsonValueKind.Array);
        items.EnumerateArray().Any(item =>
            item.TryGetProperty("id", out var rowId) &&
            string.Equals(rowId.GetString(), id, StringComparison.Ordinal)).Should().BeTrue();
    }
}
