using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorMetadataTests
{
    [Fact]
    public async Task ErrorSetMetadataAsync_ShouldPersistNormalizedMetadataAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
                workspace.ProjectRootPath,
                error.Code.ToString(),
                "documentation owner",
                "  Storage Team  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Metadata.TryGet("DOCUMENTATION_OWNER", out string? value));
        Assert.Equal("Storage Team", value);

        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, error.Id);
        Assert.True(saved.Metadata.TryGet("DOCUMENTATION_OWNER", out string? savedValue));
        Assert.Equal("Storage Team", savedValue);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorSetMetadataAsync_ShouldUpdateExistingMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsWorkspaceEditor editor = new();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        Assert.True((await editor.ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Id,
            "TEAM",
            "Storage")).IsSuccess);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response = await editor.ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Name,
            "team",
            "Diagnostics");

        Assert.True(response.IsSuccess);
        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, error.Id);
        Assert.True(saved.Metadata.TryGet("TEAM", out string? value));
        Assert.Equal("Diagnostics", value);
        Assert.Equal(1, saved.Metadata.Count);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorSetMetadataAsync_ShouldRejectSameValueWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsWorkspaceEditor editor = new();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        Assert.True((await editor.ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Id,
            "TEAM",
            "Storage")).IsSuccess);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response = await editor.ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Id,
            "team",
            "Storage");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorMetadataAlreadySet");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveMetadataAsync_ShouldRemoveMetadataAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsWorkspaceEditor editor = new();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        Assert.True((await editor.ErrorSetMetadataAsync(
            workspace.ProjectRootPath,
            error.Id,
            "documentation.owner",
            "Storage Team")).IsSuccess);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response = await editor.ErrorRemoveMetadataAsync(
            workspace.ProjectRootPath,
            error.Name,
            "documentation-owner");

        Assert.True(response.IsSuccess);
        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, error.Id);
        Assert.False(saved.Metadata.TryGet("DOCUMENTATION_OWNER", out _));
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveMetadataAsync_ShouldReturnNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveMetadataAsync(
                workspace.ProjectRootPath,
                error.Id,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorMetadataNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorSetMetadataAsync_ShouldReturnNotFoundForUnknownError()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "TEAM",
                "Storage");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "TEAM", "Storage", "ErrorLookupIsEmpty")]
    [InlineData("AFW_NET_0001", "", "Storage", "ErrorMetadataKeyIsEmpty")]
    [InlineData("AFW_NET_0001", "TEAM", "", "ErrorMetadataValueIsEmpty")]
    public async Task ErrorSetMetadataAsync_ShouldRejectEmptyValues(
        string lookupValue,
        string metadataKey,
        string metadataValue,
        string issueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
                workspace.ProjectRootPath,
                lookupValue,
                metadataKey,
                metadataValue);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == issueCode);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;

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
