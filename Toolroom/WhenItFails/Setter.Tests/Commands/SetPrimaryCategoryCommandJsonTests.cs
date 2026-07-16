using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SetPrimaryCategoryCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_UpdatesCategoryAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndDifferentCategoryAsync(workspace.WhenItFailsJsonsPath);
        string categoryLookup = category.Aliases.FirstOrDefault() ?? category.Name;
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Id,
            categoryLookup,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("set-primary-category", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
        Assert.Equal(
            category.Name,
            data.GetProperty("error").GetProperty("primaryCategory").GetString());
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeCategory_UpdatesCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndDifferentCategoryAsync(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            "--json",
            category.Name
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            category.Name,
            document.RootElement
                .GetProperty("data")
                .GetProperty("error")
                .GetProperty("primaryCategory")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategoryAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Id,
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndDifferentCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetPrimaryCategoryCommand.ExecuteAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Id,
            category.Name,
            "--json",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await SetPrimaryCategoryCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<(ErrorDefinition Error, ErrorCategoryDefinition Category)>
        LoadErrorAndDifferentCategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument errors = await LoadErrorsAsync(jsonsPath);
        ErrorCategoryCatalogDocument categories = await LoadCategoriesAsync(jsonsPath);

        foreach (ErrorDefinition error in errors.Errors)
        {
            ErrorCategoryDefinition? category = categories.Categories.FirstOrDefault(candidate =>
                !string.Equals(
                    candidate.Name,
                    error.PrimaryCategory,
                    StringComparison.OrdinalIgnoreCase));

            if (category is not null)
            {
                return (error, category);
            }
        }

        throw new InvalidOperationException(
            "The test workspace contains no alternative primary category for any error.");
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryCatalogDocument> LoadCategoriesAsync(string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
