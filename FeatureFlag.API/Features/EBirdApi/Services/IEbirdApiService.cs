using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlag.API.Features.EBirdApi.DTOs;

namespace FeatureFlag.API.Features.EBirdApi.Services;

public interface IEBirdApiService
{
    Task<List<EBirdApiResponse?>?> GetBirdDataByRegionAsync(string regionCode, CancellationToken cancellationToken);
}