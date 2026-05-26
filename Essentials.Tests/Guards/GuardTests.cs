using Afrowave.Toolbox.Essentials.Guards;

namespace Afrowave.Toolbox.Essentials.Tests.Guards;

/// <summary>
/// Tests for the <see cref="Guard"/> class.
/// </summary>
public class GuardTests
{
    [Fact]
    public void NotNull_WhenValueIsNotNull_ReturnsValue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Guard.NotNull(value, nameof(value));

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void NotNull_WhenValueIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.NotNull(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
    }

    [Fact]
    public void NotNullOrWhiteSpace_WhenValueIsValid_ReturnsValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Guard.NotNullOrWhiteSpace(value, nameof(value));

        // Assert
        Assert.Equal("test value", result);
    }

    [Fact]
    public void NotNullOrWhiteSpace_WhenValueIsNull_ThrowsArgumentException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains("cannot be null, empty, or whitespace", exception.Message);
    }

    [Fact]
    public void NotNullOrWhiteSpace_WhenValueIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var value = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
    }

    [Fact]
    public void NotNullOrWhiteSpace_WhenValueIsWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var value = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void NotNegative_Int_WhenValueIsZeroOrPositive_ReturnsValue(int value)
    {
        // Act
        var result = Guard.NotNegative(value, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void NotNegative_Int_WhenValueIsNegative_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.NotNegative(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(long.MaxValue)]
    public void NotNegative_Long_WhenValueIsZeroOrPositive_ReturnsValue(long value)
    {
        // Act
        var result = Guard.NotNegative(value, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(-100L)]
    [InlineData(long.MinValue)]
    public void NotNegative_Long_WhenValueIsNegative_ThrowsArgumentOutOfRangeException(long value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.NotNegative(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Positive_Int_WhenValueIsPositive_ReturnsValue(int value)
    {
        // Act
        var result = Guard.Positive(value, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Positive_Int_WhenValueIsZeroOrNegative_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains("must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(long.MaxValue)]
    public void Positive_Long_WhenValueIsPositive_ReturnsValue(long value)
    {
        // Act
        var result = Guard.Positive(value, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    [InlineData(long.MinValue)]
    public void Positive_Long_WhenValueIsZeroOrNegative_ThrowsArgumentOutOfRangeException(long value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(value, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains("must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(0, 0, 10)]
    [InlineData(5, 0, 10)]
    [InlineData(10, 0, 10)]
    [InlineData(100, 50, 150)]
    public void InRange_WhenValueIsWithinRange_ReturnsValue(int value, int minimum, int maximum)
    {
        // Act
        var result = Guard.InRange(value, minimum, maximum, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(-1, 0, 10)]
    [InlineData(11, 0, 10)]
    [InlineData(100, 0, 50)]
    public void InRange_WhenValueIsOutsideRange_ThrowsArgumentOutOfRangeException(int value, int minimum, int maximum)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(value, minimum, maximum, nameof(value)));
        Assert.Equal(nameof(value), exception.ParamName);
        Assert.Contains($"must be between {minimum} and {maximum}", exception.Message);
    }
}
