using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorNameTests
{
    [Fact]
    public async Task SetNameAsync_ShouldNormalizeAndSaveNameWithoutChangingIdentity()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument before = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition selected = before.Errors.First();
        string originalId = selected.Id;
        int originalCode = selected.Code;
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetNameAsync(
                workspace.ProjectRootPath,
                selected.Id,
                "Renamed sample error");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("RENAMED_SAMPLE_ERROR", response.Data.Name);
        Assert.Equal(originalId, response.Data.Id);
        Assert.Equal(originalCode, response.Data.Code);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition? savedError = saved.Errors.FirstOrDefault(error => error.Id == originalId);
        Assert.NotNull(savedError);
        Assert.Equal("RENAMED_SAMPLE_ERROR", savedError.Name);
        Assert.Equal(originalCode, savedError.Code);
    }

    [Fact]
    public async Task SetNameAsync_ShouldRejectExistingNameWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument catalog = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition selected = catalog.Errors[0];
        ErrorDefinition existing = catalog.Errors[1];
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetNameAsync(
                workspace.ProjectRootPath,
                selected.Id,
                existing.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorNameAlreadyExists");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorCatalogDocument saved = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(selected.Name, saved.Errors.First(error => error.Id == selected.Id).Name);
    }

    [Fact]
    public async Task SetNameAsync_ShouldRejectEquivalentNormalizedNameWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition selected = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        string equivalentName = selected.Name.Replace('_', ' ').ToLowerInvariant();

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetNameAsync(
                workspace.ProjectRootPath,
                selected.Id,
                equivalentName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorNameAlreadySet");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "NEW_NAME", "ErrorLookupIsEmpty")]
    [InlineData("DOES_NOT_EXIST", "NEW_NAME", "ErrorDefinitionNotFound")]
    [InlineData("AFW_GEN_0001", "", "ErrorNameIsEmpty")]
    public async Task SetNameAsync_ShouldRejectInvalidInputWithoutBackup(
        string lookup,
        string newName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetNameAsync(
                workspace.ProjectRootPath,
                lookup,
                newName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedIssueCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
