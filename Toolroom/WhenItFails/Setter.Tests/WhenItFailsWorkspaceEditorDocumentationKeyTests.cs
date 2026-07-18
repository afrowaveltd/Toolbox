using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests;

public sealed class WhenItFailsWorkspaceEditorDocumentationKeyTests
{
    [Fact]
    public async Task SetErrorDocumentationKeyAsync_WithInvalidFormat_DoesNotSaveOrCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument before = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition target = before.Errors.First();
        string? originalKey = target.DocumentationKey;
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorDocumentationKeyAsync(
                workspace.ProjectRootPath,
                target.Id,
                "Docs Network Interrupted");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => string.Equals(
                issue.Code,
                "InvalidDocumentationKeyFormat",
                StringComparison.Ordinal));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath))
            .Errors
            .Single(error => error.Id == target.Id);
        Assert.Equal(originalKey, saved.DocumentationKey);
    }

    [Fact]
    public async Task SetErrorDocumentationKeyAsync_WithDuplicateKey_DoesNotSaveOrCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument before = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition target = before.Errors.First();
        ErrorDefinition source = before.Errors.Skip(1).First();
        string? originalKey = target.DocumentationKey;
        string duplicateKey = Assert.IsType<string>(source.DocumentationKey);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorDocumentationKeyAsync(
                workspace.ProjectRootPath,
                target.Id,
                duplicateKey.ToUpperInvariant());

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => string.Equals(
                issue.Code,
                "DuplicateDocumentationKey",
                StringComparison.Ordinal));
        Assert.Contains(source.Id, response.Message, StringComparison.Ordinal);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath))
            .Errors
            .Single(error => error.Id == target.Id);
        Assert.Equal(originalKey, saved.DocumentationKey);
    }

    [Fact]
    public async Task SetErrorDocumentationKeyAsync_WithCanonicalFormat_SavesValue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition target = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        const string newKey = "when-it-fails/errors/network/editor-test";

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorDocumentationKeyAsync(
                workspace.ProjectRootPath,
                target.Id,
                newKey);

        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(newKey, response.Data.DocumentationKey);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath))
            .Errors
            .Single(error => error.Id == target.Id);
        Assert.Equal(newKey, saved.DocumentationKey);
    }

    [Fact]
    public async Task SetErrorDocumentationKeyAsync_WithWhitespace_ReturnsExistingEmptyValueFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition target = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorDocumentationKeyAsync(
                workspace.ProjectRootPath,
                target.Id,
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => string.Equals(
                issue.Code,
                "DocumentationKeyIsEmpty",
                StringComparison.Ordinal));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));

        Assert.True(response.IsSuccess, response.Message);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
