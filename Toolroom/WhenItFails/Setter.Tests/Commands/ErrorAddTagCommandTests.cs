using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorAddTagCommandTests
{
    private const string TestErrorId = "AFW_NET_0001";

    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsNormalizedTag()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ErrorAddTagCommand.ExecuteAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            " support visible "
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition errorDefinition = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            TestErrorId);

        Assert.Contains(
            "SUPPORT_VISIBLE",
            errorDefinition.Tags,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateTag_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.Equal(
            0,
            await ErrorAddTagCommand.ExecuteAsync(
            [
                "error-add-tag",
                workspace.ProjectRootPath,
                TestErrorId,
                "support-visible"
            ]));

        int exitCode = await ErrorAddTagCommand.ExecuteAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            "SUPPORT_VISIBLE"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-add-tag" },
            new[] { "error-add-tag", "." },
            new[] { "error-add-tag", ".", TestErrorId },
            new[] { "error-add-tag", ".", TestErrorId, "TAG", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorAddTagCommand.ExecuteAsync(args));
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
