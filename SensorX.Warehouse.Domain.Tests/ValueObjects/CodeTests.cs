using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.ValueObjects;

public class CodeTests
{
    /// <summary>
    /// Kiểm tra tạo Code từ một chuỗi hợp lệ.
    /// </summary>
    [Fact]
    public void From_ShouldCreateCode_WhenValueIsValid()
    {
        // Arrange
        var value = "TEST-001";

        // Act
        var code = Code.From(value);

        // Assert
        Assert.Equal(value, code.Value);
    }

    /// <summary>
    /// Kiểm tra việc ném lỗi khi tạo Code từ chuỗi rỗng hoặc null.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void From_ShouldThrowDomainException_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<DomainException>(() => Code.From(invalidValue!));
#pragma warning restore CS8604 // Possible null reference argument.
    }

    /// <summary>
    /// Kiểm tra phương thức Create tạo mã tự động theo prefix.
    /// </summary>
    [Fact]
    public void Create_ShouldGenerateCodeWithCorrectPrefix()
    {
        // Arrange
        var prefix = "PN";

        // Act
        var code = Code.Create(prefix);

        // Assert
        Assert.StartsWith("PN-", code.Value);
        // Kiểm tra định dạng: PREFIX-YYMMDD-HHmmssfff (độ dài tương ứng)
        // PREFIX(2) + separator(1) + YYMMDD(6) + separator(1) + HHmmssfff(9) = 19 ký tự
        Assert.Equal(19, code.Value.Length);
    }

    /// <summary>
    /// Kiểm tra việc ném lỗi khi dùng prefix rỗng để tạo mã tự động.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrowDomainException_WhenPrefixIsNullOrWhiteSpace(string? invalidPrefix)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<DomainException>(() => Code.Create(invalidPrefix!));
#pragma warning restore CS8604 // Possible null reference argument.
    }

    /// <summary>
    /// Kiểm tra chuyển đổi ngầm định từ Code sang string.
    /// </summary>
    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var value = "ABC-123";
        var code = Code.From(value);

        // Act
        string result = code;

        // Assert
        Assert.Equal(value, result);
    }

    /// <summary>
    /// Kiểm tra chuyển đổi ngầm định trả về chuỗi rỗng khi đối tượng Code là null.
    /// </summary>
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

    /// <summary>
    /// Kiểm tra phương thức ToString trả về đúng giá trị mã.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var value = "XYZ-789";
        var code = Code.From(value);

        // Act
        var result = code.ToString();

        // Assert
        Assert.Equal(value, result);
    }
}
