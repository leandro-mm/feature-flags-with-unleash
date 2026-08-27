namespace FeatureFlag.API.Features.WeatherForecast.DTOs;

public record WeatherForecastResponseDto(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
