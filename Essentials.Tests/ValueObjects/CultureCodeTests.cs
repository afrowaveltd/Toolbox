using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.ValueObjects;

/// <summary>
/// Tests for the <see cref="CultureCode"/> struct.
/// </summary>
public class CultureCodeTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("cs")]
    [InlineData("en-US")]
    [InlineData("cs-CZ")]
    [InlineData("de-DE")]
    public void Constructor_WhenValueIsValid_CreatesInstance(string value)
    {
        // Act
        var cultureCode = new CultureCode(value);

        // Assert
        Assert.Equal(value, cultureCode.Value);
    }

    [Fact]
    public void Constructor_WhenValueHasWhitespace_TrimsValue()
    {
        // Arrange
        var value = "  en-US  ";

        // Act
        var cultureCode = new CultureCode(value);

        // Assert
        Assert.Equal("en-US", cultureCode.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenValueIsNullOrWhiteSpace_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new CultureCode(value!));
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("cs", true)]
    [InlineData("de", true)]
    [InlineData("en-US", false)]
    [InlineData("cs-CZ", false)]
    public void IsNeutral_ReturnsExpectedValue(string value, bool expectedIsNeutral)
    {
        // Arrange
        var cultureCode = new CultureCode(value);

        // Act
        var isNeutral = cultureCode.IsNeutral;

        // Assert
        Assert.Equal(expectedIsNeutral, isNeutral);
    }

    [Theory]
    [InlineData("en", false)]
    [InlineData("cs", false)]
    [InlineData("en-US", true)]
    [InlineData("cs-CZ", true)]
    [InlineData("zh-Hans-CN", true)]
    public void IsSpecific_ReturnsExpectedValue(string value, bool expectedIsSpecific)
    {
        // Arrange
        var cultureCode = new CultureCode(value);

        // Act
        var isSpecific = cultureCode.IsSpecific;

        // Assert
        Assert.Equal(expectedIsSpecific, isSpecific);
    }

    [Theory]
    [InlineData("en", "en")]
    [InlineData("cs", "cs")]
    [InlineData("en-US", "en")]
    [InlineData("cs-CZ", "cs")]
    [InlineData("zh-Hans-CN", "zh")]
    public void GetNeutralPart_ReturnsExpectedValue(string value, string expectedNeutralPart)
    {
        // Arrange
        var cultureCode = new CultureCode(value);

        // Act
        var neutralPart = cultureCode.GetNeutralPart();

        // Assert
        Assert.Equal(expectedNeutralPart, neutralPart);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var cultureCode = new CultureCode("en-US");

        // Act
        var result = cultureCode.ToString();

        // Assert
        Assert.Equal("en-US", result);
    }

    [Fact]
    public void From_CreatesInstance()
    {
        // Act
        var cultureCode = CultureCode.From("en-GB");

        // Assert
        Assert.Equal("en-GB", cultureCode.Value);
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        // Arrange
        var cultureCode = new CultureCode("fr-FR");

        // Act
        string result = cultureCode;

        // Assert
        Assert.Equal("fr-FR", result);
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromString()
    {
        // Arrange
        var value = "es-ES";

        // Act
        var cultureCode = (CultureCode)value;

        // Assert
        Assert.Equal("es-ES", cultureCode.Value);
    }

    [Fact]
    public void Equality_WhenValuesAreEqual_ReturnsTrue()
    {
        // Arrange
        var cultureCode1 = new CultureCode("en-US");
        var cultureCode2 = new CultureCode("en-US");

        // Act & Assert
        Assert.Equal(cultureCode1, cultureCode2);
        Assert.True(cultureCode1 == cultureCode2);
    }

    [Fact]
    public void Equality_WhenValuesAreDifferent_ReturnsFalse()
    {
        // Arrange
        var cultureCode1 = new CultureCode("en-US");
        var cultureCode2 = new CultureCode("en-GB");

        // Act & Assert
        Assert.NotEqual(cultureCode1, cultureCode2);
        Assert.True(cultureCode1 != cultureCode2);
    }
}
