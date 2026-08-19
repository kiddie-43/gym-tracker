using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingMetricLogsHealthChecksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TrainingMetricLogsHealthChecksIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldExposeTrainingMetricLogsDependencyStatus()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().NotBeNullOrWhiteSpace();
        result.Entries.Should().ContainKey("training-metric-logs-dependencies");
        result.Entries["training-metric-logs-dependencies"].Status.Should().BeOneOf("Healthy", "Degraded");
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200WithDegradedStatus_WhenMetricDefinitionsDependencyIsDegraded()
    {
        using var factory = CreateFactoryForHealthStatus(
            HealthStatus.Degraded,
            "Metric definitions dependency is unavailable. Module runs in degraded mode.");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Degraded");
        result.Entries["training-metric-logs-dependencies"].Status.Should().Be("Degraded");
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn503_WhenCriticalSqlDependencyIsUnavailable()
    {
        using var factory = CreateFactoryForHealthStatus(
            HealthStatus.Unhealthy,
            "Training metric logs SQL tables are unavailable.");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Unhealthy");
        result.Entries["training-metric-logs-dependencies"].Status.Should().Be("Unhealthy");
    }

    private WebApplicationFactory<Program> CreateFactoryForHealthStatus(HealthStatus status, string description)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "training-metric-logs-dependencies",
                        _ => new StaticHealthCheck(status, description),
                        failureStatus: status,
                        tags: null,
                        timeout: default));
                });
            });
        });
    }

    private sealed record HealthResponse(string Status, IReadOnlyDictionary<string, HealthEntry> Entries);

    private sealed record HealthEntry(string Status, string? Description, double Duration, IReadOnlyDictionary<string, object?> Data);

    private sealed class StaticHealthCheck : IHealthCheck
    {
        private readonly HealthStatus _status;
        private readonly string _description;

        public StaticHealthCheck(HealthStatus status, string description)
        {
            _status = status;
            _description = description;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = _status switch
            {
                HealthStatus.Healthy => HealthCheckResult.Healthy(_description),
                HealthStatus.Degraded => HealthCheckResult.Degraded(_description),
                _ => HealthCheckResult.Unhealthy(_description),
            };

            return Task.FromResult(result);
        }
    }
}