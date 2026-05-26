using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Metadata;

/// <summary>
/// Tests for the <see cref="MetadataBag"/> class.
/// </summary>
public class MetadataBagTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyBag()
    {
        // Act
        var bag = new MetadataBag();

        // Assert
        Assert.Equal(0, bag.Count);
        Assert.True(bag.IsEmpty);
    }

    [Fact]
    public void Constructor_WithDictionary_CopiesItems()
    {
        // Arrange
        var items = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        var bag = new MetadataBag(items);

        // Assert
        Assert.Equal(2, bag.Count);
        Assert.False(bag.IsEmpty);
        Assert.Equal("value1", bag["key1"]);
        Assert.Equal("value2", bag["key2"]);
    }

    [Fact]
    public void Constructor_WithDictionary_UsesCaseInsensitiveComparison()
    {
        // Arrange
        var items = new Dictionary<string, string>
        {
            ["ContentType"] = "application/json"
        };

        // Act
        var bag = new MetadataBag(items);

        // Assert
        Assert.Equal("application/json", bag["contenttype"]);
        Assert.Equal("application/json", bag["CONTENTTYPE"]);
        Assert.Equal("application/json", bag["ContentType"]);
    }

    [Fact]
    public void Indexer_Get_ReturnsValue()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["testKey"] = "testValue";

        // Act
        var result = bag["testKey"];

        // Assert
        Assert.Equal("testValue", result);
    }

    [Fact]
    public void Indexer_Get_IsCaseInsensitive()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["TestKey"] = "value";

        // Act & Assert
        Assert.Equal("value", bag["testkey"]);
        Assert.Equal("value", bag["TESTKEY"]);
        Assert.Equal("value", bag["TestKey"]);
    }

    [Fact]
    public void Indexer_Get_WhenKeyDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => bag["nonexistent"]);
    }

    [Fact]
    public void Indexer_Set_AddsOrUpdatesValue()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act
        bag["key1"] = "value1";
        bag["key1"] = "value2";

        // Assert
        Assert.Equal("value2", bag["key1"]);
        Assert.Equal(1, bag.Count);
    }

    [Fact]
    public void Set_AddsNewValue()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act
        bag.Set("author", "John Doe");

        // Assert
        Assert.Equal("John Doe", bag["author"]);
        Assert.Equal(1, bag.Count);
    }

    [Fact]
    public void Set_UpdatesExistingValue()
    {
        // Arrange
        var bag = new MetadataBag();
        bag.Set("version", "1.0");

        // Act
        bag.Set("version", "2.0");

        // Assert
        Assert.Equal("2.0", bag["version"]);
        Assert.Equal(1, bag.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Set_WhenKeyIsNullOrWhiteSpace_ThrowsArgumentException(string? key)
    {
        // Arrange
        var bag = new MetadataBag();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => bag.Set(key!, "value"));
    }

    [Fact]
    public void TryGet_WhenKeyExists_ReturnsTrueAndValue()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["status"] = "active";

        // Act
        var found = bag.TryGet("status", out var value);

        // Assert
        Assert.True(found);
        Assert.Equal("active", value);
    }

    [Fact]
    public void TryGet_WhenKeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act
        var found = bag.TryGet("missing", out var value);

        // Assert
        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["UserId"] = "12345";

        // Act
        var found = bag.TryGet("userid", out var value);

        // Assert
        Assert.True(found);
        Assert.Equal("12345", value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGet_WhenKeyIsNullOrWhiteSpace_ThrowsArgumentException(string? key)
    {
        // Arrange
        var bag = new MetadataBag();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => bag.TryGet(key!, out _));
    }

    [Fact]
    public void Remove_WhenKeyExists_RemovesAndReturnsTrue()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["temp"] = "data";

        // Act
        var removed = bag.Remove("temp");

        // Assert
        Assert.True(removed);
        Assert.Equal(0, bag.Count);
        Assert.False(bag.TryGet("temp", out _));
    }

    [Fact]
    public void Remove_WhenKeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act
        var removed = bag.Remove("nonexistent");

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void Remove_IsCaseInsensitive()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["RemoveMe"] = "value";

        // Act
        var removed = bag.Remove("removeme");

        // Assert
        Assert.True(removed);
        Assert.Equal(0, bag.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Remove_WhenKeyIsNullOrWhiteSpace_ThrowsArgumentException(string? key)
    {
        // Arrange
        var bag = new MetadataBag();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => bag.Remove(key!));
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["key1"] = "value1";
        bag["key2"] = "value2";
        bag["key3"] = "value3";

        // Act
        bag.Clear();

        // Assert
        Assert.Equal(0, bag.Count);
        Assert.True(bag.IsEmpty);
    }

    [Fact]
    public void Items_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var bag = new MetadataBag();
        bag["platform"] = "windows";
        bag["arch"] = "x64";

        // Act
        var items = bag.Items;

        // Assert
        Assert.Equal(2, items.Count);
        Assert.True(items.ContainsKey("platform"));
        Assert.True(items.ContainsKey("arch"));
        Assert.Equal("windows", items["platform"]);
        Assert.Equal("x64", items["arch"]);
    }

    [Fact]
    public void Count_ReflectsNumberOfItems()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act & Assert
        Assert.Equal(0, bag.Count);

        bag["item1"] = "value1";
        Assert.Equal(1, bag.Count);

        bag["item2"] = "value2";
        Assert.Equal(2, bag.Count);

        bag.Remove("item1");
        Assert.Equal(1, bag.Count);

        bag.Clear();
        Assert.Equal(0, bag.Count);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueWhenNoItems()
    {
        // Arrange
        var bag = new MetadataBag();

        // Assert
        Assert.True(bag.IsEmpty);

        // Act
        bag["key"] = "value";

        // Assert
        Assert.False(bag.IsEmpty);
    }

    [Fact]
    public void ComplexScenario_MultipleOperations()
    {
        // Arrange
        var bag = new MetadataBag();

        // Act - Add multiple items
        bag.Set("environment", "production");
        bag.Set("region", "eu-west-1");
        bag["owner"] = "team-a";

        // Verify initial state
        Assert.Equal(3, bag.Count);

        // Update existing value
        bag.Set("environment", "staging");
        Assert.Equal("staging", bag["environment"]);
        Assert.Equal(3, bag.Count);

        // Try to get existing and non-existing
        Assert.True(bag.TryGet("region", out var region));
        Assert.Equal("eu-west-1", region);
        Assert.False(bag.TryGet("missing", out _));

        // Remove one item
        Assert.True(bag.Remove("owner"));
        Assert.Equal(2, bag.Count);

        // Access via different casing
        Assert.Equal("staging", bag["ENVIRONMENT"]);
        Assert.Equal("eu-west-1", bag["Region"]);

        // Clear all
        bag.Clear();
        Assert.True(bag.IsEmpty);
    }
}
