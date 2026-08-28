using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using FeatureFlag.API.Features.EBirdApi.Services;
using Xunit;
using FeatureFlag.API.Features.EBirdApi.DTOs;
using System.Text.Json;
using System.Collections.Generic;

namespace FeatureFlag.Tests
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    public class EBirdApiServiceTests
    {
        [Fact]
        public async Task GetBirdDataByRegionAsync_Returns_Observations()
        {
            var sample = new[]
            {
                new EBirdApiResponse("spng001", "Sample Bird", "Specius sampleus", "L1", "Loc1", "2026-08-28", 1, 10.0, 20.0, true, false, false, "s1")
            };

            var json = JsonSerializer.Serialize(sample);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handler = new FakeHttpMessageHandler(response);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new System.Uri("https://api.ebird.org/v2/")
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "EBird:ApiKey", "test" } })
                .Build();

            var logger = NullLogger<EbirdApiService>.Instance;

            var service = new EbirdApiService(httpClient, logger, config);

            var result = await service.GetBirdDataByRegionAsync("BR-SP", CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("spng001", result[0]?.SpeciesCode);
        }
    }
}
