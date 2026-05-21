using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class ExerciseFormTypesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExerciseFormTypesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task CanonicalMeasurementTypes_ShouldCreateUpdateDeleteAndReactivate()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var createRequest = new
        {
            name = $"Fuerza Basica {suffix}",
            fields = new[]
            {
                new
                {
                    name = "Repeticiones",
                },
            },
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/measurement-types", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.TryGetProperty("id", out var idProperty).Should().BeTrue();
        var id = idProperty.GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var listResponse = await _client.GetAsync("/api/admin/measurement-types");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest = new
        {
            name = $"Fuerza Basica v2 {suffix}",
            fields = new[]
            {
                new
                {
                    name = "Repeticiones",
                },
            },
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/measurement-types/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/measurement-types/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reactivateResponse = await _client.PostAsync($"/api/admin/measurement-types/{id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
