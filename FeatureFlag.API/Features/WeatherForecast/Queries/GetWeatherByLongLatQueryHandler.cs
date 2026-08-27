using FeatureFlag.API.Features.WeatherForecast.DTOs;
using MediatR;

namespace FeatureFlag.API.Features.WeatherForecast.Queries;

public class GetWeatherByLongLatQueryHandler
    : IRequestHandler<GetWeatherByLongLatQuery, WeatherForecastDto>
{
    public Task<WeatherForecastDto> Handle(GetWeatherByLongLatQuery request, CancellationToken cancellationToken)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecast = new WeatherForecastDto
        (
            DateOnly.FromDateTime(DateTime.Now),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );

        return Task.FromResult(forecast);
    }
}