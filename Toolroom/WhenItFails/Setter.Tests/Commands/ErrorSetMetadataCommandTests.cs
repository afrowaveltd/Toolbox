using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorSetMetadataCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_PersistsNormalizedMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorSetMetadataCommand.ExecuteAsync(
        [
            "error-set-metadata",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            "documentation.owner",
            "Storage Team"
        ]);

        Assert.Equal(0, exitCode);
        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, error.Id);
        Assert.True(saved.Metadata.TryGet("DOCUMENTATION_OWNER", out string? value));
        Assert.Equal("Storage Team", value);
    }

    [Fact]
    public async Task ExecuteAsync_WithSameValue_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        Assert.Equal(0, await ErrorSetMetadataCommand.ExecuteAsync(
        [
            "error-set-metadata",
            workspace.ProjectRootPath,
            error.Id,
            "TEAM",
            "Storage"
        ]));

        int exitCode = await ErrorSetMetadataCommand.ExecuteAsync(
        [
            "error-set-metadata",
            workspace.ProjectRootPath,
            error.Name,
            "team",
            "Storage"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-set-metadata" },
            new[] { "error-set-metadata", "." },
            new[] { "error-set-metadata", ".", "AFW_NET_0001" },
            new[] { "error-set-metadata", ".", "AFW_NET_0001", "TEAM" },
            new[] { "error-set-metadata", ".", "AFW_NET_0001", "TEAM", "Storage", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorSetMetadataCommand.ExecuteAsync(args));
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
