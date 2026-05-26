using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

/// <summary>
/// Test enum with flags for testing <see cref="EnumFlagExtensions"/>.
/// </summary>
[Flags]
public enum TestPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    Delete = 8,
    Admin = 16,
    All = Read | Write | Execute | Delete | Admin
}

/// <summary>
/// Tests for the <see cref="EnumFlagExtensions"/> class.
/// </summary>
public class EnumFlagExtensionsTests
{
    [Fact]
    public void HasAll_WhenAllFlagsArePresent_ReturnsTrue()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write | TestPermissions.Execute;

        // Act
        var result = permissions.HasAll(TestPermissions.Read | TestPermissions.Write);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAll_WhenSomeFlagsAreMissing_ReturnsFalse()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write;

        // Act
        var result = permissions.HasAll(TestPermissions.Read | TestPermissions.Execute);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAll_WhenCheckingAgainstNone_ReturnsTrue()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.HasAll(TestPermissions.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAll_WhenCheckingSameFlags_ReturnsTrue()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write;

        // Act
        var result = permissions.HasAll(TestPermissions.Read | TestPermissions.Write);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAny_WhenAtLeastOneFlagIsPresent_ReturnsTrue()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write;

        // Act
        var result = permissions.HasAny(TestPermissions.Write | TestPermissions.Execute);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAny_WhenNoFlagsArePresent_ReturnsFalse()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.HasAny(TestPermissions.Write | TestPermissions.Execute);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAny_WhenCheckingAgainstNone_ReturnsFalse()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.HasAny(TestPermissions.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void With_AddsSpecifiedFlags()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.With(TestPermissions.Write | TestPermissions.Execute);

        // Assert
        Assert.True(result.HasAll(TestPermissions.Read | TestPermissions.Write | TestPermissions.Execute));
    }

    [Fact]
    public void With_WhenFlagAlreadyPresent_KeepsFlag()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write;

        // Act
        var result = permissions.With(TestPermissions.Read);

        // Assert
        Assert.Equal(TestPermissions.Read | TestPermissions.Write, result);
    }

    [Fact]
    public void With_WhenAddingNone_DoesNotChangeValue()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.With(TestPermissions.None);

        // Assert
        Assert.Equal(TestPermissions.Read, result);
    }

    [Fact]
    public void Without_RemovesSpecifiedFlags()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write | TestPermissions.Execute;

        // Act
        var result = permissions.Without(TestPermissions.Write | TestPermissions.Execute);

        // Assert
        Assert.Equal(TestPermissions.Read, result);
        Assert.False(result.HasAny(TestPermissions.Write | TestPermissions.Execute));
    }

    [Fact]
    public void Without_WhenFlagNotPresent_DoesNotChangeValue()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act
        var result = permissions.Without(TestPermissions.Write);

        // Assert
        Assert.Equal(TestPermissions.Read, result);
    }

    [Fact]
    public void Without_WhenRemovingNone_DoesNotChangeValue()
    {
        // Arrange
        var permissions = TestPermissions.Read | TestPermissions.Write;

        // Act
        var result = permissions.Without(TestPermissions.None);

        // Assert
        Assert.Equal(TestPermissions.Read | TestPermissions.Write, result);
    }

    [Fact]
    public void ComplexScenario_CombiningMultipleOperations()
    {
        // Arrange
        var permissions = TestPermissions.Read;

        // Act - Add Write and Execute, then remove Read
        var result = permissions
            .With(TestPermissions.Write)
            .With(TestPermissions.Execute)
            .Without(TestPermissions.Read);

        // Assert
        Assert.True(result.HasAll(TestPermissions.Write | TestPermissions.Execute));
        Assert.False(result.HasAny(TestPermissions.Read));
        Assert.Equal(TestPermissions.Write | TestPermissions.Execute, result);
    }

    [Fact]
    public void All_ContainsAllIndividualFlags()
    {
        // Arrange
        var all = TestPermissions.All;

        // Act & Assert
        Assert.True(all.HasAll(TestPermissions.Read));
        Assert.True(all.HasAll(TestPermissions.Write));
        Assert.True(all.HasAll(TestPermissions.Execute));
        Assert.True(all.HasAll(TestPermissions.Delete));
        Assert.True(all.HasAll(TestPermissions.Admin));
        Assert.True(all.HasAll(TestPermissions.Read | TestPermissions.Write | TestPermissions.Execute | TestPermissions.Delete | TestPermissions.Admin));
    }
}
