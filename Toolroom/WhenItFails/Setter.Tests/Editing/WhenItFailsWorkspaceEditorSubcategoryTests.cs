using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorSubcategoryTests
{
    [Fact]
    public async Task ErrorAddSubcategoryAsync_WithExistingWorkspaceSubcategory_AddsCanonicalValueAndBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition target, string subcategory) =
            await FindTargetAndSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                target.Code.ToString(),
                subcategory.ToLowerInvariant().Replace('_', '-'));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Contains(subcategory, response.Data.Subcategories);

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            target.Id);
        Assert.Contains(subcategory, saved.Subcategories);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorAddSubcategoryAsync_WithDuplicateSubcategory_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error =
            await LoadFirstErrorWithSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        string existingSubcategory = error.Subcategories[0];
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                error.Name,
                existingSubcategory.ToLowerInvariant());

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorSubcategoryAlreadyIncluded");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveSubcategoryAsync_WithIncludedSubcategory_RemovesValueAndCreatesBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error =
            await LoadFirstErrorWithSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        string subcategory = error.Subcategories[0];
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveSubcategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                subcategory.ToLowerInvariant().Replace('_', ' '));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(
            response.Data.Subcategories,
            value => string.Equals(value, subcategory, StringComparison.OrdinalIgnoreCase));

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.DoesNotContain(
            saved.Subcategories,
            value => string.Equals(value, subcategory, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorRemoveSubcategoryAsync_WithMissingSubcategory_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition target, string subcategory) =
            await FindTargetAndSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveSubcategoryAsync(
                workspace.ProjectRootPath,
                target.Name,
                subcategory);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorSubcategoryNotIncluded");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorAddSubcategoryAsync_WithUnknownSubcategory_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "SubcategoryNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ErrorAddSubcategoryAsync_WithUnknownError_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition source =
            await LoadFirstErrorWithSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                source.Subcategories[0]);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "TEST_SUBCATEGORY")]
    [InlineData("AFW_NET_0001", "")]
    public async Task ErrorAddSubcategoryAsync_WithEmptyInput_ReturnsFailureWithoutBackup(
        string lookupValue,
        string subcategoryName)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                workspace.ProjectRootPath,
                lookupValue,
                subcategoryName);

        Assert.False(response.IsSuccess);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(ErrorDefinition Target, string Subcategory)> FindTargetAndSubcategoryAsync(
        string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);

        foreach (string subcategory in catalog.Errors
                     .SelectMany(error => error.Subcategories)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ErrorDefinition? target = catalog.Errors.FirstOrDefault(error =>
                !string.IsNullOrWhiteSpace(error.Id)
                && !string.IsNullOrWhiteSpace(error.Name)
                && error.Code > 0
                && !error.Subcategories.Contains(subcategory, StringComparer.OrdinalIgnoreCase));

            if (target is not null)
            {
                return (target, TextKeyNormalizer.NormalizeKey(subcategory));
            }
        }

        throw new InvalidOperationException(
            "The initialized catalog must contain a subcategory that is not assigned to every error.");
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

    private static async Task<ErrorDefinition> LoadFirstErrorWithSubcategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            candidate.Subcategories.Count > 0
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
