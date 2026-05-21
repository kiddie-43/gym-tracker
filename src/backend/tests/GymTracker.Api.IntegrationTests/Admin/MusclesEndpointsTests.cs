using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MusclesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MusclesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateListAndDeleteMuscle()
    {
        var createRequest = new
        {
            name = "Pectoral Mayor",
            code = "PECTORALIS_MAJOR",
            description = "Musculo principal de empuje",
            muscleGroupIds = new[] { "chest" },
            active = true,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/muscles", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.TryGetProperty("id", out var idProperty).Should().BeTrue();
        var id = idProperty.GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var listResponse = await _client.GetAsync("/api/admin/muscles?page=1&pageSize=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest = new
        {
            name = "Pectoral Mayor Superior",
            code = "PECTORALIS_MAJOR_UPPER",
            description = "Musculo de empuje actualizado",
            muscleGroupIds = new[] { "chest" },
            active = true,
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/muscles/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/muscles/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
