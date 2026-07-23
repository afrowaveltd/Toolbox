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
    public void Write_ShouldWriteStableEnvelopeToConsole()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);

            CommandJsonOutput.Write(
                "sample-command",
                new SampleData("VALUE", 42));
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        using JsonDocument document = JsonDocument.Parse(output.ToString());
        JsonElement root = document.RootElement;

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("sample-command", root.GetProperty("command").GetString());
        Assert.Equal("VALUE", root.GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public void Write_WithNullData_ShouldWriteNullDataProperty()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);

            CommandJsonOutput.Write<object?>("sample-command", null);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        using JsonDocument document = JsonDocument.Parse(output.ToString());
        JsonElement root = document.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Write_WithBlankCommand_ShouldThrowWithoutWriting(string command)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);

            Assert.Throws<ArgumentException>(() =>
                CommandJsonOutput.Write(
                    command,
                    new SampleData("VALUE", 42)));
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public void Write_WithNullCommand_ShouldThrowWithoutWriting()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);

            Assert.Throws<ArgumentNullException>(() =>
                CommandJsonOutput.Write(
                    null!,
                    new SampleData("VALUE", 42)));
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public void Serialize_WithEmptyCommand_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CommandJsonOutput.Serialize("", new SampleData("VALUE", 42)));
    }

    [Fact]
    public void Serialize_WithWhitespaceCommand_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CommandJsonOutput.Serialize("   ", new SampleData("VALUE", 42)));
    }

    [Fact]
    public void Serialize_WithNullCommand_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CommandJsonOutput.Serialize(null!, new SampleData("VALUE", 42)));
    }

    [Fact]
    public void Serialize_WithNullData_ShouldWriteNullDataProperty()
    {
        string json = CommandJsonOutput.Serialize<object?>("sample-command", null);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    private sealed record SampleData(string Name, int Count);
}
