using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsAliasNormalizationTests
{
    [Fact]
    public async Task SuggestAsync_WithNormalizedAliasInput_ReturnsCanonicalCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        ErrorCategoryDefinition category = await LoadCategoryWithAliasAsync(
            workspace.WhenItFailsJsonsPath);
        string normalizedAliasInput = $"  {category.Aliases[0].ToLowerInvariant()}  ";

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                normalizedAliasInput,
                "Normalized alias options");

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal(category.Name, suggestion.Category);
        Assert.Equal("Normalized alias options", suggestion.Title);
        Assert.EndsWith(
            "/normalized-alias-options",
            suggestion.DocumentationKey,
            StringComparison.Ordinal);
        Assert.True(
            WhenItFailsDocumentationKeyFormatChecker.IsCanonical(
                suggestion.DocumentationKey));
    }

    private static async Task<ErrorCategoryDefinition> LoadCategoryWithAliasAsync(
        string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories
            .FirstOrDefault(candidate => candidate.Aliases.Count > 0);
        Assert.NotNull(category);
        return category;
    }
}
