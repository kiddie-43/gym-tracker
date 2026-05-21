using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesPaginationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesPaginationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task List_ShouldPaginateByPageAndPageSize()
    {
        var prefix = $"PAGMT_{Guid.NewGuid():N}"[..14].ToUpperInvariant();

        foreach (var suffix in new[] { "A", "B", "C" })
        {
            var createResponse = await _client.PostAsJsonAsync("/api/admin/measurement-types", new
            {
                key = $"{prefix}_{suffix}",
                name = $"{prefix}_{suffix}",
                unit = "kg",
                dataType = "decimal",
                category = "strength",
                description = "Pagination test",
            });

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var page1Response = await _client.GetAsync($"/api/admin/measurement-types?search={prefix}&sortBy=key&sortDirection=asc&page=1&pageSize=2");
        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page1Body = await page1Response.Content.ReadFromJsonAsync<JsonElement>();

        var page2Response = await _client.GetAsync($"/api/admin/measurement-types?search={prefix}&sortBy=key&sortDirection=asc&page=2&pageSize=2");
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page2Body = await page2Response.Content.ReadFromJsonAsync<JsonElement>();

        page1Body.GetProperty("totalCount").GetInt32().Should().Be(3);
        page1Body.GetProperty("page").GetInt32().Should().Be(1);
        page1Body.GetProperty("pageSize").GetInt32().Should().Be(2);

        page2Body.GetProperty("totalCount").GetInt32().Should().Be(3);
        page2Body.GetProperty("page").GetInt32().Should().Be(2);
        page2Body.GetProperty("pageSize").GetInt32().Should().Be(2);

        var page1Keys = page1Body.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("key").GetString()).ToArray();
        var page2Keys = page2Body.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("key").GetString()).ToArray();

        page1Keys.Should().HaveCount(2);
        page2Keys.Should().HaveCount(1);
        page2Keys[0].Should().NotBeNullOrWhiteSpace();
        page1Keys.Should().NotContain(page2Keys[0]);
    }
}
