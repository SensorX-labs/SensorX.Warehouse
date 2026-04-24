using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CreateStockIn;
using SensorX.Warehouse.WebApi.Extensions;

namespace SensorX.Warehouse.WebApi.API
{
    public static class StockInApi
    {
        public static RouteGroupBuilder MapStockInApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/stockIn").WithTags("StockIn");

            api.MapPost("/createStockIn", CreateStockIn).WithOpenApi();
            return api;
        }

        private static async Task<IResult> CreateStockIn(
            [FromBody] CreateStockInCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }
    }
}
