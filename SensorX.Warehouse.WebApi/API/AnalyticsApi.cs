using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.Application.Queries.Analytics.GetWarehouseDashboardStats;
using SensorX.Warehouse.WebApi.Extensions;

namespace SensorX.Warehouse.WebApi.API;

public static class AnalyticsApi
{
    public static RouteGroupBuilder MapAnalyticsApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("analytics").WithTags("Analytics");

        api.MapGet("/dashboard-stats", GetWarehouseDashboardStats)
            .WithOpenApi()
            .WithSummary("Get dashboard statistics")
            .WithDescription("Lấy thống kê dashboard cho kho vận");

        return api;
    }

    private static async Task<IResult> GetWarehouseDashboardStats(
        [FromQuery] string timeRange,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(new GetWarehouseDashboardStatsQuery(timeRange ?? "month"));
        return result.ToResult();
    }
}
