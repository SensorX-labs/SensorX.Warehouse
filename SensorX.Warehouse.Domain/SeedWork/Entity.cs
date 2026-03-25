namespace SensorX.Warehouse.Domain.SeedWork
{
    public abstract class Entity<EntityID>(EntityID id) : IEquatable<Entity<EntityID>> where EntityID : VoId
    {
        private int? _requestedHashCode;
        public EntityID Id { get; protected set; } = id;

        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }
        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public override bool Equals(object? obj) => Equals(obj as Entity<EntityID>);
        public bool Equals(Entity<EntityID>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id && this.GetType() == other.GetType();
        }

        public override int GetHashCode()
        {
            _requestedHashCode ??= (Id.GetHashCode() ^ 31);
            return _requestedHashCode.Value;
        }

        public static bool operator ==(Entity<EntityID> left, Entity<EntityID> right) => Equals(left, right);
        public static bool operator !=(Entity<EntityID> left, Entity<EntityID> right) => !(left == right);
    }
}

