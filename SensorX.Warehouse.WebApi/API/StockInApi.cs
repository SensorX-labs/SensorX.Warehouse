using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


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
            var command = new UpdateProfileCommand(id, request.Name, request.UrlAvatar);
            Result result = await mediator.Send(command);
            return result ? TypedResults.Ok() : TypedResults.BadRequest(result.Error);
        }

}