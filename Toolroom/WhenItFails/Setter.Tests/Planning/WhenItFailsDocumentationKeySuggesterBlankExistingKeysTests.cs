using System.Text.Json;
using System.Text.Json.Nodes;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterBlankExistingKeysTests
{
    [Fact]
    public async Task SuggestAsync_WhenExistingDocumentationKeysAreBlank_ReturnsBaseKey()
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
        Assert.True(errors.Count >= 3);

        Assert.IsType<JsonObject>(errors[0])["documentationKey"] = null;
        Assert.IsType<JsonObject>(errors[1])["documentationKey"] = string.Empty;
        Assert.IsType<JsonObject>(errors[2])["documentationKey"] = "   ";

        await File.WriteAllTextAsync(
            errorCatalogPath,
            catalog.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "NETWORK",
                "Blank existing keys sample");

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal(
            "when-it-fails/errors/network/blank-existing-keys-sample",
            suggestion.DocumentationKey);
        Assert.True(
            WhenItFailsDocumentationKeyFormatChecker.IsCanonical(
                suggestion.DocumentationKey));
    }
}
