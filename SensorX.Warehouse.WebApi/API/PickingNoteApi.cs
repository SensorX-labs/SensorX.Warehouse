using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Commands.CancelPickingNote;
using SensorX.Warehouse.Application.Commands.CompletePickingNote;
using SensorX.Warehouse.Application.Commands.CreatePickingNote;
using SensorX.Warehouse.Application.Commands.StartPickingNote;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Application.Queries.GetPickingNote;
using SensorX.Warehouse.Application.Queries.PickingNotes;
using SensorX.Warehouse.Application.Common.Pagination;

namespace SensorX.Warehouse.WebApi.API;

public static class PickingNoteApi
{
    public static RouteGroupBuilder MapPickingNoteApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("pickingNote").WithTags("PickingNote");

        // Tạo phiếu soạn hàng (từ SalesOrder hoặc TransferOrder)
        api.MapPost("/createPickingNote", CreatePickingNote).WithOpenApi();

        // Bắt đầu soạn hàng → allocate inventory
        api.MapPost("/startPicking", StartPicking).WithOpenApi();

        // Hoàn thành soạn hàng → status = Completed, chờ xuất kho
        api.MapPost("/completePicking", CompletePicking).WithOpenApi();

        // Hủy soạn hàng → release allocation
        api.MapPost("/cancelPicking", CancelPicking).WithOpenApi();

        // Lấy thông tin phiếu soạn hàng
        api.MapGet("/getPickingNote/{id:guid}", GetPickingNote).WithOpenApi();
        api.MapGet("/{id:guid}", GetPickingNote).WithOpenApi();
        api.MapGet("/getPickingNotes", GetPickingNotes).WithOpenApi();
        api.MapGet("/list", GetPickingNotes).WithOpenApi();

        return api;
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CreatePickingNote(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
        [FromBody] CreatePickingNoteCommand command,
        [FromServices] IMediator mediator
    )
    {
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

        command.WarehouseId = warehouseId.Value;
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> StartPicking(
        [FromBody] StartPickingNoteCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CompletePicking(
        [FromBody] CompletePickingNoteCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>, ProblemHttpResult>> CancelPicking(
        [FromBody] CancelPickingNoteCommand command,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<PickingNoteDto>, BadRequest<string>, ProblemHttpResult>> GetPickingNote(
        [FromHeader(Name = "X-Warehouse-Id")] Guid? warehouseId,
        [FromRoute] Guid id,
        [FromServices] IMediator mediator
    )
    {
        if (!warehouseId.HasValue || warehouseId == Guid.Empty)
            return TypedResults.BadRequest("Vui lòng chọn kho bãi (X-Warehouse-Id header is missing)");

        var result = await mediator.Send(new GetPickingNoteQuery { PickingNoteId = id, WarehouseId = warehouseId.Value });
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<Ok<PickingNoteCursorPagedResult>, BadRequest<string>, ProblemHttpResult>> GetPickingNotes(
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

        var query = new GetPageListPickingNotesQuery
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
}
