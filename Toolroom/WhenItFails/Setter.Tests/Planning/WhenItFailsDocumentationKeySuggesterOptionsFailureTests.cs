using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsFailureTests
{
    [Fact]
    public async Task SuggestAsync_WithResolvedOptionsAndUnknownCategory_ReturnsCategoryNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "DOES_NOT_EXIST",
                "Options failure sample");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }
}
