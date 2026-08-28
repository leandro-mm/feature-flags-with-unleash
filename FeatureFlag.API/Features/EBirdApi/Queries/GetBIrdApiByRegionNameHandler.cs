using FeatureFlag.API.Features.EBirdApi.DTOs;
using FeatureFlag.API.Features.EBirdApi.Queries;
using FeatureFlag.API.Features.EBirdApi.Services;
using MediatR;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Text.Json;
using System.Linq;


namespace FeatureFlag.API.Features.EBirdApi.Queries;

public class GetBirdApiByRegionNameHandler : IRequestHandler<GetBirdApiByRegionNameQuery, EBirdApiResponse?>
{
    private readonly IEBirdApiService _eBirdApiService;
    private readonly ILogger<GetBirdApiByRegionNameHandler> _logger;

    public GetBirdApiByRegionNameHandler(
                IEBirdApiService eBirdApiService,
                ILogger<GetBirdApiByRegionNameHandler> logger)
    {
        _eBirdApiService = eBirdApiService;
        _logger = logger;
    }

    public async Task<EBirdApiResponse?> Handle(GetBirdApiByRegionNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var region = request.RegionCode;
            var userName = request.UserName;

            _logger.LogInformation($"Processing request for region: {region}");

            var observations = await _eBirdApiService.GetBirdDataByRegionAsync(region, cancellationToken);

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