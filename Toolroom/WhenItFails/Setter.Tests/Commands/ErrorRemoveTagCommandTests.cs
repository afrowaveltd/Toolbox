using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorRemoveTagCommandTests
{
    private const string TestErrorId = "AFW_NET_0001";

    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesNormalizedTag()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.True((await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
            workspace.ProjectRootPath,
            TestErrorId,
            "support-visible")).IsSuccess);

        int exitCode = await ErrorRemoveTagCommand.ExecuteAsync(
        [
            "error-remove-tag",
            workspace.ProjectRootPath,
            "600001",
            " SUPPORT_VISIBLE "
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition errorDefinition = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            TestErrorId);

        Assert.DoesNotContain(
            "SUPPORT_VISIBLE",
            errorDefinition.Tags,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingTag_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ErrorRemoveTagCommand.ExecuteAsync(
        [
            "error-remove-tag",
            workspace.ProjectRootPath,
            "NETWORKUNAVAILABLE",
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-remove-tag" },
            new[] { "error-remove-tag", "." },
            new[] { "error-remove-tag", ".", TestErrorId },
            new[] { "error-remove-tag", ".", TestErrorId, "TAG", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorRemoveTagCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorDefinition? errorDefinition = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .FirstOrDefault(error => string.Equals(
                error.Id,
                errorId,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(errorDefinition);
        return errorDefinition;
    }
}
