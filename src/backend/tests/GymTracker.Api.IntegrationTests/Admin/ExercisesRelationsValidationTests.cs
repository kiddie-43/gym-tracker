using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class ExercisesRelationsValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExercisesRelationsValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPrimaryMuscleDoesNotExist()
    {
        await EnsureMeasurementTypeAsync();

        var request = new
        {
            name = "Peso muerto",
            code = $"DEADLIFT_{Guid.NewGuid():N}"[..17].ToUpperInvariant(),
            description = "Ejercicio compuesto",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = "form-strength",
            formTypeCode = "STRENGTH_BASIC",
            primaryMuscleIds = new[] { "missing-muscle" },
            secondaryMuscleIds = Array.Empty<string>(),
            muscleGroupIds = new[] { "lower-body" },
            active = true,
        };

        var response = await _client.PostAsJsonAsync("/api/admin/exercises", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenMeasurementTypeIsDeleted()
    {
        await EnsureMuscleAsync("chest", "Chest");

        var createdMeasurementType = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
        {
            name = $"Tempo {Guid.NewGuid():N}"[..14],
            fields = new[]
            {
                new { name = "Segundos" },
            },
        });

        createdMeasurementType.StatusCode.Should().Be(HttpStatusCode.Created, await createdMeasurementType.Content.ReadAsStringAsync());
        var createdPayload = await createdMeasurementType.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var measurementTypeId = createdPayload.GetProperty("id").GetString();
        measurementTypeId.Should().NotBeNullOrWhiteSpace();

        var deleteResponse = await _client.DeleteAsync($"/api/admin/measurement-types/{measurementTypeId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var request = new
        {
            name = "Press inclinado",
            code = $"INCLINE_{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            description = "Ejercicio compuesto",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = measurementTypeId,
            formTypeCode = "IGNORED",
            primaryMuscleIds = new[] { "chest" },
            secondaryMuscleIds = Array.Empty<string>(),
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        };

        var response = await _client.PostAsJsonAsync("/api/admin/exercises", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, await response.Content.ReadAsStringAsync());
    }

    private async Task EnsureMeasurementTypeAsync()
    {
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

    private async Task EnsureMuscleAsync(string code, string name)
    {
        var create = await _client.PostAsJsonAsync("/api/admin/muscles", new
        {
            name,
            code,
            description = "Musculo",
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        });

        create.StatusCode.Should().Match(
            status => status == HttpStatusCode.Created || status == HttpStatusCode.Conflict,
            await create.Content.ReadAsStringAsync());
    }
}
