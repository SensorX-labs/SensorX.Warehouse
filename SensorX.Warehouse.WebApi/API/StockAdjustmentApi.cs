using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.ApproveStockAdjustment;
using SensorX.Warehouse.Application.Commands.CreateStockAdjustment;
using SensorX.Warehouse.Application.Commands.RejectStockAdjustment;
using SensorX.Warehouse.Application.Queries.GetStockAdjustment;
using SensorX.Warehouse.Application.Queries.StockAdjustments;
using SensorX.Warehouse.Application.Common.Pagination;

namespace SensorX.Warehouse.WebApi.API;

public static class StockAdjustmentApi
{
    public static RouteGroupBuilder MapStockAdjustmentApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("stockAdjustment").WithTags("StockAdjustment");

        // Tạo mới phiếu điều chỉnh
        api.MapPost("/create", CreateStockAdjustment).WithOpenApi();

        // Duyệt phiếu điều chỉnh
        api.MapPost("/approve", ApproveStockAdjustment).WithOpenApi();

        // Từ chối phiếu điều chỉnh
        api.MapPost("/reject", RejectStockAdjustment).WithOpenApi();

        // Lấy chi tiết phiếu điều chỉnh
        api.MapGet("/detail/{id:guid}", GetStockAdjustmentDetail).WithOpenApi();
        api.MapGet("/list", GetStockAdjustments).WithOpenApi();

        return api;
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreateStockAdjustment(
        [FromBody] CreateStockAdjustmentCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> ApproveStockAdjustment(
        [FromBody] ApproveStockAdjustmentCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> RejectStockAdjustment(
        [FromBody] RejectStockAdjustmentCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<StockAdjustmentDto>, BadRequest<string>, ProblemHttpResult>> GetStockAdjustmentDetail(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(new GetStockAdjustmentQuery { Id = id });
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<StockAdjustmentCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetStockAdjustments(
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
        var query = new GetPageListStockAdjustmentsQuery
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
