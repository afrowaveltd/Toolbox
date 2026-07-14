using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorTagTests
{
    [Fact]
    public async Task ErrorAddTagAsync_WithValidTag_AddsNormalizedTagAndBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        string tag = CreateUniqueTag(error);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
                workspace.ProjectRootPath,
                error.Code.ToString(),
                tag.ToLowerInvariant().Replace('_', '-'));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Contains(tag, response.Data.Tags);

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.Contains(tag, saved.Tags);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorAddTagAsync_WithDuplicateTag_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorWithTagAsync(workspace.WhenItFailsJsonsPath);
        string existingTag = error.Tags[0];
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
                workspace.ProjectRootPath,
                error.Name,
                existingTag.ToLowerInvariant());

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorTagAlreadyIncluded");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveTagAsync_WithIncludedTag_RemovesTagAndCreatesBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsWorkspaceEditor editor = new();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        string tag = CreateUniqueTag(error);

        Assert.True((await editor.ErrorAddTagAsync(
            workspace.ProjectRootPath,
            error.Id,
            tag)).IsSuccess);
        int backupsBeforeRemove = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response = await editor.ErrorRemoveTagAsync(
            workspace.ProjectRootPath,
            error.Name,
            tag.ToLowerInvariant().Replace('_', ' '));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(tag, response.Data.Tags);

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.DoesNotContain(tag, saved.Tags);
        Assert.Equal(backupsBeforeRemove + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveTagAsync_WithMissingTag_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        string missingTag = CreateUniqueTag(error);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveTagAsync(
                workspace.ProjectRootPath,
                error.Id,
                missingTag);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorTagNotIncluded");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorAddTagAsync_WithUnknownError_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "TEST_TAG");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "TEST_TAG")]
    [InlineData("AFW_NET_0001", "")]
    public async Task ErrorAddTagAsync_WithEmptyInput_ReturnsFailureWithoutBackup(
        string lookupValue,
        string tagName)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
                workspace.ProjectRootPath,
                lookupValue,
                tagName);

        Assert.False(response.IsSuccess);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    private static string CreateUniqueTag(ErrorDefinition error)
    {
        string candidate = "SETTER_TAG_TEST";
        while (error.Tags.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            candidate += "_X";
        }

        return candidate;
    }

    private static int CountBackups(string jsonsPath)
    {
        return Directory.GetFiles(
            jsonsPath,
            "errors.en.*.bak.json",
            SearchOption.TopDirectoryOnly).Length;
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate.Id)
            && !string.IsNullOrWhiteSpace(candidate.Name)
            && candidate.Code > 0);
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadFirstErrorWithTagAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            candidate.Tags.Count > 0
            && !string.IsNullOrWhiteSpace(candidate.Name));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate => string.Equals(
            candidate.Id,
            errorId,
            StringComparison.OrdinalIgnoreCase));
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
