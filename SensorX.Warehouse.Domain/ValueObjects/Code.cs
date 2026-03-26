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

    public static Code Create(string prefix, int number)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new DomainException("Prefix cannot be empty.");

        if (number < 0)
            throw new DomainException("Sequence number cannot be negative.");

        var now = DateTime.UtcNow;
        var code = $"{prefix.ToUpper()}-{now:yyMMdd}-{number:D5}";
        return new Code(code);
    }

    public static implicit operator string(Code code) => code?.Value ?? string.Empty;

    public override string ToString() => Value;
}