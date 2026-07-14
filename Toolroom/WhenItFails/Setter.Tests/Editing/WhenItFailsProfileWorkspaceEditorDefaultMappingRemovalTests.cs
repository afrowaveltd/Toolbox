using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorDefaultMappingRemovalTests
{
    [Fact]
    public async Task ProfileRemoveDefaultMappingAsync_WithExistingMapping_RemovesMappingAndAddsBackup()
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

        Response<ErrorProfileDefinition> setResponse =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "web.problemDetails",
                "true");

        Assert.True(setResponse.IsSuccess);

        string[] backupsBeforeRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                "WEB.PROBLEMDETAILS");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data.DefaultMappings);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Empty(savedProfile.DefaultMappings);

        string[] backupsAfterRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeRemove.Length + 1, backupsAfterRemove.Length);
    }

    [Fact]
    public async Task ProfileRemoveDefaultMappingAsync_WithMissingMapping_ReturnsNotFoundWithoutBackup()
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
            await editor.ProfileRemoveDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "web.problemDetails");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileMappingNotFound");

        string[] backupsAfterAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAttempt.Length, backupsAfterAttempt.Length);
    }

    [Fact]
    public async Task ProfileRemoveDefaultMappingAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "web.problemDetails");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "WEB.PROBLEMDETAILS")]
    [InlineData("DITA_TEST", "")]
    public async Task ProfileRemoveDefaultMappingAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string mappingKey)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                mappingKey);

        Assert.False(response.IsSuccess);
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
