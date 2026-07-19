using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;
using System.Text.Json.Nodes;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterMultipleCollisionTests
{
    [Fact]
    public async Task SuggestAsync_WhenSeveralSuffixesExist_ReturnsNextAvailableSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        WhenItFailsDocumentationKeySuggester suggester = new();

        Response<DocumentationKeySuggestion> initialResponse = await suggester.SuggestAsync(
            options,
            "NETWORK",
            "Repeated collision sample");

        Assert.True(initialResponse.IsSuccess);
        DocumentationKeySuggestion initialSuggestion =
            Assert.IsType<DocumentationKeySuggestion>(initialResponse.Data);

        string errorCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.ErrorCatalogFileName);
        JsonObject catalog = Assert.IsType<JsonObject>(
            JsonNode.Parse(await File.ReadAllTextAsync(errorCatalogPath)));
        JsonArray errors = Assert.IsType<JsonArray>(catalog["errors"]);
        JsonObject sourceError = Assert.IsType<JsonObject>(errors[0]);

        AddCollision(errors, sourceError, initialSuggestion.DocumentationKey, 1);
        AddCollision(errors, sourceError, $"{initialSuggestion.DocumentationKey}-2", 2);
        AddCollision(errors, sourceError, $"{initialSuggestion.DocumentationKey}-3", 3);

        await File.WriteAllTextAsync(errorCatalogPath, catalog.ToJsonString());

        Response<DocumentationKeySuggestion> response = await suggester.SuggestAsync(
            options,
            "NETWORK",
            "Repeated collision sample");

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal(
            $"{initialSuggestion.DocumentationKey}-4",
            suggestion.DocumentationKey);
    }

    private static void AddCollision(
        JsonArray errors,
        JsonObject sourceError,
        string documentationKey,
        int sequence)
    {
        JsonObject collision = Assert.IsType<JsonObject>(sourceError.DeepClone());
        collision["id"] = $"AFW_TEST_COLLISION_{sequence:0000}";
        collision["code"] = 990000 + sequence;
        collision["name"] = $"TEST_COLLISION_{sequence}";
        collision["documentationKey"] = documentationKey;
        errors.Add(collision);
    }
}
