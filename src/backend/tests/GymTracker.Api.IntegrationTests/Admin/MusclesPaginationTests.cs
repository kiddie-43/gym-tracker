using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MusclesPaginationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MusclesPaginationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task List_ShouldPaginateByPageAndPageSize()
    {
        var prefix = $"PAGM_{Guid.NewGuid():N}"[..13].ToUpperInvariant();

        foreach (var suffix in new[] { "A", "B", "C" })
        {
            var createResponse = await _client.PostAsJsonAsync("/api/admin/muscles", new
            {
                name = $"{prefix}_{suffix}",
                code = $"{prefix}_{suffix}",
                description = "Pagination test",
                muscleGroupIds = new[] { "general" },
                active = true,
            });

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var page1Response = await _client.GetAsync($"/api/admin/muscles?search={prefix}&sortBy=code&sortDirection=asc&page=1&pageSize=2");
        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page1Body = await page1Response.Content.ReadFromJsonAsync<JsonElement>();

        var page2Response = await _client.GetAsync($"/api/admin/muscles?search={prefix}&sortBy=code&sortDirection=asc&page=2&pageSize=2");
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page2Body = await page2Response.Content.ReadFromJsonAsync<JsonElement>();

        page1Body.GetProperty("totalCount").GetInt32().Should().Be(3);
        page1Body.GetProperty("page").GetInt32().Should().Be(1);
        page1Body.GetProperty("pageSize").GetInt32().Should().Be(2);

        page2Body.GetProperty("totalCount").GetInt32().Should().Be(3);
        page2Body.GetProperty("page").GetInt32().Should().Be(2);
        page2Body.GetProperty("pageSize").GetInt32().Should().Be(2);

        var page1Codes = page1Body.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("code").GetString()).ToArray();
        var page2Codes = page2Body.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("code").GetString()).ToArray();

        page1Codes.Should().HaveCount(2);
        page2Codes.Should().HaveCount(1);
        page2Codes[0].Should().NotBeNullOrWhiteSpace();
        page1Codes.Should().NotContain(page2Codes[0]);
    }
}
