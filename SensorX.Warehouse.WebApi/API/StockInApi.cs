using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CreateStockIn;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.WebApi.API
{
    public static class StockInApi
    {
        public static RouteGroupBuilder MapStockInApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/stockIn").WithTags("StockIn");

            api.MapPut("/createStockIn", CreateStockIn).WithOpenApi();
            return api;
        }

        private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreateStockIn(
            [FromBody] CreateStockInCommand command,
            [FromServices] IMediator mediator
        )
        {
            Result<Guid> result = await mediator.Send(command);
            return result ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
        }
    }
}