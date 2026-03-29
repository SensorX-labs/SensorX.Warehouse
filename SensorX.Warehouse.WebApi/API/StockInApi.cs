using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CreateStockIn;
using SensorX.Warehouse.Application.Common.ResponseClient;


namespace WebApi.API
{
    public static class StockInApi
    {
        public static RouteGroupBuilder MapStockInApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/stockIn").WithTags("StockIn");

            api.MapPut("/createStockIn", CreateStockIn).WithOpenApi();
            return api;
        }

        private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateStockIn(
            [FromBody] CreateStockInCommand request,
            [FromServices] IMediator mediator
        )
        {
            Result<Guid> result = await mediator.Send(request);
            return result ? TypedResults.Ok() : TypedResults.BadRequest(result.Error);
        }
    }
}
