using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class ExercisesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExercisesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateListAndDeleteExercise()
    {
        await EnsureReferenceCatalogAsync();

        var createRequest = new
        {
            name = "Press banca",
            code = "BENCH_PRESS",
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
        var createResponseBody = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"response body: {createResponseBody}");
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var listResponse = await _client.GetAsync("/api/admin/exercises");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest = new
        {
            name = "Press banca plano",
            code = "BENCH_PRESS_FLAT",
            description = "Actualizado",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = "form-strength",
            formTypeCode = "STRENGTH_BASIC",
            primaryMuscleIds = new[] { "chest" },
            secondaryMuscleIds = new[] { "triceps" },
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/exercises/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/exercises/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ImportCsv_ShouldCreateRowsAndReportRowResults()
    {
        await EnsureReferenceCatalogAsync();

        var importRequest = new
        {
            rows = new[]
            {
                new
                {
                    code = $"CSV_EX_{Guid.NewGuid():N}".ToUpperInvariant(),
                    name = "Row valida",
                    description = (string?)"Importada por CSV",
                    category = "Strength",
                    difficulty = "Beginner",
                    formTypeId = "form-strength",
                    primaryMuscleIds = new[] { "chest" },
                    secondaryMuscleIds = new[] { "triceps" },
                },
                new
                {
                    code = string.Empty,
                    name = "Row invalida",
                    description = (string?)null,
                    category = "Strength",
                    difficulty = "Beginner",
                    formTypeId = "form-strength",
                    primaryMuscleIds = new[] { "chest" },
                    secondaryMuscleIds = new[] { string.Empty },
                },
            },
        };

        var response = await _client.PostAsJsonAsync("/api/admin/exercises/import-csv", importRequest);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var payload = JsonDocument.Parse(body).RootElement;
        payload.GetProperty("totalRows").GetInt32().Should().Be(2);
        payload.GetProperty("createdRows").GetInt32().Should().Be(1);
        payload.GetProperty("rejectedRows").GetInt32().Should().Be(1);
        payload.GetProperty("rows").GetArrayLength().Should().Be(2);
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
