using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class AddErrorCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_AddsErrorAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.Name,
            category.Name,
            "JSON sample error",
            "JSON sample error",
            "A sample error was created with JSON output.",
            "warning",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("add-error", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("added").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);

        JsonElement error = data.GetProperty("error");
        Assert.Equal("JSON_SAMPLE_ERROR", error.GetProperty("name").GetString());
        Assert.Equal(owner.Name, error.GetProperty("owner").GetString());
        Assert.Equal(group.Name, error.GetProperty("codeGroup").GetString());
        Assert.Equal(category.Name, error.GetProperty("primaryCategory").GetString());
        Assert.Equal("Warning", error.GetProperty("defaultSeverity").GetString());
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeSeverity_UsesExplicitSeverity()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.Name,
            category.Name,
            "JSON option order sample",
            "JSON option order sample",
            "The JSON switch may precede severity.",
            "--json",
            "critical"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            "Critical",
            document.RootElement
                .GetProperty("data")
                .GetProperty("error")
                .GetProperty("defaultSeverity")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutputAndInvalidSeverity_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.Name,
            category.Name,
            "Invalid JSON severity sample",
            "Invalid JSON severity sample",
            "This definition must not be saved.",
            "Catastrophic",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("added").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal("DefaultSeverityIsInvalid", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("Warning", "Critical")]
    public async Task ExecuteAsync_WithDuplicateOptionalArguments_ReturnsCommandInputError(
        string firstOptionalArgument,
        string secondOptionalArgument)
    {
        string[] args =
        [
            "add-error",
            ".",
            "AFW",
            "GENERAL",
            "GENERAL",
            "NAME",
            "Title",
            "Message",
            firstOptionalArgument,
            secondOptionalArgument
        ];

        Assert.Equal(1, await AddErrorCommand.ExecuteAsync(args));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await AddErrorCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<(ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group)>
        FindCompatiblePairAsync(string projectRootPath, string jsonsPath)
    {
        ErrorOwnerCatalogDocument owners = await LoadOwnersAsync(jsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadGroupsAsync(jsonsPath);
        WhenItFailsNextCodeFinder finder = new();

        foreach (ErrorOwnerDefinition owner in owners.Owners)
        {
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                Response<NextCodeSuggestion> response = await finder.FindAsync(
                    projectRootPath,
                    owner.Name,
                    group.Name);
                if (response.IsSuccess)
                {
                    return (owner, group);
                }
            }
        }

        throw new InvalidOperationException("The test workspace contains no compatible owner and code-group pair.");
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnersAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCodeGroupCatalogDocument> LoadGroupsAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
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
