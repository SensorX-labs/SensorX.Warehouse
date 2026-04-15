using SensorX.Warehouse.WebApi.API;

namespace SensorX.Warehouse.WebApi;

public static class Api
{
    public static RouteGroupBuilder MapApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api");

        api.MapStockInApi();

        return api;
    }
}