using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SuggestDocumentationKeyCommandAliasJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithCategoryAlias_ReturnsCanonicalCategoryInJson()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadCategoryWithAliasAsync(
            workspace.WhenItFailsJsonsPath);
        string alias = category.Aliases[0];

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            alias,
            "Alias JSON sample",
            "--json"
        ]);

        Assert.Equal(0, exitCode);

        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.Equal(category.Name, data.GetProperty("category").GetString());
        Assert.Equal("Alias JSON sample", data.GetProperty("title").GetString());
        Assert.EndsWith(
            $"/{category.Name.ToLowerInvariant().Replace('_', '-')}/alias-json-sample",
            data.GetProperty("documentationKey").GetString(),
            StringComparison.Ordinal);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
    }

    private static async Task<ErrorCategoryDefinition> LoadCategoryWithAliasAsync(string jsonsPath)
    {
        var response = await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
            Path.Combine(jsonsPath, "categories.en.json"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault(
            candidate => candidate.Aliases.Count > 0);

        Assert.NotNull(category);
        return category;
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
