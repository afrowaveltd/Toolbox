using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorOwnerTests
{
    [Fact]
    public async Task SetOwnerAsync_ShouldChangeOwnerAndStructuredIdAtomically()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddCompatibleOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            error.Code,
            "SETTER_OWNER");
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                workspace.ProjectRootPath,
                error.Code.ToString(),
                owner.Aliases[0].ToLowerInvariant());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(owner.Name, response.Data.Owner);
        Assert.StartsWith(
            $"{owner.Name}_{TextKeyNormalizer.NormalizeKey(error.CodePrefix)}_",
            response.Data.Id,
            StringComparison.Ordinal);

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            response.Data.Id);
        Assert.Equal(owner.Name, saved.Owner);
        Assert.Equal(response.Data.Id, saved.Id);
        Assert.Equal(error.Code, saved.Code);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetOwnerAsync_ShouldRejectOwnerOutsideCodeRangeWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "OTHER_OWNER",
            error.Code + 1,
            error.Code + 100,
            "OTHER");
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                workspace.ProjectRootPath,
                error.Id,
                owner.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorCodeOutsideOwnerRange");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetOwnerAsync_ShouldRejectCurrentOwnerWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                workspace.ProjectRootPath,
                error.Name,
                error.Owner);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "OwnerAlreadySet");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetOwnerAsync_ShouldReturnNotFoundForUnknownOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                workspace.ProjectRootPath,
                error.Id,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "OwnerNotFound");
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "AFW", "ErrorLookupIsEmpty")]
    [InlineData("AFW_NET_0001", "", "OwnerNameIsEmpty")]
    public async Task SetOwnerAsync_ShouldRejectEmptyValues(
        string lookupValue,
        string ownerName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                workspace.ProjectRootPath,
                lookupValue,
                ownerName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedIssueCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<ErrorOwnerDefinition> AddCompatibleOwnerAsync(
        string jsonsPath,
        int code,
        string ownerName)
    {
        return await AddOwnerAsync(
            jsonsPath,
            ownerName,
            code,
            code,
            "SETTER");
    }

    private static async Task<ErrorOwnerDefinition> AddOwnerAsync(
        string jsonsPath,
        string ownerName,
        int codeFrom,
        int codeTo,
        string alias)
    {
        string filePath = Path.Combine(jsonsPath, "owners.en.json");
        Response<ErrorOwnerCatalogDocument> loadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(filePath);
        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorOwnerCatalogDocument catalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorOwnerDefinition owner = new()
        {
            Name = ownerName,
            DisplayName = "Setter test owner",
            CodeFrom = codeFrom,
            CodeTo = codeTo,
            IsBuiltIn = false,
            Aliases = [alias]
        };
        catalog.Owners.Add(owner);

        Response saveResponse =
            await new JsonCatalogDocumentWriter().SaveToFileAsync(catalog, filePath);
        Assert.True(saveResponse.IsSuccess);
        return owner;
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadErrorCatalogAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
