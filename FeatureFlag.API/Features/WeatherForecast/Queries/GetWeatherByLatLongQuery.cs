
using FeatureFlag.API.Features.WeatherForecast.DTOs;
using MediatR;

namespace FeatureFlag.API.Features.WeatherForecast.Queries;

public record GetWeatherByLongLatQuery(double Longitude, double Latitude, string UserName)
    : IRequest<WeatherForecastResponseDto>;