using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterReadOnlyTests
{
    [Fact]
    public async Task SuggestAsync_WithResolvedOptions_DoesNotModifyWorkspaceFiles()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        Dictionary<string, string> filesBefore = await ReadWorkspaceFilesAsync(
            workspace.WhenItFailsJsonsPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "NETWORK",
                "Read only suggestion sample");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        Dictionary<string, string> filesAfter = await ReadWorkspaceFilesAsync(
            workspace.WhenItFailsJsonsPath);
        Assert.Equal(filesBefore.Count, filesAfter.Count);

        foreach ((string relativePath, string contentBefore) in filesBefore)
        {
            Assert.True(
                filesAfter.TryGetValue(relativePath, out string? contentAfter),
                $"Workspace file '{relativePath}' disappeared.");
            Assert.Equal(contentBefore, contentAfter);
        }
    }

    private static async Task<Dictionary<string, string>> ReadWorkspaceFilesAsync(
        string workspacePath)
    {
        Dictionary<string, string> files = new(StringComparer.Ordinal);

        foreach (string filePath in Directory.GetFiles(
            workspacePath,
            "*",
            SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(workspacePath, filePath);
            files.Add(relativePath, await File.ReadAllTextAsync(filePath));
        }

        return files;
    }
}
