using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CreateStockIn;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.WebApi.Extensions;
using SensorX.Warehouse.Application.Queries.StockIns;
using SensorX.Warehouse.Application.Common.Pagination;

namespace SensorX.Warehouse.WebApi.API
{
    public static class StockInApi
    {
        public static RouteGroupBuilder MapStockInApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("stockIn").WithTags("StockIn");

            api.MapPost("/createStockIn", CreateStockIn).WithOpenApi();
            api.MapGet("/list", GetStockIns).WithOpenApi();
            api.MapGet("/{id:guid}", GetStockInById).WithOpenApi();
            api.MapGet("/detail/{id:guid}", GetStockInById).WithOpenApi();
            return api;
        }

        private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreateStockIn(
            [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
            [FromBody] CreateStockInCommand command,
            [FromServices] IMediator mediator
        )
        {
            if (!warehouseId.HasValue || warehouseId == Guid.Empty)
                return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

            command.WarehouseId = warehouseId.Value;
            Result<Guid> result = await mediator.Send(command);
            return result ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
        }

        private static async Task<Results<Ok<StockInCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetStockIns(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
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

        var query = new GetPageListStockInsQuery
        {
            WarehouseId = warehouseId.Value,
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

        private static async Task<Results<Ok<StockInDetailDto>, BadRequest<string>, ProblemHttpResult>> GetStockInById(
            [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
            [FromRoute] Guid id,
            [FromServices] IMediator mediator
        )
        {
            if (!warehouseId.HasValue || warehouseId == Guid.Empty)
                return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

            var result = await mediator.Send(new GetStockInByIdQuery { Id = id, WarehouseId = warehouseId.Value });
            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : TypedResults.BadRequest(result.Error);
        }
}
}
