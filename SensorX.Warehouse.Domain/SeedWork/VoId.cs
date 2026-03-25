namespace SensorX.Warehouse.Domain.SeedWork;

public record VoId(Guid Value)
{
    public static implicit operator Guid(VoId voId) => voId.Value;
    public override string ToString() => Value.ToString();
}