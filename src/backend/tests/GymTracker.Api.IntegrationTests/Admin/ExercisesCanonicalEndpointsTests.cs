using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class ExercisesCanonicalEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExercisesCanonicalEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateSoftDeleteAndReactivateExercise()
    {
        await EnsureReferenceCatalogAsync();

        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var createRequest = new
        {
            name = $"Press banca {uniqueSuffix}",
            code = $"BENCH_{uniqueSuffix}",
            description = "Ejercicio compuesto",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = "form-strength",
            formTypeCode = "STRENGTH_BASIC",
            primaryMuscleIds = new[] { "chest" },
            secondaryMuscleIds = new[] { "triceps" },
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/exercises", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, await createResponse.Content.ReadAsStringAsync());

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var deleteResponse = await _client.DeleteAsync($"/api/admin/exercises/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listDefault = await _client.GetAsync("/api/admin/exercises");
        listDefault.StatusCode.Should().Be(HttpStatusCode.OK);
        var defaultRows = await listDefault.Content.ReadFromJsonAsync<JsonElement>();
        defaultRows.GetProperty("items").EnumerateArray().Any(item => string.Equals(item.GetProperty("id").GetString(), id, StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse();

        var reactivateResponse = await _client.PostAsync($"/api/admin/exercises/{id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK, await reactivateResponse.Content.ReadAsStringAsync());

        var listAfterReactivate = await _client.GetAsync("/api/admin/exercises");
        listAfterReactivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var activeRows = await listAfterReactivate.Content.ReadFromJsonAsync<JsonElement>();
        activeRows.GetProperty("items").EnumerateArray().Any(item => string.Equals(item.GetProperty("id").GetString(), id, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    private async Task EnsureReferenceCatalogAsync()
    {
        var chestCreate = await _client.PostAsJsonAsync("/api/admin/muscles", new
        {
            name = "Chest",
            code = "chest",
            description = "Musculo pectoral",
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        });

        chestCreate.StatusCode.Should().Match(
            status => status == HttpStatusCode.Created || status == HttpStatusCode.Conflict,
            await chestCreate.Content.ReadAsStringAsync());

        var tricepsCreate = await _client.PostAsJsonAsync("/api/admin/muscles", new
        {
            name = "Triceps",
            code = "triceps",
            description = "Musculo triceps",
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        });

        tricepsCreate.StatusCode.Should().Match(
            status => status == HttpStatusCode.Created || status == HttpStatusCode.Conflict,
            await tricepsCreate.Content.ReadAsStringAsync());

        var measurementTypeCreate = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
        {
            name = "Strength Basic",
            fields = new[]
            {
                new { name = "Repeticiones" },
            },
        });

        measurementTypeCreate.StatusCode.Should().Match(
            status => status == HttpStatusCode.Created || status == HttpStatusCode.Conflict,
            await measurementTypeCreate.Content.ReadAsStringAsync());
    }
}
