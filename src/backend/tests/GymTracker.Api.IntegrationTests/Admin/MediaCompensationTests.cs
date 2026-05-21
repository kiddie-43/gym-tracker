using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MediaCompensationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MediaCompensationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task ConfirmMedia_ShouldReturnValidationProblem_WhenExerciseDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/exercises/missing-exercise/media", new
        {
            mediaId = "m1",
            mediaType = 0,
            storagePath = "admin/exercises/missing/m1/front.webp",
            thumbnailPath = (string?)null,
            contentType = "image/webp",
            fileName = "front.webp",
            sizeBytes = 1024,
            title = "Frontal",
            sortOrder = 0,
            isPrimary = true,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
