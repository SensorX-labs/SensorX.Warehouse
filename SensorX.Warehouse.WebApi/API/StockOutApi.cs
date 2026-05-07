using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CreateStockOut;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Application.Queries.StockOuts;
using SensorX.Warehouse.Application.Common.Pagination;

namespace SensorX.Warehouse.WebApi.API;

public static class StockOutApi
{
    public static RouteGroupBuilder MapStockOutApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("stockOut").WithTags("StockOut");

        // Tạo phiếu xuất kho
        api.MapPost("/createStockOut", CreateStockOut).WithOpenApi();

        api.MapGet("/list", GetStockOuts).WithOpenApi();

        return api;
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreateStockOut(
        [FromBody] CreateStockOutCommand command,
        [FromServices] IMediator mediator
    )
    {
        Result<Guid> result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<StockOutCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetStockOuts(
        [FromQuery] string? searchTerm,
        [FromQuery] int? pageSize,
        [FromQuery] bool? isPrevious,
        [FromQuery] DateTimeOffset? firstCreatedAt,
        [FromQuery] Guid? firstId,
        [FromQuery] DateTimeOffset? lastCreatedAt,
        [FromQuery] Guid? lastId,
        [FromServices] IMediator mediator
    )
    {
        var query = new GetPageListStockOutsQuery
        {
            SearchTerm = searchTerm,
            PageSize = pageSize ?? CursorPagedQuery.DefaultPageSize,
            IsPrevious = isPrevious ?? false,
            FirstCreatedAt = firstCreatedAt,
            FirstId = firstId,
            LastCreatedAt = lastCreatedAt,
            LastId = lastId
        };

        var result = await mediator.Send(query);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }
}
