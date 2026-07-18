using System.Text.Json;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Metadata;

public sealed class MetadataBagJsonConverterTests
{
    [Fact]
    public void Deserialize_WithPlainStringObject_ReadsMetadata()
    {
        MetadataBag? metadata = JsonSerializer.Deserialize<MetadataBag>(
            """
            {
              "Environment": "Production",
              "Region": "EU"
            }
            """);

        Assert.NotNull(metadata);
        Assert.Equal(2, metadata.Count);
        Assert.Equal("Production", metadata["Environment"]);
        Assert.Equal("EU", metadata["Region"]);
    }

    [Fact]
    public void Deserialize_WithLegacyEmptyShape_ReturnsEmptyMetadata()
    {
        MetadataBag? metadata = JsonSerializer.Deserialize<MetadataBag>(
            """
            {
              "Count": 0,
              "IsEmpty": true,
              "Items": {}
            }
            """);

        Assert.NotNull(metadata);
        Assert.True(metadata.IsEmpty);
    }

    [Fact]
    public void Deserialize_WithLegacyItemsShape_ReadsNestedItems()
    {
        MetadataBag? metadata = JsonSerializer.Deserialize<MetadataBag>(
            """
            {
              "Count": 2,
              "IsEmpty": false,
              "Items": {
                "Environment": "Production",
                "Region": "EU"
              }
            }
            """);

        Assert.NotNull(metadata);
        Assert.Equal(2, metadata.Count);
        Assert.Equal("Production", metadata["Environment"]);
        Assert.Equal("EU", metadata["Region"]);
    }

    [Fact]
    public void Deserialize_WithNonStringPlainValue_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<MetadataBag>(
                """
                {
                  "Count": 1
                }
                """));
    }

    [Fact]
    public void Serialize_WithMetadataBag_WritesOnlyPlainStringObject()
    {
        MetadataBag metadata = new();
        metadata.Set("Region", "EU");
        metadata.Set("Environment", "Production");

        string json = JsonSerializer.Serialize(metadata);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("Production", root.GetProperty("Environment").GetString());
        Assert.Equal("EU", root.GetProperty("Region").GetString());
        Assert.False(root.TryGetProperty("Count", out _));
        Assert.False(root.TryGetProperty("IsEmpty", out _));
        Assert.False(root.TryGetProperty("Items", out _));
    }
}
