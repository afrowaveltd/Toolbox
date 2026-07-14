using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorsCommandProfileIntegrationTests
{
    [Fact]
    public async Task ExecuteAsync_WithProfile_UsesCompleteProfileResolution()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        IReadOnlyList<ErrorDefinition> errors = await LoadErrorsAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Assert.True(errors.Count >= 2);

        ErrorDefinition includedError = errors[0];
        ErrorDefinition excludedError = errors[1];

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "CLI_PROFILE_TEST",
                "CLI Profile Test");

        Assert.True(addProfileResponse.IsSuccess);

        Response<ErrorProfileDefinition> includeFirstResponse =
            await editor.ProfileAddErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "CLI_PROFILE_TEST",
                includedError.Code.ToString());

        Assert.True(includeFirstResponse.IsSuccess);

        Response<ErrorProfileDefinition> includeSecondResponse =
            await editor.ProfileAddErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "CLI_PROFILE_TEST",
                excludedError.Name);

        Assert.True(includeSecondResponse.IsSuccess);

        Response<ErrorProfileDefinition> excludeSecondResponse =
            await editor.ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "CLI_PROFILE_TEST",
                excludedError.Id);

        Assert.True(excludeSecondResponse.IsSuccess);

        int exitCode = await ErrorsCommand.ExecuteAsync(
            [
                "errors",
                temporaryWorkspace.ProjectRootPath,
                "--profile",
                "CLI Profile Test",
                "--plain"
            ]);

        Assert.Equal(0, exitCode);

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(
                temporaryWorkspace.ProjectRootPath);

        ErrorListOptions options = ErrorsCommand.ParseErrorListOptions(
            [
                "errors",
                temporaryWorkspace.ProjectRootPath,
                "--profile",
                "CLI Profile Test",
                "--plain"
            ]);

        IReadOnlyList<ErrorDefinition> filteredErrors =
            ErrorsCommand.ApplyErrorFilters(summary, options).ToList();

        ErrorDefinition resolvedError = Assert.Single(filteredErrors);

        Assert.Equal(
            TextKeyNormalizer.NormalizeKey(includedError.Id),
            TextKeyNormalizer.NormalizeKey(resolvedError.Id));

        Assert.DoesNotContain(
            filteredErrors,
            error => string.Equals(
                error.Id,
                excludedError.Id,
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsCommandError()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ErrorsCommand.ExecuteAsync(
            [
                "errors",
                temporaryWorkspace.ProjectRootPath,
                "--profile",
                "DOES_NOT_EXIST",
                "--plain"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<IReadOnlyList<ErrorDefinition>> LoadErrorsAsync(
        string whenItFailsJsonsPath)
    {
        string errorCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "errors.en.json");

        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(errorCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        return errorCatalog.Errors
            .Where(error =>
                !string.IsNullOrWhiteSpace(error.Id)
                && !string.IsNullOrWhiteSpace(error.Name)
                && error.Code > 0)
            .Take(2)
            .ToList();
    }
}
