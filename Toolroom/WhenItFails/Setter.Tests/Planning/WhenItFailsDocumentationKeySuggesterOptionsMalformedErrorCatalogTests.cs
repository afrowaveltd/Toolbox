using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsMalformedErrorCatalogTests
{
    [Fact]
    public async Task SuggestAsync_WithResolvedOptionsAndMalformedErrorCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        string errorCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.ErrorCatalogFileName);
        await File.WriteAllTextAsync(errorCatalogPath, "{ this is not valid json }");

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "NETWORK",
                "Malformed error catalog");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }
}
