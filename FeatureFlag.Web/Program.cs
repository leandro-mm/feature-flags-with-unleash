using FeatureFlag.Web.Components;
using FeatureFlag.Web.Services;
using FeatureFlag.Web.Settings;
using Microsoft.Extensions.Options;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient("WebApi", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
})
.AddStandardResilienceHandler();
// .AddResilienceHandler("default", configure =>
//     {
//         configure.AddTimeout(TimeSpan.FromSeconds(2)); //how long you can wait for the response

//         configure.AddRetry(new HttpRetryStrategyOptions
//         {
//             MaxRetryAttempts = 3, //Number of retries
//             BackoffType = DelayBackoffType.Linear, //delay between retries
//             Delay = TimeSpan.FromMilliseconds(20), //delay between retries
//             UseJitter = true, //random delay to each retry
//         });

//         configure
//           .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions()); //Circuit-Breaker pattern (open, half-open, close);

//     });

builder.Services.AddScoped<ApiService>();

builder.Services.AddSweetAlert2();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
