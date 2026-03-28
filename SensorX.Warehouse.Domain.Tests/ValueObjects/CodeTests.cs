using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.ValueObjects;

public class CodeTests
{
    private const string ValidCode = "TEST-260328-123456789";

    [Fact]
    public void From_ShouldCreateCode_WhenValueIsValid()
    {
        // Act
        var code = Code.From(ValidCode);

        // Assert
        Assert.Equal(ValidCode, code.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void From_ShouldThrowDomainException_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        var exception = Assert.Throws<DomainException>(() => Code.From(invalidValue!));
#pragma warning restore CS8604 // Possible null reference argument.
        Assert.Equal("Code cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData("TEST-123")] // Wrong format
    [InlineData("test-260328-123456789")] // Lowercase prefix
    [InlineData("TEST-260328123456789")] // Missing hyphen
    [InlineData("TEST-26032-123456789")] // Date part too short
    [InlineData("TEST-260328-12345678")] // Timestamp part too short
    [InlineData("TEST-260328-1234567890")] // Timestamp part too long
    public void From_ShouldThrowDomainException_WhenFormatIsInvalid(string invalidFormat)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => Code.From(invalidFormat));
        Assert.Equal("Code format is invalid.", exception.Message);
    }

    [Fact]
    public void Create_ShouldGenerateCodeWithCorrectPrefix()
    {
        // Arrange
        var prefix = "PN";

        // Act
        var code = Code.Create(prefix);

        // Assert
        Assert.StartsWith("PN-", code.Value);
        // Prefix(2) + Hyphen(1) + Date(6) + Hyphen(1) + Time(9) = 19
        Assert.Equal(19, code.Value.Length);
        
        // Verify it matches the inner regex
        var again = Code.From(code.Value);
        Assert.Equal(code.Value, again.Value);
    }

    [Fact]
    public void Create_ShouldNormalizePrefixToUpperCase()
    {
        // Arrange
        var prefix = "abc";

        // Act
        var code = Code.Create(prefix);

        // Assert
        Assert.StartsWith("ABC-", code.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrowDomainException_WhenPrefixIsNullOrWhiteSpace(string? invalidPrefix)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        var exception = Assert.Throws<DomainException>(() => Code.Create(invalidPrefix!));
#pragma warning restore CS8604 // Possible null reference argument.
        Assert.Equal("Prefix cannot be empty.", exception.Message);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var code = Code.From(ValidCode);

        // Act
        string result = code;

        // Assert
        Assert.Equal(ValidCode, result);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnEmptyString_WhenCodeIsNull()
    {
        // Arrange
        Code? code = null;

        // Act
        string result = code!;

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var code = Code.From(ValidCode);

        // Act
        var result = code.ToString();

        // Assert
        Assert.Equal(ValidCode, result);
    }

    [Fact]
    public void Equality_ShouldWorkBasedOnValue()
    {
        // Arrange
        var code1 = Code.From(ValidCode);
        var code2 = Code.From(ValidCode);
        var code3 = Code.Create("OTHER");

        // Assert
        Assert.Equal(code1, code2);
        Assert.NotEqual(code1, code3);
        Assert.True(code1 == code2);
        Assert.False(code1 == code3);
    }
}
