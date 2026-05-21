using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MusclesIncludeDeletedTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MusclesIncludeDeletedTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task List_ShouldHideDeletedByDefault_AndShowWhenIncludeDeletedIsTrue()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var code = $"DEL_{suffix}";

        var createResponse = await _client.PostAsJsonAsync("/api/admin/muscles", new
        {
            name = $"Deleted {suffix}",
            code,
            description = "Test",
            muscleGroupIds = new[] { "general" },
            active = true,
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await _client.DeleteAsync($"/api/admin/muscles/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var defaultListResponse = await _client.GetAsync("/api/admin/muscles");
        defaultListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var defaultBody = await defaultListResponse.Content.ReadFromJsonAsync<JsonElement>();
        defaultBody.TryGetProperty("items", out var defaultRows).Should().BeTrue();
        var defaultCodes = defaultRows
            .EnumerateArray()
            .Where(row => row.TryGetProperty("code", out _))
            .Select(row => row.GetProperty("code").GetString())
            .ToArray();
        defaultCodes.Should().NotContain(code);

        var includeDeletedResponse = await _client.GetAsync("/api/admin/muscles?includeDeleted=true");
        includeDeletedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var includeDeletedBody = await includeDeletedResponse.Content.ReadFromJsonAsync<JsonElement>();
        includeDeletedBody.TryGetProperty("items", out var includeDeletedRows).Should().BeTrue();
        var includeDeletedCodes = includeDeletedRows
            .EnumerateArray()
            .Where(row => row.TryGetProperty("code", out _))
            .Select(row => row.GetProperty("code").GetString())
            .ToArray();
        includeDeletedCodes.Should().Contain(code);
    }
}
