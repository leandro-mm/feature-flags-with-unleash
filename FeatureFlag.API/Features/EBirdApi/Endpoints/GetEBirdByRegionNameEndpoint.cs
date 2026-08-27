using MediatR;
using FeatureFlag.API.Features.EBirdApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlag.API.Features.EBirdApi.Endpoints;

public static class GetEBirdByRegionNameEndpoint
{
    public static void MapGetEBirdByRegionNameEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/ebird/by-region-name", async (
            IMediator mediator,
            [FromBody] GetBirdApiByRegionNameQuery request) =>
        {
            var query = new GetBirdApiByRegionNameQuery(
                RegionCode: request.RegionCode,
                UserName: request.UserName);

            var forecast = await mediator.Send(query);
            return forecast;
        })
        .WithName("GetEBirdByRegionName")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status404NotFound);
    }
}