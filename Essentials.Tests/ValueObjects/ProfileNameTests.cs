using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.ValueObjects;

/// <summary>
/// Tests for the <see cref="ProfileName"/> struct.
/// </summary>
public class ProfileNameTests
{
    [Theory]
    [InlineData("default")]
    [InlineData("markdown-refine")]
    [InlineData("live-chat-fast")]
    [InlineData("production")]
    public void Constructor_WhenValueIsValid_CreatesInstance(string value)
    {
        // Act
        var profileName = new ProfileName(value);

        // Assert
        Assert.Equal(value, profileName.Value);
    }

    [Fact]
    public void Constructor_WhenValueHasWhitespace_TrimsValue()
    {
        // Arrange
        var value = "  development  ";

        // Act
        var profileName = new ProfileName(value);

        // Assert
        Assert.Equal("development", profileName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenValueIsNullOrWhiteSpace_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => new ProfileName(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var profileName = new ProfileName("staging");

        // Act
        var result = profileName.ToString();

        // Assert
        Assert.Equal("staging", result);
    }

    [Fact]
    public void From_CreatesInstance()
    {
        // Act
        var profileName = ProfileName.From("testing");

        // Assert
        Assert.Equal("testing", profileName.Value);
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        // Arrange
        var profileName = new ProfileName("integration");

        // Act
        string result = profileName;

        // Assert
        Assert.Equal("integration", result);
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromString()
    {
        // Arrange
        var value = "performance";

        // Act
        var profileName = (ProfileName)value;

        // Assert
        Assert.Equal("performance", profileName.Value);
    }

    [Fact]
    public void Equality_WhenValuesAreEqual_ReturnsTrue()
    {
        // Arrange
        var profileName1 = new ProfileName("production");
        var profileName2 = new ProfileName("production");

        // Act & Assert
        Assert.Equal(profileName1, profileName2);
        Assert.True(profileName1 == profileName2);
    }

    [Fact]
    public void Equality_WhenValuesAreDifferent_ReturnsFalse()
    {
        // Arrange
        var profileName1 = new ProfileName("development");
        var profileName2 = new ProfileName("production");

        // Act & Assert
        Assert.NotEqual(profileName1, profileName2);
        Assert.True(profileName1 != profileName2);
    }
}
