using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FeatureFlag.API.Features.EBirdApi.DTOs;

namespace FeatureFlag.API.Features.EBirdApi.Services;

public class EbirdApiService : IEBirdApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EbirdApiService> _logger;
    private readonly IConfiguration _configuration;

    public EbirdApiService(HttpClient httpClient, ILogger<EbirdApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<EBirdApiResponse?>?> GetBirdDataByRegionAsync(string regionCode, CancellationToken cancellationToken)
    {
        try
        {
            var region = regionCode;
            var apiKey = _configuration["EBird:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                var secretFile = Environment.GetEnvironmentVariable("EBIRD__APIKEY_FILE") ?? Environment.GetEnvironmentVariable("EBIRD_APIKEY_FILE");
                if (!string.IsNullOrWhiteSpace(secretFile) && File.Exists(secretFile))
                {
                    apiKey = (await File.ReadAllTextAsync(secretFile, cancellationToken)).Trim();
                }
            }

            var path = $"data/obs/{region}/recent?key={apiKey}";

            _logger.LogInformation("Calling eBird API for region: {Region}", region);

            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("eBird API returned error: {StatusCode} Error {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to get bird data from eBird API: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            var observations = JsonSerializer.Deserialize<List<EBirdApiResponse?>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (observations == null || observations.Count == 0)
            {
                _logger.LogWarning("No observations found for region: {Region}", region);
                return new List<EBirdApiResponse?>();
            }

            _logger.LogInformation("Successfully retrieved {Count} observations for region: {Region}", observations.Count, region);

            return observations;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error occurred while fetching bird data from eBird API.");
            throw;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error occurred while processing eBird API response.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching bird data from eBird API.");
            throw;
        }
    }
}