using System.Net.Http.Json;
using RailcarTrips.Shared.Models;

namespace RailcarTrips.Client.Services;

/// <summary>
/// Client-side service for communicating with the Railcar Trips API.
/// </summary>
public class RailcarTripsApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<RailcarTripsApiService> _logger;

    public RailcarTripsApiService(HttpClient http, ILogger<RailcarTripsApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Upload an equipment events CSV file for processing.
    /// </summary>
    public async Task<UploadResultDto?> UploadEventsFileAsync(Stream fileStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        _logger.LogInformation("Uploading file '{FileName}' ({Size} bytes) to api/upload/events",
            fileName, memoryStream.Length);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(memoryStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync("api/upload/events", content);

        _logger.LogInformation("Upload response: {StatusCode}", response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new UploadResultDto
            {
                Success = false,
                ErrorMessage = $"Server returned empty response with status {response.StatusCode}"
            };
        }

        try
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<UploadResultDto>(responseBody,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
        }
        catch (System.Text.Json.JsonException)
        {
            return new UploadResultDto
            {
                Success = false,
                ErrorMessage = $"Upload failed ({response.StatusCode}): {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}"
            };
        }
    }

    /// <summary>
    /// Get all processed trips.
    /// </summary>
    public async Task<List<TripDto>> GetTripsAsync()
    {
        return await _http.GetFromJsonAsync<List<TripDto>>("api/trips") ?? new();
    }

    /// <summary>
    /// Get a single trip with its ordered events.
    /// </summary>
    public async Task<TripDetailDto?> GetTripDetailAsync(int tripId)
    {
        return await _http.GetFromJsonAsync<TripDetailDto>($"api/trips/{tripId}");
    }
}
