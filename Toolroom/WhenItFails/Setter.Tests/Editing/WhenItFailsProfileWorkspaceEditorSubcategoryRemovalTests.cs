using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorSubcategoryRemovalTests
{
    [Fact]
    public async Task ProfileRemoveSubcategoryAsync_WithIncludedSubcategory_RemovesCanonicalValue()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        string subcategory = await LoadFirstSubcategoryAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addResponse =
            await editor.ProfileAddSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                subcategory);

        Assert.True(addResponse.IsSuccess);

        string[] backupsBeforeRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> removeResponse =
            await editor.ProfileRemoveSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                subcategory.ToLowerInvariant());

        Assert.True(removeResponse.IsSuccess);
        Assert.NotNull(removeResponse.Data);
        Assert.DoesNotContain(
            TextKeyNormalizer.NormalizeKey(subcategory),
            removeResponse.Data.IncludeSubcategories);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.DoesNotContain(
            TextKeyNormalizer.NormalizeKey(subcategory),
            savedProfile.IncludeSubcategories);

        string[] backupsAfterRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeRemove.Length + 1, backupsAfterRemove.Length);
    }

    [Fact]
    public async Task ProfileRemoveSubcategoryAsync_WithSubcategoryNotIncluded_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        string subcategory = await LoadFirstSubcategoryAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        string[] backupsBeforeAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                subcategory);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileSubcategoryNotIncluded");

        string[] backupsAfterAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAttempt.Length, backupsAfterAttempt.Length);
    }

    [Fact]
    public async Task ProfileRemoveSubcategoryAsync_WithUnknownSubcategory_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        string[] backupsBeforeAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "SubcategoryNotFound");

        string[] backupsAfterAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAttempt.Length, backupsAfterAttempt.Length);
    }

    [Fact]
    public async Task ProfileRemoveSubcategoryAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string subcategory = await LoadFirstSubcategoryAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                subcategory);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "NETWORK")]
    [InlineData("DITA_TEST", "")]
    public async Task ProfileRemoveSubcategoryAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string subcategoryName)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                subcategoryName);

        Assert.False(response.IsSuccess);
    }

    private static async Task<string> LoadFirstSubcategoryAsync(
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

        string? subcategory = errorCatalog.Errors
            .SelectMany(error => error.Subcategories)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        Assert.False(string.IsNullOrWhiteSpace(subcategory));
        return subcategory;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        string profileCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "profiles.json");

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorProfileDefinition? profile = profileCatalog.Profiles.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
