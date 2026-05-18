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
        api.MapGet("/{id:guid}", GetStockOutById).WithOpenApi();
        api.MapGet("/detail/{id:guid}", GetStockOutById).WithOpenApi();
        
        api.MapPost("/approve", ApproveStockOut).WithOpenApi();
        api.MapPost("/reject", RejectStockOut).WithOpenApi();

        return api;
    }

    private static async Task<Results<Ok<StockOutDetailDto>, BadRequest<string>, ProblemHttpResult>> GetStockOutById(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
        [FromRoute] Guid id,
        [FromServices] IMediator mediator
    )
    {
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

        var result = await mediator.Send(new GetStockOutByIdQuery { Id = id, WarehouseId = warehouseId.Value });
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreateStockOut(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
        [FromBody] CreateStockOutCommand command,
        [FromServices] IMediator mediator
    )
    {
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

        command.WarehouseId = warehouseId.Value;
        Result<Guid> result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<StockOutCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetStockOuts(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
        [FromQuery] bool? isAdjustmentOnly,
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
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

        var query = new GetPageListStockOutsQuery
        {
            WarehouseId = warehouseId.Value,
            IsAdjustmentOnly = isAdjustmentOnly ?? false,
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

    private static IResult ApproveStockOut()
    {
        return TypedResults.Ok();
    }

    private static IResult RejectStockOut()
    {
        return TypedResults.Ok();
    }
}
