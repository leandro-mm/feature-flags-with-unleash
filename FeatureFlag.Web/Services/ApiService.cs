namespace FeatureFlag.Web.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var client = _httpClientFactory.CreateClient("WebApi");
        var response = await client.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}