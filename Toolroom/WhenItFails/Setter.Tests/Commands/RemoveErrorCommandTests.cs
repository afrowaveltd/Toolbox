using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class RemoveErrorCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingUnreferencedError_RemovesDefinitionAndCreatesBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition added = await AddTemporaryErrorAsync(workspace);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await RemoveErrorCommand.ExecuteAsync(
        [
            "remove-error",
            workspace.ProjectRootPath,
            added.Id
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        ErrorCatalogDocument catalog = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.DoesNotContain(catalog.Errors, error => error.Id == added.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownError_ReturnsDomainFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await RemoveErrorCommand.ExecuteAsync(
        [
            "remove-error",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "remove-error" },
            new[] { "remove-error", "." },
            new[] { "remove-error", ".", "" },
            new[] { "remove-error", ".", "AFW_GEN_0001", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await RemoveErrorCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorDefinition> AddTemporaryErrorAsync(
        TemporaryWhenItFailsWorkspace workspace)
    {
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                new AddErrorRequest(
                    owner.Name,
                    group.Name,
                    category.Name,
                    "Temporary removable error",
                    "Temporary removable error",
                    "A temporary removable error occurred."));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<(ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group)>
        FindCompatibleIdentityAsync(string projectRootPath, string jsonsPath)
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
