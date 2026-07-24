using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterCaseInsensitiveCollisionTests
{
    [Fact]
    public async Task SuggestAsync_WithUppercaseExistingKey_ReturnsNumericSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string errorCatalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        ErrorCatalogDocument catalog = await LoadErrorsAsync(errorCatalogPath);
        ErrorDefinition existing = Assert.Single(
            catalog.Errors
                .Where(error => !string.IsNullOrWhiteSpace(error.DocumentationKey))
                .Take(1));
        string existingKey = Assert.IsType<string>(existing.DocumentationKey);
        string rawJson = await File.ReadAllTextAsync(errorCatalogPath);
        string uppercaseKey = existingKey.ToUpperInvariant();

        Assert.Contains(existingKey, rawJson, StringComparison.Ordinal);
        await File.WriteAllTextAsync(
            errorCatalogPath,
            rawJson.Replace(
                existingKey,
                uppercaseKey,
                StringComparison.Ordinal));

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                existing.PrimaryCategory,
                existing.Title);

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal($"{existingKey}-2", suggestion.DocumentationKey);
        Assert.True(
            DocumentationKeyFormat.IsCanonical(
                suggestion.DocumentationKey));
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(
        string errorCatalogPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(errorCatalogPath);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
