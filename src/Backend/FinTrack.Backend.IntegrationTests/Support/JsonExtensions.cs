using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinTrack.Application.Common.Models;

namespace FinTrack.Backend.IntegrationTests.Support;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_options);
    }

    public static async Task<ApiResponse<PagedResponse<T>>?> ReadPagedApiResponseAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<T>>>(_options);
    }
}
