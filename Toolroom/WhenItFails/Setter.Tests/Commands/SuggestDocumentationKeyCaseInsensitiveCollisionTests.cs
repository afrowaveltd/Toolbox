using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SuggestDocumentationKeyCaseInsensitiveCollisionTests
{
    [Fact]
    public async Task ExecuteAsync_WithUppercaseExistingKey_ReturnsCanonicalSuffixWithoutWriting()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string errorCatalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        ErrorDefinition existing = await LoadErrorWithDocumentationKeyAsync(errorCatalogPath);

        string originalCatalog = await File.ReadAllTextAsync(errorCatalogPath);
        string uppercaseKey = existing.DocumentationKey!.ToUpperInvariant();
        string modifiedCatalog = originalCatalog.Replace(
            existing.DocumentationKey,
            uppercaseKey,
            StringComparison.Ordinal);
        Assert.NotEqual(originalCatalog, modifiedCatalog);
        await File.WriteAllTextAsync(errorCatalogPath, modifiedCatalog);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            existing.PrimaryCategory,
            existing.Title,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal($"{existing.DocumentationKey}-2", output.Trim());
        Assert.Equal(modifiedCatalog, await File.ReadAllTextAsync(errorCatalogPath));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<ErrorDefinition> LoadErrorWithDocumentationKeyAsync(
        string errorCatalogPath)
    {
        var response = await new JsonErrorCatalogLoader().LoadFromFileAsync(errorCatalogPath);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCatalogDocument catalog =
            new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate.DocumentationKey));
        Assert.NotNull(error);
        return error;
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

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
