using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ErrorRemoveSubcategoryCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_RemovesCanonicalSubcategoryAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, string subcategory) =
            await LoadErrorAndUnusedSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorDefinition> addResponse =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                subcategory);
        Assert.True(addResponse.IsSuccess);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-remove-subcategory",
            workspace.ProjectRootPath,
            error.Name,
            subcategory.ToLowerInvariant().Replace('_', '-'),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("error-remove-subcategory", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(subcategory, data.GetProperty("removedSubcategory").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.DoesNotContain(
            data.GetProperty("error").GetProperty("subcategories").EnumerateArray(),
            item => string.Equals(item.GetString(), subcategory, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.DoesNotContain(subcategory, saved.Subcategories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeSubcategory_RemovesSubcategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, string subcategory) =
            await LoadErrorAndUnusedSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorDefinition> addResponse =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                subcategory);
        Assert.True(addResponse.IsSuccess);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-remove-subcategory",
            workspace.ProjectRootPath,
            error.Id,
            "--json",
            subcategory
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            subcategory,
            document.RootElement
                .GetProperty("data")
                .GetProperty("removedSubcategory")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubcategoryAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, string subcategory) =
            await LoadErrorAndUnusedSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-remove-subcategory",
            workspace.ProjectRootPath,
            error.Id,
            subcategory,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("removedSubcategory").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorRemoveSubcategoryCommand.ExecuteAsync(
        [
            "error-remove-subcategory",
            workspace.ProjectRootPath,
            "AFW_NET_0001",
            "TIMEOUT",
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
            int exitCode = await ErrorRemoveSubcategoryCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<(ErrorDefinition Error, string Subcategory)>
        LoadErrorAndUnusedSubcategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        string[] subcategories = catalog.Errors
            .SelectMany(error => error.Subcategories)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (ErrorDefinition error in catalog.Errors)
        {
            string? subcategory = subcategories.FirstOrDefault(candidate =>
                !error.Subcategories.Contains(candidate, StringComparer.OrdinalIgnoreCase));
            if (subcategory is not null)
            {
                return (error, subcategory);
            }
        }

        throw new InvalidOperationException(
            "The test workspace contains no unused subcategory for any error.");
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadCatalogAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
