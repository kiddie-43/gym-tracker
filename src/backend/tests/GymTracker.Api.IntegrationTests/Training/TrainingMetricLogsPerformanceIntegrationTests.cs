using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingMetricLogsPerformanceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TrainingMetricLogsPerformanceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-training-metric-owner");
    }

    [Fact]
    public async Task ListAndCurrentDay_ShouldMeetP95BelowTwoSeconds()
    {
        var suffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var routineId = $"routine-perf-{suffix}";
        var sessionId = $"session-perf-{suffix}";
        var trainingId = $"training-perf-{suffix}";
        var exerciseId = $"exercise-perf-{suffix}";

        var seedResponse = await _client.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: routineId,
                SessionId: sessionId,
                TrainingId: trainingId,
                ExerciseId: exerciseId,
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Metrics: new[]
                {
                    new GroupMetricValueInputDto("metric-perf", 12),
                }));

        seedResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await _client.GetAsync($"/api/training-metric-logs?routineId={routineId}&page=0&pageSize=20");
        await _client.GetAsync($"/api/training-metric-logs/current-day?routineId={routineId}&sessionId={sessionId}&trainingId={trainingId}&exerciseId={exerciseId}");

        var listDurations = await MeasureEndpointDurationsAsync(
            $"/api/training-metric-logs?routineId={routineId}&sessionId={sessionId}&trainingId={trainingId}&exerciseId={exerciseId}&page=0&pageSize=20",
            iterations: 20);

        var currentDayDurations = await MeasureEndpointDurationsAsync(
            $"/api/training-metric-logs/current-day?routineId={routineId}&sessionId={sessionId}&trainingId={trainingId}&exerciseId={exerciseId}",
            iterations: 20);

        var listP95 = CalculateP95(listDurations);
        var currentDayP95 = CalculateP95(currentDayDurations);

        listP95.Should().BeLessThan(2000);
        currentDayP95.Should().BeLessThan(2000);
    }

    private async Task<IReadOnlyCollection<double>> MeasureEndpointDurationsAsync(string url, int iterations)
    {
        var durations = new List<double>(iterations);

        for (var i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(url);
            stopwatch.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            durations.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        return durations;
    }

    private static double CalculateP95(IReadOnlyCollection<double> values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var p95Index = (int)Math.Ceiling(0.95 * sorted.Length) - 1;
        return sorted[Math.Max(0, p95Index)];
    }
}
