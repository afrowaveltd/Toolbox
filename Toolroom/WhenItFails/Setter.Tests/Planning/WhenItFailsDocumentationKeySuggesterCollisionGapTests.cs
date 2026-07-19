using System.Text.Json;
using System.Text.Json.Nodes;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterCollisionGapTests
{
    [Fact]
    public async Task SuggestAsync_WhenSecondSuffixIsFree_ReturnsFirstAvailableSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        WhenItFailsDocumentationKeySuggester suggester = new();

        Response<DocumentationKeySuggestion> initialResponse = await suggester.SuggestAsync(
            options,
            "NETWORK",
            "Collision gap sample");

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
        JsonObject template = Assert.IsType<JsonObject>(errors[0]?.DeepClone());

        errors.Add(CreateCollision(template, "AFW_TEST_9901", 990001, "COLLISIONGAPBASE", initialSuggestion.DocumentationKey));
        errors.Add(CreateCollision(template, "AFW_TEST_9903", 990003, "COLLISIONGAPTHREE", $"{initialSuggestion.DocumentationKey}-3"));

        await File.WriteAllTextAsync(
            errorCatalogPath,
            catalog.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Response<DocumentationKeySuggestion> response = await suggester.SuggestAsync(
            options,
            "NETWORK",
            "Collision gap sample");

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal($"{initialSuggestion.DocumentationKey}-2", suggestion.DocumentationKey);
    }

    private static JsonObject CreateCollision(
        JsonObject template,
        string id,
        int code,
        string name,
        string documentationKey)
    {
        JsonObject collision = Assert.IsType<JsonObject>(template.DeepClone());
        collision["id"] = id;
        collision["code"] = code;
        collision["name"] = name;
        collision["title"] = "Collision gap sample";
        collision["documentationKey"] = documentationKey;
        return collision;
    }
}
