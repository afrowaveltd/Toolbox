using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysDuplicateJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithDuplicateKeyAndJson_WritesOnlyDuplicateKeyIssue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        const string duplicateDocumentationKey =
            "when-it-fails/errors/general/unknown-error";
        json = json.Replace(
            "when-it-fails/errors/general/operation-failed",
            duplicateDocumentationKey,
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
        Assert.Empty(missingKeys.EnumerateArray());
        Assert.Empty(invalidKeys.EnumerateArray());

        JsonElement duplicateKey = Assert.Single(duplicateKeys.EnumerateArray());
        Assert.Equal(
            duplicateDocumentationKey,
            duplicateKey.GetProperty("documentationKey").GetString());

        JsonElement[] errors = duplicateKey
            .GetProperty("errors")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(2, errors.Length);
        Assert.Contains(
            errors,
            error => string.Equals(
                error.GetProperty("errorId").GetString(),
                "AFW_GEN_0001",
                StringComparison.Ordinal));
        Assert.Contains(
            errors,
            error => string.Equals(
                error.GetProperty("errorId").GetString(),
                "AFW_GEN_0002",
                StringComparison.Ordinal));
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
