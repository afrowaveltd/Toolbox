using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorAddErrorTests
{
    [Fact]
    public async Task AddErrorAsync_ShouldCreateCompleteDefinitionUsingNextIdentity()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group, NextCodeSuggestion suggestion) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        int errorsBefore = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count;

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                new AddErrorRequest(
                    owner.Aliases.FirstOrDefault() ?? owner.Name,
                    group.CodePrefix,
                    category.Aliases.FirstOrDefault() ?? category.Name,
                    "New sample error",
                    "New sample error",
                    "A newly created sample error occurred.",
                    "warning"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(suggestion.Id, response.Data.Id);
        Assert.Equal(suggestion.Code, response.Data.Code);
        Assert.Equal("NEWSAMPLEERROR", response.Data.Name);
        Assert.Equal(owner.Name, response.Data.Owner);
        Assert.Equal(group.Name, response.Data.CodeGroup);
        Assert.Equal(group.CodePrefix, response.Data.CodePrefix);
        Assert.Equal(category.Name, response.Data.PrimaryCategory);
        Assert.Equal([category.Name], response.Data.Categories);
        Assert.Equal("New sample error", response.Data.Title);
        Assert.Equal("A newly created sample error occurred.", response.Data.Message);
        Assert.Equal("Warning", response.Data.DefaultSeverity);
        Assert.Empty(response.Data.Subcategories);
        Assert.Empty(response.Data.Tags);

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(errorsBefore + 1, saved.Errors.Count);
        ErrorDefinition? savedError = saved.Errors.FirstOrDefault(error => error.Id == suggestion.Id);
        Assert.NotNull(savedError);
        Assert.Equal(response.Data.Name, savedError.Name);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task AddErrorAsync_ShouldRejectDuplicateNameWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition existing = errors.Errors.First();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group, _) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                CreateRequest(owner.Name, group.Name, category.Name, existing.Name));

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorNameAlreadyExists");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(errors.Errors.Count, (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.Count);
    }

    [Fact]
    public async Task AddErrorAsync_ShouldRejectUnknownCategoryWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group, _) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                CreateRequest(owner.Name, group.Name, "DOES_NOT_EXIST", "Unique unknown category error"));

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task AddErrorAsync_ShouldRejectInvalidSeverityWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group, _) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                CreateRequest(owner.Name, group.Name, category.Name, "Invalid severity error") with
                {
                    DefaultSeverity = "Catastrophic"
                });

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "DefaultSeverityIsInvalid");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    public static TheoryData<AddErrorRequest, string> InvalidRequestCases =>
        new()
        {
            { new AddErrorRequest("", "GENERAL", "GENERAL", "NAME", "Title", "Message"), "OwnerNameIsEmpty" },
            { new AddErrorRequest("AFW", "", "GENERAL", "NAME", "Title", "Message"), "CodeGroupNameIsEmpty" },
            { new AddErrorRequest("AFW", "GENERAL", "", "NAME", "Title", "Message"), "PrimaryCategoryNameIsEmpty" },
            { new AddErrorRequest("AFW", "GENERAL", "GENERAL", "", "Title", "Message"), "ErrorNameIsEmpty" },
            { new AddErrorRequest("AFW", "GENERAL", "GENERAL", "NAME", "", "Message"), "ErrorTitleIsEmpty" },
            { new AddErrorRequest("AFW", "GENERAL", "GENERAL", "NAME", "Title", ""), "ErrorMessageIsEmpty" }
        };

    [Theory]
    [MemberData(nameof(InvalidRequestCases))]
    public async Task AddErrorAsync_ShouldRejectMissingRequiredValues(
        AddErrorRequest request,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                request);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedIssueCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static AddErrorRequest CreateRequest(
        string owner,
        string group,
        string category,
        string name) =>
        new(
            owner,
            group,
            category,
            name,
            "Test title",
            "Test message.");

    private static async Task<(ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group, NextCodeSuggestion Suggestion)>
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
                if (response.IsSuccess && response.Data is not null)
                {
                    return (owner, group, response.Data);
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
