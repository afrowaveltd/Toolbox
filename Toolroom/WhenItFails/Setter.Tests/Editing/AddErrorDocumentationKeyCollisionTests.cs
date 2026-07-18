using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class AddErrorDocumentationKeyCollisionTests
{
    [Fact]
    public async Task AddErrorAsync_WithSameCategoryAndTitle_UsesFirstAvailableNumericSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> first = await editor.AddErrorAsync(
            workspace.ProjectRootPath,
            new AddErrorRequest(
                owner.Name,
                group.Name,
                category.Name,
                "First collision sample",
                "Shared documentation title",
                "The first error uses the base documentation key."));

        Response<ErrorDefinition> second = await editor.AddErrorAsync(
            workspace.ProjectRootPath,
            new AddErrorRequest(
                owner.Name,
                group.Name,
                category.Name,
                "Second collision sample",
                "Shared documentation title",
                "The second error uses the next available documentation key."));

        Assert.True(first.IsSuccess, first.Message);
        Assert.NotNull(first.Data);
        Assert.True(second.IsSuccess, second.Message);
        Assert.NotNull(second.Data);

        string categorySegment = category.Name.ToLowerInvariant().Replace('_', '-');
        string baseKey =
            $"when-it-fails/errors/{categorySegment}/shared-documentation-title";
        Assert.Equal(baseKey, first.Data.DocumentationKey);
        Assert.Equal($"{baseKey}-2", second.Data.DocumentationKey);

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.True(new WhenItFailsDocumentationKeyChecker().Check(saved).IsValid);
        Assert.True(new WhenItFailsDocumentationKeyFormatChecker().Check(saved).IsValid);
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

        throw new InvalidOperationException(
            "The test workspace contains no compatible owner and code-group pair.");
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnersAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCodeGroupCatalogDocument> LoadGroupsAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }
}