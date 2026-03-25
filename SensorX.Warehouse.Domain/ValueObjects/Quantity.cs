using SensorX.Warehouse.Domain.Common.Exceptions;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("Quantity must be positive");
        }
        Value = value;
    }

    public static implicit operator int(Quantity quantity) => quantity.Value;
}