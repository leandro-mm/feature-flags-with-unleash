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

    private string? FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            // check for solution file or .git folder as repo root markers
            if (File.Exists(Path.Combine(dir.FullName, "FeatureFlag.slnx")) || Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
    private string? GetApiKeyFromAppSettings()
    {
        return _configuration["EBird:ApiKey"];
    }

    private string? GetSecretFileFromDocker()
    {
        return Environment.GetEnvironmentVariable("EBIRD__APIKEY_FILE") ??
                         Environment.GetEnvironmentVariable("EBIRD_APIKEY_FILE");
    }

    private async Task<string?> GetApiKeyFromSecretsFolder(CancellationToken cancellationToken)
    {
        var repoRoot = FindRepositoryRoot();

        if (!string.IsNullOrEmpty(repoRoot))
        {
            var repoSecretPath = Path.Combine(repoRoot, "secrets", "ebird_api_key.txt");
            if (File.Exists(repoSecretPath))
            {
                _logger.LogInformation("Reading API key from repo-local secrets file: {RepoSecret}", repoSecretPath);
                return (await File.ReadAllTextAsync(repoSecretPath, cancellationToken)).Trim();
            }
        }

        // Fallback to project-local secrets file 
        var localSecretPath = Path.Combine(Directory.GetCurrentDirectory(), "secrets", "ebird_api_key.txt");
        if (File.Exists(localSecretPath))
        {
            _logger.LogInformation("Reading API key from local secrets file: {LocalSecret}", localSecretPath);
            return (await File.ReadAllTextAsync(localSecretPath, cancellationToken)).Trim();
        }
        return null;
    }
    private async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken)
    {
        var apiKey = GetApiKeyFromAppSettings();

        if (!string.IsNullOrWhiteSpace(apiKey))
            return apiKey.Trim();

        var secretFile = GetSecretFileFromDocker();

        if (!string.IsNullOrWhiteSpace(secretFile) && File.Exists(secretFile))
        {
            _logger.LogInformation("Reading API key from file: {SecretFile}", secretFile);
            return (await File.ReadAllTextAsync(secretFile, cancellationToken)).Trim();
        }

        // Look for a secrets file at the repository root (e.g. ../secrets/ebird_api_key.txt)
        apiKey = await GetApiKeyFromSecretsFolder(cancellationToken);

        if (!string.IsNullOrWhiteSpace(apiKey))
            return apiKey.Trim();

        return null;
    }

    public async Task<List<EBirdApiResponse?>?> GetBirdDataByRegionAsync(string regionCode, CancellationToken cancellationToken)
    {
        try
        {
            var region = regionCode;
            var apiKey = await GetApiKeyAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("No API key found in configuration or secret file");
                throw new InvalidOperationException("eBird API key is not configured");
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