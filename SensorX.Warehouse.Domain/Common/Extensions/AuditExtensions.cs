using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.Common.Extensions;
public static class AuditExtensions 
{
    public static void MarkCreated(this ICreationTrackable entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
    }

    public static void MarkUpdated(this IUpdateTrackable entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }
}

