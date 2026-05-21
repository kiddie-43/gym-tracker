using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class ExerciseMediaEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExerciseMediaEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task MediaWorkflow_ShouldRequestUploadConfirmReorderSetPrimaryAndDelete()
    {
        var exerciseId = await CreateExerciseAsync();

        var uploadResponse = await _client.PostAsJsonAsync($"/api/admin/exercises/{exerciseId}/media/upload-url", new
        {
            mediaType = 0,
            fileName = "front.webp",
            contentType = "image/webp",
            sizeBytes = 1024,
            title = "Frontal",
        });

        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadTicket = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>();
        var mediaId = uploadTicket!.GetProperty("mediaId").GetString();
        var storagePath = uploadTicket.GetProperty("storagePath").GetString();

        var confirmResponse = await _client.PostAsJsonAsync($"/api/admin/exercises/{exerciseId}/media", new
        {
            mediaId,
            mediaType = 0,
            storagePath,
            thumbnailPath = (string?)null,
            contentType = "image/webp",
            fileName = "front.webp",
            sizeBytes = 1024,
            title = "Frontal",
            sortOrder = 0,
            isPrimary = true,
        });

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var reorderResponse = await _client.PatchAsJsonAsync($"/api/admin/exercises/{exerciseId}/media", new
        {
            items = new[]
            {
                new { mediaId, sortOrder = 0 },
            },
        });
        reorderResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var setPrimaryResponse = await _client.PatchAsJsonAsync($"/api/admin/exercises/{exerciseId}/media/set-primary", new
        {
            mediaId,
        });
        setPrimaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/exercises/{exerciseId}/media/{mediaId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> CreateExerciseAsync()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/admin/exercises", new
        {
            name = "Remo",
            code = $"ROW_{Guid.NewGuid():N}"[..12],
            description = "Ejercicio",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = "form-strength",
            formTypeCode = "STRENGTH_BASIC",
            primaryMuscleIds = new[] { "back" },
            secondaryMuscleIds = new[] { "biceps" },
            muscleGroupIds = new[] { "upper-body" },
            active = true,
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        return payload!.GetProperty("id").GetString()!;
    }
}
