using System.Text.Json;
using System.Text.Json.Nodes;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterWhitespaceCollisionTests
{
    [Fact]
    public async Task SuggestAsync_WhenExistingDocumentationKeyHasSurroundingWhitespace_ReturnsNextSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        string errorCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.ErrorCatalogFileName);

        JsonObject catalog = Assert.IsType<JsonObject>(
            JsonNode.Parse(await File.ReadAllTextAsync(errorCatalogPath)));
        JsonArray errors = Assert.IsType<JsonArray>(catalog["errors"]);
        JsonObject existing = errors
            .Select(node => Assert.IsType<JsonObject>(node))
            .First(error => !string.IsNullOrWhiteSpace(error["documentationKey"]?.GetValue<string>()));

        string category = Assert.NotNull(existing["primaryCategory"]).GetValue<string>();
        string title = Assert.NotNull(existing["title"]).GetValue<string>();
        string documentationKey = Assert.NotNull(existing["documentationKey"]).GetValue<string>();
        existing["documentationKey"] = $"  {documentationKey}\t";

        await File.WriteAllTextAsync(
            errorCatalogPath,
            catalog.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                category,
                title);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal($"{documentationKey}-2", suggestion.DocumentationKey);
    }
}
