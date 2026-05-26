using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.ValueObjects;

/// <summary>
/// Tests for the <see cref="ProviderName"/> struct.
/// </summary>
public class ProviderNameTests
{
    [Theory]
    [InlineData("ollama-local")]
    [InlineData("libretranslate-main")]
    [InlineData("sqlite-default")]
    [InlineData("azure-openai")]
    public void Constructor_WhenValueIsValid_CreatesInstance(string value)
    {
        // Act
        var providerName = new ProviderName(value);

        // Assert
        Assert.Equal(value, providerName.Value);
    }

    [Fact]
    public void Constructor_WhenValueHasWhitespace_TrimsValue()
    {
        // Arrange
        var value = "  ollama-local  ";

        // Act
        var providerName = new ProviderName(value);

        // Assert
        Assert.Equal("ollama-local", providerName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenValueIsNullOrWhiteSpace_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ProviderName(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var providerName = new ProviderName("azure-storage");

        // Act
        var result = providerName.ToString();

        // Assert
        Assert.Equal("azure-storage", result);
    }

    [Fact]
    public void From_CreatesInstance()
    {
        // Act
        var providerName = ProviderName.From("redis-cache");

        // Assert
        Assert.Equal("redis-cache", providerName.Value);
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        // Arrange
        var providerName = new ProviderName("postgres-main");

        // Act
        string result = providerName;

        // Assert
        Assert.Equal("postgres-main", result);
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromString()
    {
        // Arrange
        var value = "mongodb-primary";

        // Act
        var providerName = (ProviderName)value;

        // Assert
        Assert.Equal("mongodb-primary", providerName.Value);
    }

    [Fact]
    public void Equality_WhenValuesAreEqual_ReturnsTrue()
    {
        // Arrange
        var providerName1 = new ProviderName("elasticsearch-cluster");
        var providerName2 = new ProviderName("elasticsearch-cluster");

        // Act & Assert
        Assert.Equal(providerName1, providerName2);
        Assert.True(providerName1 == providerName2);
    }

    [Fact]
    public void Equality_WhenValuesAreDifferent_ReturnsFalse()
    {
        // Arrange
        var providerName1 = new ProviderName("mysql-main");
        var providerName2 = new ProviderName("mysql-replica");

        // Act & Assert
        Assert.NotEqual(providerName1, providerName2);
        Assert.True(providerName1 != providerName2);
    }
}
