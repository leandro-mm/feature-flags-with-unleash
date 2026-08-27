namespace FeatureFlag.API.Features.WeatherForecast.DTOs;

public record WeatherForecastRequestDto(double Longitude, double Latitude, string UserName);
