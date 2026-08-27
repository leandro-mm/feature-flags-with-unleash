using MediatR;
using FeatureFlag.API.Features.WeatherForecast.Queries;
using FeatureFlag.API.Features.WeatherForecast.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlag.API.Features.WeatherForecast.Endpoints;

public static class GetWeatherByLongLatEndpoint
{
    public static void MapGetWeatherByLongLatEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/weatherforecast/longlat", async (
            IMediator mediator,
            [FromBody] WeatherForecastRequestDto request) =>
        {
            var query = new GetWeatherByLongLatQuery(
                Longitude: request.Longitude,
                Latitude: request.Latitude,
                UserName: request.UserName);

            var forecast = await mediator.Send(query);
            return forecast;
        })
        .WithName("GetWeatherByLongLat")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status404NotFound);
    }
}