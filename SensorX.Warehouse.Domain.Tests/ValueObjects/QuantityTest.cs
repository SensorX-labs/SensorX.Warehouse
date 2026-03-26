using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.ValueObjects;

public class QuantityTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ShouldThrowException_WhenValueIsNegative(int invalidValue)
    {
        Assert.Throws<DomainException>(() => new Quantity(invalidValue));
    }

    [Fact]
    public void Constructor_ShouldAllowZero()
    {
        var q = new Quantity(0);
        Assert.Equal(0, q.Value);
    }

    [Fact]
    public void Comparison_Operators_ShouldWork()
    {
        var q10 = new Quantity(10);
        var q5 = new Quantity(5);
        var q10_2 = new Quantity(10);

        Assert.True(q10 > q5);
        Assert.True(q5 < q10);
        Assert.True(q10 >= q10_2);
        Assert.True(q10 == q10_2);
    }

    [Fact]
    public void Arithmetic_Between_Quantities_ShouldWork()
    {
        var q1 = new Quantity(10);
        var q2 = new Quantity(5);

        Assert.Equal(15, (q1 + q2));
        Assert.Equal(5, (q1 - q2));
        Assert.True(q1 > q2);
    }

    [Fact]
    public void Arithmetic_With_Int_ShouldWork()
    {
        var q = new Quantity(10);

        // Test cộng với int
        var resultAdd = q + 5;
        Assert.Equal(15, resultAdd);

        // Test nhân với int
        var resultMul = q * 2;
        Assert.Equal(20, resultMul);

        // Test so sánh trực tiếp với int
        Assert.True(q > 8);
        Assert.False(q < 5);
    }

    [Fact]
    public void Operation_Resulting_In_Negative_ShouldThrow()
    {
        var q = new Quantity(10);
        Assert.Throws<DomainException>(() => q - 15);
    }
}