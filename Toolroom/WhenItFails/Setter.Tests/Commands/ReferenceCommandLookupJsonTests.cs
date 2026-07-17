using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ReferenceCommandLookupJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithErrorsJson_WritesLimitedStructuredList()
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
            ["reference", "errors", "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement value = data.GetProperty("value");

        Assert.Equal("reference", root.GetProperty("command").GetString());
        Assert.Equal("errors", data.GetProperty("subcommand").GetString());
        Assert.Equal(37, value.GetProperty("matchingCount").GetInt32());
        Assert.Equal(20, value.GetProperty("returnedCount").GetInt32());
        Assert.Equal(20, value.GetProperty("errors").GetArrayLength());
        Assert.False(value.GetProperty("options").GetProperty("showAll").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WithFilteredErrorsJson_ReturnsMatchingErrorsAndOptions()
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
            ["reference", "errors", "--group", "CONFIGURATION", "--all", "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement value = document.RootElement.GetProperty("data").GetProperty("value");
        JsonElement options = value.GetProperty("options");

        Assert.True(options.GetProperty("showAll").GetBoolean());
        Assert.Equal("CONFIGURATION", options.GetProperty("codeGroup").GetString());
        Assert.Equal(value.GetProperty("matchingCount").GetInt32(), value.GetProperty("returnedCount").GetInt32());

        foreach (JsonElement error in value.GetProperty("errors").EnumerateArray())
        {
            Assert.Equal("CONFIGURATION", error.GetProperty("codeGroup").GetString());
        }
    }

    [Theory]
    [InlineData("AFW-CFG-0001")]
    [InlineData("MissingConfigurationValue")]
    public async Task ExecuteAsync_WithReferenceErrorJson_ReturnsCompleteMatch(string lookup)
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
            ["reference", "error", lookup, "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        JsonElement value = data.GetProperty("value");

        Assert.Equal("error", data.GetProperty("subcommand").GetString());
        Assert.True(value.GetProperty("found").GetBoolean());
        Assert.Equal("AFW-CFG-0001", value.GetProperty("item").GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, value.GetProperty("failureCode").ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_WithReferenceProfileJson_ReturnsCompleteProfile()
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
            ["reference", "profile", "web_api", "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement value = document.RootElement.GetProperty("data").GetProperty("value");

        Assert.True(value.GetProperty("found").GetBoolean());
        Assert.Equal("WEB_API", value.GetProperty("item").GetProperty("name").GetString());
    }

    [Theory]
    [InlineData("error", "AFW-NOPE-0001", "ReferenceErrorNotFound")]
    [InlineData("profile", "NOPE", "ReferenceProfileNotFound")]
    public async Task ExecuteAsync_WithUnknownLookupAndJson_WritesStructuredFailure(
        string subcommand,
        string lookup,
        string expectedFailureCode)
    {
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
            ["reference", subcommand, lookup, "--json"]);

        Assert.Equal(1, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement value = document.RootElement.GetProperty("data").GetProperty("value");

        Assert.False(value.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, value.GetProperty("item").ValueKind);
        Assert.Equal(expectedFailureCode, value.GetProperty("failureCode").GetString());
    }

    [Theory]
    [InlineData("reference", "errors", "--json", "--json")]
    [InlineData("reference", "error", "AFW-CFG-0001", "--unknown")]
    [InlineData("reference", "profile", "WEB_API", "--json", "--json")]
    public async Task ExecuteAsync_WithInvalidJsonArguments_ReturnsInputError(params string[] args)
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
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
