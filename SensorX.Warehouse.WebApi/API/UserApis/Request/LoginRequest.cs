namespace SensorX.Warehouse.WebApi.API.UserApis.Request
{
    public readonly record struct LoginRequest(
        string Email,
        string Password,
        string Device,
        string Browser,
        string IpAddress
    );
}

