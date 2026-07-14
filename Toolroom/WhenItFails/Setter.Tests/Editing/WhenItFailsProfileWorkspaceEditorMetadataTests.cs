using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorMetadataTests
{
    [Fact]
    public async Task ProfileSetMetadataAsync_WithNewMetadata_AddsNormalizedMetadataAndBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        string[] backupsBeforeSet = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileSetMetadataAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                " documentation.owner ",
                " DiTa Team ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        string normalizedKey = TextKeyNormalizer.NormalizeKey("documentation.owner");
        Assert.True(response.Data.Metadata.TryGet(normalizedKey, out string? responseValue));
        Assert.Equal("DiTa Team", responseValue);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.True(savedProfile.Metadata.TryGet(normalizedKey, out string? savedValue));
        Assert.Equal("DiTa Team", savedValue);

        string[] backupsAfterSet = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeSet.Length + 1, backupsAfterSet.Length);
    }

    [Fact]
    public async Task ProfileSetMetadataAsync_WithExistingMetadata_UpdatesValueAndKeepsSingleKey()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        Assert.True((await editor.ProfileSetMetadataAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "documentation.owner",
            "DiTa Team")).IsSuccess);

        Response<ErrorProfileDefinition> updateResponse =
            await editor.ProfileSetMetadataAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOCUMENTATION-OWNER",
                "Storage Team");

        Assert.True(updateResponse.IsSuccess);
        Assert.NotNull(updateResponse.Data);
        Assert.Equal(1, updateResponse.Data.Metadata.Count);

        string normalizedKey = TextKeyNormalizer.NormalizeKey("documentation.owner");
        Assert.True(updateResponse.Data.Metadata.TryGet(normalizedKey, out string? updatedValue));
        Assert.Equal("Storage Team", updatedValue);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Equal(1, savedProfile.Metadata.Count);
        Assert.True(savedProfile.Metadata.TryGet(normalizedKey, out string? savedValue));
        Assert.Equal("Storage Team", savedValue);
    }

    [Fact]
    public async Task ProfileSetMetadataAsync_WithSameValue_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        Assert.True((await editor.ProfileSetMetadataAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "documentation.owner",
            "DiTa Team")).IsSuccess);

        string[] backupsBeforeSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> secondResponse =
            await editor.ProfileSetMetadataAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOCUMENTATION_OWNER",
                "DiTa Team");

        Assert.False(secondResponse.IsSuccess);
        Assert.Contains(
            secondResponse.Issues,
            issue => issue.Code == "ProfileMetadataAlreadySet");

        string[] backupsAfterSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(
            backupsBeforeSecondAttempt.Length,
            backupsAfterSecondAttempt.Length);
    }

    [Fact]
    public async Task ProfileSetMetadataAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetMetadataAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "documentation.owner",
                "DiTa Team");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "documentation.owner", "DiTa Team")]
    [InlineData("DITA_TEST", "", "DiTa Team")]
    [InlineData("DITA_TEST", "documentation.owner", "")]
    public async Task ProfileSetMetadataAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string metadataKey,
        string metadataValue)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetMetadataAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                metadataKey,
                metadataValue);

        Assert.False(response.IsSuccess);
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(whenItFailsJsonsPath, "profiles.json"));

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(loadResponse.Data)
            .Profiles
            .FirstOrDefault(candidate => string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
