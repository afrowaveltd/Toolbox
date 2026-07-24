using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class AddErrorCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsErrorAndCreatesOneBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int errorsBefore = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        string expectedDocumentationKey =
            $"when-it-fails/errors/{category.Name.ToLowerInvariant().Replace('_', '-')}/cli-sample-error";

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.CodePrefix,
            category.Name,
            "CLI sample error",
            "CLI sample error",
            "A sample error was created from the CLI.",
            "warning"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Documentation key:", output, StringComparison.Ordinal);
        Assert.Contains(expectedDocumentationKey, output, StringComparison.Ordinal);

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(errorsBefore + 1, saved.Errors.Count);
        ErrorDefinition? added = saved.Errors.FirstOrDefault(error => error.Name == "CLI_SAMPLE_ERROR");
        Assert.NotNull(added);
        Assert.Equal(owner.Name, added.Owner);
        Assert.Equal(group.Name, added.CodeGroup);
        Assert.Equal(category.Name, added.PrimaryCategory);
        Assert.Equal("Warning", added.DefaultSeverity);
        Assert.Equal(expectedDocumentationKey, added.DocumentationKey);
        Assert.True(
            DocumentationKeyFormat.IsCanonical(added.DocumentationKey!));
        Assert.True(new WhenItFailsDocumentationKeyChecker().Check(saved).IsValid);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSeverity_UsesErrorSeverity()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await AddErrorCommand.ExecuteAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.Name,
            category.Name,
            "Default severity sample",
            "Default severity sample",
            "The default severity should be used."
        ]);

        Assert.Equal(0, exitCode);
        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition? added = saved.Errors.FirstOrDefault(error => error.Name == "DEFAULT_SEVERITY_SAMPLE");
        Assert.NotNull(added);
        Assert.Equal("Error", added.DefaultSeverity);
        Assert.False(string.IsNullOrWhiteSpace(added.DocumentationKey));
        Assert.True(new WhenItFailsDocumentationKeyChecker().Check(saved).IsValid);
        Assert.True(new WhenItFailsDocumentationKeyFormatChecker().Check(saved).IsValid);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSeverity_ReturnsEditorFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        int errorsBefore = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;

        int exitCode = await AddErrorCommand.ExecuteAsync(
        [
            "add-error",
            workspace.ProjectRootPath,
            owner.Name,
            group.Name,
            category.Name,
            "Invalid severity CLI sample",
            "Invalid severity CLI sample",
            "This definition must not be saved.",
            "Catastrophic"
        ]);

        Assert.Equal(2, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(errorsBefore, (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "add-error" },
            new[] { "add-error", ".", "AFW", "GENERAL", "GENERAL", "NAME", "Title" },
            new[] { "add-error", ".", "AFW", "GENERAL", "GENERAL", "NAME", "Title", "Message", "Error", "EXTRA" },
            new[] { "add-error", ".", "AFW", "GENERAL", "GENERAL", "", "Title", "Message" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await AddErrorCommand.ExecuteAsync(args));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        IAnsiConsole originalConsole = AnsiConsole.Console;
        using StringWriter output = new();
        IAnsiConsole testConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(output)
        });

        try
        {
            AnsiConsole.Console = testConsole;
            int exitCode = await AddErrorCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            AnsiConsole.Console = originalConsole;
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

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
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
