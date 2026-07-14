using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorDefaultMappingTests
{
    [Fact]
    public async Task ProfileSetDefaultMappingAsync_WithNewMapping_AddsNormalizedMappingAndBackup()
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

        string[] backupsBeforeSet = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        const string mappingKey = " web.problemDetails ";
        string normalizedMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                mappingKey,
                " true ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("true", response.Data.DefaultMappings[normalizedMappingKey]);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Equal("true", savedProfile.DefaultMappings[normalizedMappingKey]);

        string[] backupsAfterSet = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeSet.Length + 1, backupsAfterSet.Length);
    }

    [Fact]
    public async Task ProfileSetDefaultMappingAsync_WithExistingMapping_UpdatesValueAndKeepsSingleKey()
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

        const string mappingKey = "WEB.PROBLEMDETAILS";
        string normalizedMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);

        Response<ErrorProfileDefinition> firstResponse =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                mappingKey,
                "true");

        Assert.True(firstResponse.IsSuccess);

        Response<ErrorProfileDefinition> updateResponse =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "web.problemdetails",
                "false");

        Assert.True(updateResponse.IsSuccess);
        Assert.NotNull(updateResponse.Data);
        Assert.Single(updateResponse.Data.DefaultMappings);
        Assert.Equal("false", updateResponse.Data.DefaultMappings[normalizedMappingKey]);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Single(savedProfile.DefaultMappings);
        Assert.Equal("false", savedProfile.DefaultMappings[normalizedMappingKey]);
    }

    [Fact]
    public async Task ProfileSetDefaultMappingAsync_WithSameValue_ReturnsInvalidWithoutBackup()
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

        Response<ErrorProfileDefinition> firstResponse =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "WEB.PROBLEMDETAILS",
                "true");

        Assert.True(firstResponse.IsSuccess);

        string[] backupsBeforeSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> secondResponse =
            await editor.ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "web.problemdetails",
                "true");

        Assert.False(secondResponse.IsSuccess);
        Assert.Contains(
            secondResponse.Issues,
            issue => issue.Code == "ProfileMappingAlreadySet");

        string[] backupsAfterSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(
            backupsBeforeSecondAttempt.Length,
            backupsAfterSecondAttempt.Length);
    }

    [Fact]
    public async Task ProfileSetDefaultMappingAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "WEB.PROBLEMDETAILS",
                "true");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "WEB.PROBLEMDETAILS", "true")]
    [InlineData("DITA_TEST", "", "true")]
    [InlineData("DITA_TEST", "WEB.PROBLEMDETAILS", "")]
    public async Task ProfileSetDefaultMappingAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string mappingKey,
        string mappingValue)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetDefaultMappingAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                mappingKey,
                mappingValue);

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
