namespace SensorX.Warehouse.WebApi.API.UserApis.Request
{
    public readonly record struct VerifyTokenRequest(
        string Token,
        int Type
    );
}

