
using Scalar.AspNetCore;
using FeatureFlag.API.Features.WeatherForecast.DTOs;
using FeatureFlag.API.Features.WeatherForecast.Queries;
using FeatureFlag.API.Features.WeatherForecast.Endpoints;
using FeatureFlag.API.Features.EBirdApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        if (allowedOrigins != null && allowedOrigins.Any())
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

<<<<<<< Updated upstream
builder.Services.AddHttpClient();
=======
// Register a typed HttpClient for the eBird API
builder.Services.AddHttpClient<IEBirdApiService, EbirdApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EBird:BaseUrl"] ?? "https://api.ebird.org/v2/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

>>>>>>> Stashed changes

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGetWeatherByLongLatEndpoint();
app.MapGetEBirdByRegionNameEndpoint();

app.Run();

