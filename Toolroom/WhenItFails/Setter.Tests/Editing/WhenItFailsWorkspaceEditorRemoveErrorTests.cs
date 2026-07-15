using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorRemoveErrorTests
{
    [Fact]
    public async Task RemoveErrorAsync_ShouldRemoveUnreferencedErrorAndCreateOneBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition added = await AddDisposableErrorAsync(workspace);
        int backupsBeforeRemoval = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        int errorsBeforeRemoval = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(
                workspace.ProjectRootPath,
                added.Name);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(added.Id, response.Data.Id);
        Assert.Equal(added.Code, response.Data.Code);
        Assert.Equal(backupsBeforeRemoval + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(errorsBeforeRemoval - 1, saved.Errors.Count);
        Assert.DoesNotContain(saved.Errors, error => error.Id == added.Id);
    }

    [Fact]
    public async Task RemoveErrorAsync_ShouldRejectErrorReferencedByProfileWithoutErrorBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition added = await AddDisposableErrorAsync(workspace);
        ErrorProfileDefinition profile = await LoadFirstProfileAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> referenceResponse =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddErrorAsync(
                workspace.ProjectRootPath,
                profile.Name,
                added.Id);
        Assert.True(referenceResponse.IsSuccess);
        int backupsBeforeRemoval = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        int errorsBeforeRemoval = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(
                workspace.ProjectRootPath,
                added.Id);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorIsReferencedByProfiles");
        Assert.Contains(profile.Name, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(backupsBeforeRemoval, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(errorsBeforeRemoval, (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count);
    }

    [Fact]
    public async Task RemoveErrorAsync_ShouldRejectUnknownErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        int errorsBefore = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(errorsBefore, (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count);
    }

    [Fact]
    public async Task RemoveErrorAsync_ShouldRejectEmptyLookupWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(
                workspace.ProjectRootPath,
                " ");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorLookupIsEmpty");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<ErrorDefinition> AddDisposableErrorAsync(
        TemporaryWhenItFailsWorkspace workspace)
    {
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                new AddErrorRequest(
                    owner.Name,
                    group.Name,
                    category.Name,
                    $"Disposable error {Guid.NewGuid():N}",
                    "Disposable error",
                    "A disposable test error occurred."));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
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

    private static async Task<ErrorProfileDefinition> LoadFirstProfileAsync(string jsonsPath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileCatalogDocument catalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorProfileDefinition? profile = catalog.Profiles.FirstOrDefault();
        Assert.NotNull(profile);
        return profile;
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
