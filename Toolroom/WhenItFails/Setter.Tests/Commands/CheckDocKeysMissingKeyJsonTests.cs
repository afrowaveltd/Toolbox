using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysMissingKeyJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithMissingKeyAndJson_WritesOnlyMissingKeyIssue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        json = json.Replace(
            "\"documentationKey\": \"when-it-fails/errors/general/unknown-error\"",
            "\"documentationKey\": \"\"",
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        JsonElement keys = data.GetProperty("keys");
        JsonElement format = data.GetProperty("format");
        JsonElement missingKeys = keys.GetProperty("missingKeys");
        JsonElement duplicateKeys = keys.GetProperty("duplicateKeys");
        JsonElement invalidKeys = format.GetProperty("invalidKeys");

        Assert.False(data.GetProperty("isValid").GetBoolean());
        JsonElement missingKey = Assert.Single(missingKeys.EnumerateArray());
        Assert.Equal("AFW_GEN_0001", missingKey.GetProperty("errorId").GetString());
        Assert.Equal(100001, missingKey.GetProperty("errorCode").GetInt32());
        Assert.Equal("UNKNOWNERROR", missingKey.GetProperty("errorName").GetString());
        Assert.Empty(duplicateKeys.EnumerateArray());
        Assert.Empty(invalidKeys.EnumerateArray());
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(
        string[] args)
    {
        IAnsiConsole originalConsole = AnsiConsole.Console;
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await CheckDocKeysCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            AnsiConsole.Console = originalConsole;
        }
    }
}
