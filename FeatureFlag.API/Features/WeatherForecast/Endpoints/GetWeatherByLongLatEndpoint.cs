using MediatR;
using FeatureFlag.API.Features.WeatherForecast.Queries;

namespace FeatureFlag.API.Features.WeatherForecast.Endpoints;

public static class GetWeatherByLongLatEndpoint
{
    public static void MapGetWeatherByLongLatEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/weatherforecast/longlat", async (
            IMediator mediator,
            double longitude,
            double latitude) =>
        {
            var query = new GetWeatherByLongLatQuery(longitude, latitude);
            var forecast = await mediator.Send(query);
            return forecast;
        })
        .WithName("GetWeatherByLongLat")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status404NotFound);
    }
}