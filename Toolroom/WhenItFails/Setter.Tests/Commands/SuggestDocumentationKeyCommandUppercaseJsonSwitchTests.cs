using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SuggestDocumentationKeyCommandUppercaseJsonSwitchTests
{
    [Fact]
    public async Task ExecuteAsync_WithUppercaseJsonSwitch_ReturnsStructuredJson()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "NETWORK",
            "Uppercase JSON switch sample",
            "--JSON"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("suggest-doc-key", root.GetProperty("command").GetString());
        JsonElement data = root.GetProperty("data");
        Assert.Equal("NETWORK", data.GetProperty("category").GetString());
        Assert.Equal("Uppercase JSON switch sample", data.GetProperty("title").GetString());
        Assert.Equal(
            "when-it-fails/errors/network/uppercase-json-switch-sample",
            data.GetProperty("documentationKey").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(
        string[] args)
    {
        IAnsiConsole originalConsole = AnsiConsole.Console;
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        IAnsiConsole testConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(output)
        });

        try
        {
            Console.SetOut(output);
            AnsiConsole.Console = testConsole;
            int exitCode = await SuggestDocumentationKeyCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            AnsiConsole.Console = originalConsole;
            Console.SetOut(originalOutput);
        }
    }
}
