using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MusclesCanonicalEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MusclesCanonicalEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateDeleteAndReactivate()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var createRequest = new
        {
            name = $"Musculo {suffix}",
            code = $"MUSCLE_{suffix}",
            description = "Test",
            muscleGroupIds = new[] { "general" },
            active = true,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/muscles", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var updateRequest = new
        {
            name = $"Musculo Updated {suffix}",
            code = $"MUSCLE_{suffix}",
            description = "Updated",
            muscleGroupIds = new[] { "general" },
            active = true,
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/muscles/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/muscles/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reactivateResponse = await _client.PostAsync($"/api/admin/muscles/{id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
