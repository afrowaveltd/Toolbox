using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysInvalidFormatJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithNonCanonicalKeyAndJson_WritesOnlyInvalidFormatIssue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        const string invalidDocumentationKey =
            "When-It-Fails/errors/general/unknown-error";
        json = json.Replace(
            "when-it-fails/errors/general/unknown-error",
            invalidDocumentationKey,
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
        JsonElement invalidKey = Assert.Single(invalidKeys.EnumerateArray());
        Assert.Equal(
            invalidDocumentationKey,
            invalidKey.GetProperty("documentationKey").GetString());
        Assert.Equal("AFW_GEN_0001", invalidKey.GetProperty("errorId").GetString());
        Assert.Equal(100001, invalidKey.GetProperty("errorCode").GetInt32());
        Assert.Equal("UNKNOWNERROR", invalidKey.GetProperty("errorName").GetString());
        Assert.Empty(missingKeys.EnumerateArray());
        Assert.Empty(duplicateKeys.EnumerateArray());
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
