using System.Net.Http.Json;
using System.Text.Json.Serialization;

using GymTracker.Application.Catalog;

using Microsoft.Extensions.Options;

namespace GymTracker.Infrastructure.Wger;

public sealed class WgerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _defaultLanguageCode;
    private readonly WgerCatalogMapper _wgerCatalogMapper;

    public WgerApiClient(HttpClient httpClient, IOptions<WgerOptions> options, WgerCatalogMapper wgerCatalogMapper)
    {
        _httpClient = httpClient;
        var wgerOptions = options.Value;
        _baseUrl = string.IsNullOrWhiteSpace(wgerOptions.BaseUrl) ? "https://wger.de/api/v2/" : wgerOptions.BaseUrl;
        _apiKey = wgerOptions.ApiKey ?? string.Empty;
        _defaultLanguageCode = string.IsNullOrWhiteSpace(wgerOptions.LanguageCode) ? "es" : wgerOptions.LanguageCode;
        _wgerCatalogMapper = wgerCatalogMapper;
    }

    public async Task<IReadOnlyCollection<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<WgerCollectionResponse<WgerNamedItem>>("muscle/", cancellationToken);
        return response?.Results.Select(item => _wgerCatalogMapper.MapMuscleGroup(item.Id, item.Name)).ToArray()
            ?? Array.Empty<MuscleGroupDto>();
    }

    public async Task<IReadOnlyCollection<ExerciseDto>> GetExercisesAsync(string? languageCode, string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default)
    {
        var effectiveLanguageCode = ResolveLanguageCode(languageCode);
        var queryParts = new List<string>();
        queryParts.Add($"language__code={Uri.EscapeDataString(effectiveLanguageCode)}");
        queryParts.Add("limit=200");

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryParts.Add($"name__search={Uri.EscapeDataString(query)}");
        }

        if (muscleGroupIds is { Count: > 0 })
        {
            queryParts.Add($"muscles__in={string.Join(',', muscleGroupIds)}");
        }

        var route = $"exerciseinfo/?{string.Join('&', queryParts)}";
        var response = await GetAsync<WgerCollectionResponse<WgerExerciseItem>>(route, cancellationToken);

        return response?.Results.Select(item =>
            _wgerCatalogMapper.MapExercise(
                item.Id,
                item.Name ?? item.Translations.FirstOrDefault(translation => !string.IsNullOrWhiteSpace(translation.Name))?.Name ?? $"Exercise {item.Id}",
                item.Muscles.Select(muscle => muscle.Id)
                    .Concat(item.MusclesSecondary.Select(muscle => muscle.Id))
                    .Distinct()
                    .ToArray(),
                item.Images.FirstOrDefault()?.Image))
            .ToArray()
            ?? Array.Empty<ExerciseDto>();
    }

    private string ResolveLanguageCode(string? requestedLanguageCode)
    {
        if (!string.IsNullOrWhiteSpace(requestedLanguageCode))
        {
            return requestedLanguageCode.Trim().ToLowerInvariant();
        }

        return _defaultLanguageCode.Trim().ToLowerInvariant();
    }

    public async Task<IReadOnlyCollection<FoodDto>> GetFoodsAsync(string query, CancellationToken cancellationToken = default)
    {
        var route = $"ingredient/?search={Uri.EscapeDataString(query)}";
        var response = await GetAsync<WgerCollectionResponse<WgerFoodItem>>(route, cancellationToken);

        return response?.Results.Select(item => _wgerCatalogMapper.MapFood(item.Id, item.Name, item.Calories)).ToArray()
            ?? Array.Empty<FoodDto>();
    }

    private async Task<T?> GetAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_baseUrl), relativePath));

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            request.Headers.Add("Authorization", $"Token {_apiKey}");
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private sealed record WgerCollectionResponse<T>([property: JsonPropertyName("results")] IReadOnlyCollection<T> Results);

    private sealed record WgerNamedItem([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("name")] string Name);

    private sealed record WgerImageItem([property: JsonPropertyName("image")] string? Image);

    private sealed record WgerExerciseItem(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("muscles")] IReadOnlyCollection<WgerNamedItem> Muscles,
        [property: JsonPropertyName("muscles_secondary")] IReadOnlyCollection<WgerNamedItem> MusclesSecondary,
        [property: JsonPropertyName("translations")] IReadOnlyCollection<WgerExerciseTranslation> Translations,
        [property: JsonPropertyName("images")] IReadOnlyCollection<WgerImageItem> Images);

    private sealed record WgerExerciseTranslation([property: JsonPropertyName("name")] string Name);

    private sealed record WgerFoodItem(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("energy")] double? Calories);
}
