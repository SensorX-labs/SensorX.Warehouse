using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record Code
{
    public string Value { get; init; }

    private Code(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Code cannot be empty.");
        Value = value;
    }

    public static Code From(string value) => new(value);

    public static Code Create(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new DomainException("Prefix cannot be empty.");

        var now = DateTime.UtcNow;
        var code = $"{prefix.ToUpper()}-{now:yyMMdd}-{now:HHmmssfff}";
        return new Code(code);
    }

    public static implicit operator string(Code code) => code?.Value ?? string.Empty;

    public override string ToString() => Value;
}