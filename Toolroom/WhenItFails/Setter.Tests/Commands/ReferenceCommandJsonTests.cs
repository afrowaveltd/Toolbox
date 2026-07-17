using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ReferenceCommandJsonTests
{
    [Theory]
    [InlineData("summary")]
    [InlineData("profiles")]
    [InlineData("categories")]
    [InlineData("code-groups")]
    public async Task ExecuteAsync_WithJsonListSubcommand_WritesStableEnvelope(
        string subcommand)
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "reference",
            subcommand,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("reference", root.GetProperty("command").GetString());
        Assert.Equal(subcommand, data.GetProperty("subcommand").GetString());
        Assert.True(data.TryGetProperty("value", out JsonElement value));

        if (subcommand == "summary")
        {
            Assert.Equal(JsonValueKind.Object, value.ValueKind);
            Assert.Equal(1, value.GetProperty("ownerCount").GetInt32());
            Assert.Equal(16, value.GetProperty("categoryCount").GetInt32());
            Assert.Equal(9, value.GetProperty("codeGroupCount").GetInt32());
            Assert.Equal(5, value.GetProperty("profileCount").GetInt32());
            Assert.Equal(37, value.GetProperty("errorCount").GetInt32());
        }
        else
        {
            Assert.Equal(JsonValueKind.Array, value.ValueKind);
            Assert.True(value.GetArrayLength() > 0);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonSwitchOnly_UsesSummarySubcommand()
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "reference",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.Equal("summary", data.GetProperty("subcommand").GetString());
        Assert.Equal(JsonValueKind.Object, data.GetProperty("value").ValueKind);
    }

    [Theory]
    [InlineData("summary", "--json", "--json")]
    [InlineData("profiles", "--unknown", null)]
    public async Task ExecuteAsync_WithInvalidJsonListOptions_ReturnsInputError(
        string subcommand,
        string first,
        string? second)
    {
        List<string> args = ["reference", subcommand, first];

        if (second is not null)
        {
            args.Add(second);
        }

        int exitCode = await ReferenceCommand.ExecuteAsync(args.ToArray());

        Assert.Equal(1, exitCode);
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(
        string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ReferenceCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
