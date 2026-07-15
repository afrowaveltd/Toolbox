using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class CommandJsonOutputTests
{
    [Fact]
    public void Serialize_ShouldUseStableCamelCaseEnvelope()
    {
        string json = CommandJsonOutput.Serialize(
            "sample-command",
            new SampleData("VALUE", 42));

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("sample-command", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.Equal("VALUE", data.GetProperty("name").GetString());
        Assert.Equal(42, data.GetProperty("count").GetInt32());
        Assert.False(data.TryGetProperty("Name", out _));
        Assert.False(root.TryGetProperty("SchemaVersion", out _));
    }

    [Fact]
    public void Serialize_WithEmptyCommand_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CommandJsonOutput.Serialize("", new SampleData("VALUE", 42)));
    }

    private sealed record SampleData(string Name, int Count);
}
