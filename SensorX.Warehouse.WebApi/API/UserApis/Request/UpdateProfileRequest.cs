namespace SensorX.Warehouse.WebApi.API.UserApis.Request
{
    public readonly record struct UpdateProfileRequest(
        string Name,
        string? UrlAvatar
    );
}

