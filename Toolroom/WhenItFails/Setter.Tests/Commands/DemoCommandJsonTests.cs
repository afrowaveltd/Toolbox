using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class DemoCommandJsonTests
{
    [Fact]
    public void Execute_WithJsonOutput_WritesStableEnvelopeAndSampleValidation()
    {
        (int exitCode, string output) = ExecuteWithCapturedOutput(
        [
            "demo",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement validation = data.GetProperty("validation");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("demo", root.GetProperty("command").GetString());
        Assert.Equal("Jsons/WhenItFails/errors.json", data.GetProperty("sourcePath").GetString());
        Assert.False(validation.GetProperty("isValid").GetBoolean());

        string validationJson = validation.GetRawText();
        Assert.Contains("MissingCatalogId", validationJson, StringComparison.Ordinal);
        Assert.Contains("UnknownProfileIncludeOwner", validationJson, StringComparison.Ordinal);
        Assert.Contains("PrimaryCategoryNotListed", validationJson, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WithoutArguments_PreservesOriginalSuccessfulBehavior()
    {
        int exitCode = DemoCommand.Execute();

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsTrueAndJsonTrue()
    {
        bool result = DemoCommand.TryParseOptions(
            ["demo", "--JSON"],
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(useJsonOutput);
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--unknown", null)]
    public void Execute_WithInvalidOptions_ReturnsInputError(
        string first,
        string? second)
    {
        List<string> args = ["demo", first];

        if (second is not null)
        {
            args.Add(second);
        }

        int exitCode = DemoCommand.Execute(args.ToArray());

        Assert.Equal(1, exitCode);
    }

    private static (int ExitCode, string Output) ExecuteWithCapturedOutput(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = DemoCommand.Execute(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
