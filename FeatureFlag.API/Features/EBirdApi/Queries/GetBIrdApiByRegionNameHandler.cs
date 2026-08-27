using FeatureFlag.API.Features.EBirdApi.DTOs;
using FeatureFlag.API.Features.EBirdApi.Queries;
using MediatR;
using System.Net.Http;
using System.Text.Json;

namespace FeatureFlag.API.Features.EBirdApi.Queries;

public class GetBirdApiByRegionNameHandler : IRequestHandler<GetBirdApiByRegionNameQuery, EBirdApiResponse?>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetBirdApiByRegionNameHandler> _logger;

    public GetBirdApiByRegionNameHandler(
                IHttpClientFactory httpClientFactory,
                ILogger<GetBirdApiByRegionNameHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<EBirdApiResponse?> Handle(GetBirdApiByRegionNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var region = request.RegionCode;
            var userName = request.UserName;
            var apiKey = "626f7fca-a2e2-4c42-8f6f-398afcbcb713";
            var url = $"https://api.ebird.org/v2/data/obs/{region}/recent?key={apiKey}";
            var client = _httpClientFactory.CreateClient();

            _logger.LogInformation($"Calling eBird API for region: {region}");

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"eBird API returned error: {response.StatusCode}");
                throw new HttpRequestException($"Failed to get bird data from eBird API: {response.StatusCode}");
            }

            // Read the response content
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            var observations = JsonSerializer.Deserialize<List<EBirdApiResponse>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (observations == null || observations.Count == 0)
            {
                _logger.LogWarning($"No observations found for region: {region}");
                return null;
            }

            // Get the first observation and map to EBirdApiResponse
            var firstObservation = observations.First();

            var result = new EBirdApiResponse(
                         SpeciesCode: firstObservation.SpeciesCode,
                         ComName: firstObservation.ComName,
                         SciName: firstObservation.SciName,
                         LocId: firstObservation.LocId,
                         LocName: firstObservation.LocName,
                         ObsDt: firstObservation.ObsDt,
                         HowMany: firstObservation.HowMany,
                         Lat: firstObservation.Lat,
                         Lng: firstObservation.Lng,
                         ObsValid: firstObservation.ObsValid,
                         ObsReviewed: firstObservation.ObsReviewed,
                         LocationPrivate: firstObservation.LocationPrivate,
                         SubId: firstObservation.SubId
                     );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching bird data for region: {request.RegionCode}");
            throw;
        }

    }
}