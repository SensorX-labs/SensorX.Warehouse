namespace SensorX.Warehouse.Domain.SeedWork
{
    public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>, IHasDomainEvents where TId : VoId, IEntityId<TId>
    {
        private int? _requestedHashCode;
        public TId Id { get; init; } = id;

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

        public override bool Equals(object? obj) => Equals(obj as Entity<TId>);
        public bool Equals(Entity<TId>? other)
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

        public static bool operator ==(Entity<TId> left, Entity<TId> right) => Equals(left, right);
        public static bool operator !=(Entity<TId> left, Entity<TId> right) => !(left == right);
    }
}

