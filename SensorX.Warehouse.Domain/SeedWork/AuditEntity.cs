using SensorX.Warehouse.Domain.Common.Extensions;

namespace SensorX.Warehouse.Domain.SeedWork
{
    public abstract class AuditEntity : Entity<VoId>, ICreationTrackable, IUpdateTrackable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        protected AuditEntity(VoId id) : base(id)
        {
            this.MarkCreated();
        }
    }
}

