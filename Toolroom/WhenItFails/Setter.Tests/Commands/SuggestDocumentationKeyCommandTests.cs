using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
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
    public async Task ExecuteAsync_WithPlainOutput_MatchesApplicationSuggester()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        const string title = "Shared suggester result";

        var serviceResponse = await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
            workspace.ProjectRootPath,
            category.Name,
            title);
        Assert.True(serviceResponse.IsSuccess);
        Assert.NotNull(serviceResponse.Data);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            category.Name,
            title,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(serviceResponse.Data.DocumentationKey, output.Trim());
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
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutputAndUnknownCategory_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "JSON failure sample",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("suggest-doc-key", root.GetProperty("command").GetString());
        JsonElement data = root.GetProperty("data");
        Assert.Equal("DOES_NOT_EXIST", data.GetProperty("category").GetString());
        Assert.Equal("JSON failure sample", data.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("documentationKey").ValueKind);
        Assert.Equal("CategoryNotFound", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutputAndMissingCategoryCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        File.Delete(Path.Combine(workspace.WhenItFailsJsonsPath, "categories.en.json"));

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "NETWORK",
            "Missing category catalog",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        AssertStructuredJsonFailure(
            output,
            expectedCategory: "NETWORK",
            expectedTitle: "Missing category catalog");
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutputAndMalformedErrorCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        await File.WriteAllTextAsync(
            Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json"),
            "{ not valid json");

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            category.Name,
            "Malformed error catalog",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        AssertStructuredJsonFailure(
            output,
            expectedCategory: category.Name,
            expectedTitle: "Malformed error catalog");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategory_ReturnsLookupFailureWithoutWriting()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string errorCatalogPath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        string catalogBefore = await File.ReadAllTextAsync(errorCatalogPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SuggestDocumentationKeyCommand.ExecuteAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "Sample title"
        ]);

        Assert.Equal(2, exitCode);
        Assert.Equal(catalogBefore, await File.ReadAllTextAsync(errorCatalogPath));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryCatalogIsMissing_ReturnsFailureWithoutWriting()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string categoryCatalogPath = Path.Combine(workspace.WhenItFailsJsonsPath, "categories.en.json");
        string errorCatalogPath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        string errorCatalogBefore = await File.ReadAllTextAsync(errorCatalogPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        File.Delete(categoryCatalogPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "NETWORK",
            "Sample title"
        ]);

        Assert.Equal(2, exitCode);
        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.Equal(errorCatalogBefore, await File.ReadAllTextAsync(errorCatalogPath));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WhenErrorCatalogIsMalformed_ReturnsFailureWithoutWriting()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        string errorCatalogPath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        const string malformedCatalog = "{ not valid json";
        await File.WriteAllTextAsync(errorCatalogPath, malformedCatalog);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            category.Name,
            "Sample title"
        ]);

        Assert.Equal(2, exitCode);
        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.Equal(malformedCatalog, await File.ReadAllTextAsync(errorCatalogPath));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "suggest-doc-key" },
            new[] { "suggest-doc-key", "." },
            new[] { "suggest-doc-key", ".", "NETWORK" },
            new[] { "suggest-doc-key", ".", "NETWORK", "Title", "--plain", "--json" },
            new[] { "suggest-doc-key", ".", "NETWORK", "Title", "--unknown" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await SuggestDocumentationKeyCommand.ExecuteAsync(args));
    }

    private static void AssertStructuredJsonFailure(
        string output,
        string expectedCategory,
        string expectedTitle)
    {
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("suggest-doc-key", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.Equal(expectedCategory, data.GetProperty("category").GetString());
        Assert.Equal(expectedTitle, data.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("documentationKey").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
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

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
