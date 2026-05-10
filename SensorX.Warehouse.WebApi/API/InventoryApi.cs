using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Application.Queries.InventoryItems;
using SensorX.Warehouse.Application.Common.Pagination;

namespace SensorX.Warehouse.WebApi.API;

public static class InventoryApi
{
    public static RouteGroupBuilder MapInventoryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("inventory").WithTags("Inventory");

        // Lấy danh sách tồn kho
        api.MapGet("/list", GetInventoryItems).WithOpenApi();

        return api;
    }

    private static async Task<Results<Ok<InventoryItemCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetInventoryItems(
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
        var query = new GetPageListInventoryItemsQuery
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
