using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorRemoveMetadataCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        Assert.True((await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Id,
            "documentation.owner",
            "Storage Team")).IsSuccess);

        int exitCode = await ErrorRemoveMetadataCommand.ExecuteAsync(
        [
            "error-remove-metadata",
            workspace.ProjectRootPath,
            error.Name,
            "documentation-owner"
        ]);

        Assert.Equal(0, exitCode);
        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, error.Id);
        Assert.False(saved.Metadata.TryGet("DOCUMENTATION_OWNER", out _));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingMetadata_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorRemoveMetadataCommand.ExecuteAsync(
        [
            "error-remove-metadata",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-remove-metadata" },
            new[] { "error-remove-metadata", "." },
            new[] { "error-remove-metadata", ".", "AFW_NET_0001" },
            new[] { "error-remove-metadata", ".", "AFW_NET_0001", "TEAM", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorRemoveMetadataCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(string jsonsPath, string errorId)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadCatalogAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
