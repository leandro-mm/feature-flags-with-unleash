
using FeatureFlag.API.Features.EBirdApi.DTOs;
using MediatR;

namespace FeatureFlag.API.Features.EBirdApi.Queries;

public record GetBirdApiByRegionNameQuery(
    string RegionCode,
    string UserName
    ) : IRequest<EBirdApiResponse?>;