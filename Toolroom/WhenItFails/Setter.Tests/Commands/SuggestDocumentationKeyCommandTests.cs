using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SuggestDocumentationKeyCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithPlainOutput_ReturnsCanonicalSuggestion()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            category.Name,
            "Příliš žluťoučký soubor",
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(
            $"when-it-fails/errors/{category.Name.ToLowerInvariant().Replace('_', '-')}/prilis-zlutoucky-soubor",
            output.Trim());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBaseKeyExists_ReturnsFirstAvailableSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition existing = Assert.Single(
            errors.Errors.Where(error => !string.IsNullOrWhiteSpace(error.DocumentationKey)).Take(1));

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
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_ReturnsStructuredSuggestion()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            category.Name,
            "JSON sample",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("suggest-doc-key", root.GetProperty("command").GetString());
        JsonElement data = root.GetProperty("data");
        Assert.Equal(category.Name, data.GetProperty("category").GetString());
        Assert.Equal("JSON sample", data.GetProperty("title").GetString());
        Assert.EndsWith("/json-sample", data.GetProperty("documentationKey").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategory_ReturnsLookupFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SuggestDocumentationKeyCommand.ExecuteAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "Sample title"
        ]);

        Assert.Equal(2, exitCode);
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
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

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        var response = await new JsonErrorCatalogLoader().LoadFromFileAsync(
            Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        var response = await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
            Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }
}
